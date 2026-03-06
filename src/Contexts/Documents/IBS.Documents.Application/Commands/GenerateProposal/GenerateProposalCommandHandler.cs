using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Documents.Application.Services;
using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Repositories;

namespace IBS.Documents.Application.Commands.GenerateProposal;

/// <summary>
/// Handler for the GenerateProposalCommand.
/// Loads a Proposal template and quote data, renders a PDF, uploads it to blob storage,
/// and creates a Document entity linked to the quote.
/// </summary>
public sealed class GenerateProposalCommandHandler(
    IDocumentTemplateRepository templateRepository,
    IDocumentRepository documentRepository,
    IQuoteDataService quoteDataService,
    IPdfGeneratorService pdfGeneratorService,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork) : ICommandHandler<GenerateProposalCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(GenerateProposalCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null || template.TenantId != request.TenantId)
            return Error.NotFound("Template not found.");

        var quoteData = await quoteDataService.GetQuoteProposalDataAsync(request.QuoteId, cancellationToken);
        if (quoteData is null)
            return Error.NotFound("Quote not found.");

        var templateData = new ProposalTemplateData
        {
            ClientName = quoteData.ClientName,
            ClientAddress = quoteData.ClientAddress,
            LineOfBusiness = quoteData.LineOfBusiness,
            EffectiveDate = quoteData.EffectiveDate.ToString("MM/dd/yyyy"),
            ExpirationDate = quoteData.ExpirationDate.ToString("MM/dd/yyyy"),
            Notes = quoteData.Notes,
            GeneratedDate = DateTimeOffset.UtcNow.ToString("MM/dd/yyyy"),
            CarrierOffers = quoteData.CarrierOffers.Select(c => new ProposalCarrierData
            {
                CarrierName = c.CarrierName,
                Status = c.Status,
                PremiumAmount = c.PremiumAmount.HasValue
                    ? c.PremiumAmount.Value.ToString("C2")
                    : null,
                Conditions = c.Conditions,
                ProposedCoverages = c.ProposedCoverages
            }).ToList()
        };

        byte[] pdfBytes;
        try
        {
            pdfBytes = await pdfGeneratorService.GenerateAsync(template.Content, templateData, cancellationToken);
        }
        catch (Exception ex)
        {
            return Error.Internal($"Failed to generate proposal PDF: {ex.Message}");
        }

        var fileName = $"Proposal_{quoteData.ClientName.Replace(" ", "_")}_{DateTimeOffset.UtcNow:yyyyMMdd}.pdf";
        var blobKey = $"{request.TenantId}/Proposals/{request.QuoteId}/{fileName}";

        using var stream = new MemoryStream(pdfBytes);
        await blobStorageService.UploadAsync(blobKey, stream, "application/pdf", cancellationToken);

        Document document;
        try
        {
            document = Document.Create(
                request.TenantId,
                DocumentEntityType.Quote,
                request.QuoteId,
                fileName,
                "application/pdf",
                pdfBytes.Length,
                blobKey,
                DocumentCategory.Proposal,
                request.UserId,
                $"Proposal for {quoteData.ClientName} — {quoteData.LineOfBusiness}");
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
