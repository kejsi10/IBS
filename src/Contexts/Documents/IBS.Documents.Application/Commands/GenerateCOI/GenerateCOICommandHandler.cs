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
    IPdfGeneratorService pdfGeneratorService,
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

        // Build an anonymous object with pre-formatted dates for Handlebars rendering
        var templateData = new
        {
            policyData.PolicyNumber,
            policyData.ClientName,
            policyData.CarrierName,
            EffectiveDate = policyData.EffectiveDate.ToString("MM/dd/yyyy"),
            ExpirationDate = policyData.ExpirationDate.ToString("MM/dd/yyyy"),
            policyData.LineOfBusiness,
            policyData.CoverageSummary,
            BrokerName = "IBS Brokerage",
            IssuedDate = DateTimeOffset.UtcNow.ToString("MM/dd/yyyy")
        };

        byte[] pdfBytes;
        try
        {
            pdfBytes = await pdfGeneratorService.GenerateAsync(template.Content, templateData, cancellationToken);
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
