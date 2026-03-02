using IBS.Documents.Application.Services;
using Microsoft.Extensions.Options;

namespace IBS.Documents.Infrastructure.Ai;

/// <summary>
/// Converts an uploaded PDF into an HTML/Handlebars COI template using qwen2.5-coder via Ollama.
/// Extracts text from the PDF with PdfPig and sends it to the coder model with a structured prompt.
/// </summary>
public sealed class TemplateImportService(
    IOllamaClient ollamaClient,
    IPdfTextExtractor pdfTextExtractor,
    IOptions<OllamaOptions> options) : ITemplateImportService
{
    private const string PromptPrefix =
        """
        You are an HTML template generator for insurance documents.
        Analyze the following PDF text extracted from a Certificate of Insurance and generate a complete HTML/Handlebars template.

        Use EXACTLY these Handlebars placeholders where the corresponding data appears:
        - {{PolicyNumber}} - policy number
        - {{ClientName}} - insured/client name
        - {{CarrierName}} - insurance carrier name
        - {{EffectiveDate}} - policy effective date
        - {{ExpirationDate}} - policy expiration date
        - {{LineOfBusiness}} - line of business / insurance type
        - {{#each CoverageSummary}}<li>{{this}}</li>{{/each}} - coverage list items
        - {{BrokerName}} - broker/agent name
        - {{IssuedDate}} - certificate issue date

        Requirements:
        - Output a complete, self-contained HTML document with embedded CSS
        - Replicate the layout, structure, fonts and colors suggested by the document text
        - Use tables for tabular data, proper semantic headings
        - Make it print-ready (US Letter / A4 size with appropriate margins)
        - Include a professional header, policy section, coverage section, broker section, and disclaimer footer

        Return ONLY the HTML code starting with <!DOCTYPE html> and ending with </html>.
        Do not include any explanation, markdown code fences, or text outside the HTML.

        PDF TEXT:
        """;

    /// <inheritdoc />
    public async Task<string> ImportFromPdfAsync(byte[] pdfBytes, CancellationToken ct)
    {
        var text = await Task.Run(() => pdfTextExtractor.ExtractText(pdfBytes), ct);

        var prompt = PromptPrefix + "\n" + text;

        var rawResponse = await ollamaClient.GenerateAsync(
            options.Value.CoderModel,
            prompt,
            ct);

        return ExtractHtml(rawResponse);
    }

    /// <summary>Extracts clean HTML from the model response, stripping any markdown fences.</summary>
    public static string ExtractHtml(string raw)
    {
        var startIndex = raw.IndexOf("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0)
            startIndex = raw.IndexOf("<html", StringComparison.OrdinalIgnoreCase);

        var endIndex = raw.LastIndexOf("</html>", StringComparison.OrdinalIgnoreCase);

        if (startIndex >= 0 && endIndex > startIndex)
            return raw.Substring(startIndex, endIndex - startIndex + "</html>".Length).Trim();

        return raw.Trim();
    }
}
