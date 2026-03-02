using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Documents.Application.Services;
using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Repositories;

namespace IBS.Documents.Application.Commands.GenerateCOI;

/// <summary>
/// Handler for the GenerateCOICommand.
/// </summary>
public sealed class GenerateCOICommandHandler(
    IDocumentTemplateRepository templateRepository,
    IDocumentRepository documentRepository,
    IPolicyDataService policyDataService,
    ICOIGeneratorService coiGeneratorService,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork) : ICommandHandler<GenerateCOICommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(GenerateCOICommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null || template.TenantId != request.TenantId)
            return Error.NotFound("Template not found.");

        var policyData = await policyDataService.GetPolicyCOIDataAsync(request.PolicyId, cancellationToken);
        if (policyData is null)
            return Error.NotFound("Policy not found.");

        var templateData = new COITemplateData
        {
            PolicyNumber = policyData.PolicyNumber,
            ClientName = policyData.ClientName,
            CarrierName = policyData.CarrierName,
            EffectiveDate = policyData.EffectiveDate,
            ExpirationDate = policyData.ExpirationDate,
            LineOfBusiness = policyData.LineOfBusiness,
            CoverageSummary = policyData.CoverageSummary,
            BrokerName = "IBS Brokerage",
            IssuedDate = DateTimeOffset.UtcNow
        };

        byte[] pdfBytes;
        try
        {
            pdfBytes = await coiGeneratorService.GenerateAsync(template.Content, templateData, cancellationToken);
        }
        catch (Exception ex)
        {
            return Error.Internal($"Failed to generate COI PDF: {ex.Message}");
        }

        var fileName = $"COI_{policyData.PolicyNumber}_{DateTimeOffset.UtcNow:yyyyMMdd}.pdf";
        var blobKey = $"{request.TenantId}/COI/{request.PolicyId}/{fileName}";

        using var stream = new MemoryStream(pdfBytes);
        await blobStorageService.UploadAsync(blobKey, stream, "application/pdf", cancellationToken);

        Document document;
        try
        {
            document = Document.Create(
                request.TenantId,
                DocumentEntityType.Policy,
                request.PolicyId,
                fileName,
                "application/pdf",
                pdfBytes.Length,
                blobKey,
                DocumentCategory.COI,
                request.UserId,
                $"COI for policy {policyData.PolicyNumber}");
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        await documentRepository.AddAsync(document, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return document.Id;
    }
}
