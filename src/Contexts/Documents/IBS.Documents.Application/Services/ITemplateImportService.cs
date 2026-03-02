namespace IBS.Documents.Application.Services;

/// <summary>
/// Service that converts an uploaded PDF document into an HTML/Handlebars COI template
/// using an AI vision model.
/// </summary>
public interface ITemplateImportService
{
    /// <summary>
    /// Analyzes the given PDF bytes using a vision model and returns generated HTML/Handlebars content.
    /// The result is a preview — callers decide whether to persist it.
    /// </summary>
    /// <param name="pdfBytes">The raw bytes of the uploaded PDF.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Generated HTML with Handlebars placeholders.</returns>
    Task<string> ImportFromPdfAsync(byte[] pdfBytes, CancellationToken ct);
}
