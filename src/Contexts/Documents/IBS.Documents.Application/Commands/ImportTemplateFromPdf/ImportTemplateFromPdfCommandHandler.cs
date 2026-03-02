using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.Documents.Application.Services;

namespace IBS.Documents.Application.Commands.ImportTemplateFromPdf;

/// <summary>
/// Handles the ImportTemplateFromPdfCommand by delegating PDF analysis to the
/// ITemplateImportService and returning the generated template for user preview.
/// Does not persist the template — the user must save it explicitly.
/// </summary>
public sealed class ImportTemplateFromPdfCommandHandler(
    ITemplateImportService templateImportService) : ICommandHandler<ImportTemplateFromPdfCommand, ImportTemplateFromPdfResult>
{
    /// <inheritdoc />
    public async Task<Result<ImportTemplateFromPdfResult>> Handle(
        ImportTemplateFromPdfCommand request,
        CancellationToken cancellationToken)
    {
        if (request.PdfBytes.Length == 0)
            return Error.Validation("PDF file is empty.");

        string generatedContent;
        try
        {
            generatedContent = await templateImportService.ImportFromPdfAsync(request.PdfBytes, cancellationToken);
        }
        catch (Exception ex)
        {
            return Error.Internal($"AI import failed: {ex.Message}");
        }

        var suggestedName = Path.GetFileNameWithoutExtension(request.FileName)
            .Replace('-', ' ')
            .Replace('_', ' ');

        return new ImportTemplateFromPdfResult(generatedContent, suggestedName);
    }
}
