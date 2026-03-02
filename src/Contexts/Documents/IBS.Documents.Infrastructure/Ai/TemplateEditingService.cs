using IBS.Documents.Application.Services;
using Microsoft.Extensions.Options;

namespace IBS.Documents.Infrastructure.Ai;

/// <summary>
/// Applies natural language edit instructions to an HTML/Handlebars template
/// using Qwen2.5-Coder via Ollama. Handlebars expressions are preserved.
/// </summary>
public sealed class TemplateEditingService(
    IOllamaClient ollamaClient,
    IOptions<OllamaOptions> options) : ITemplateEditingService
{
    /// <inheritdoc />
    public async Task<string> EditTemplateAsync(string currentContent, string instruction, CancellationToken ct)
    {
        // Use non-interpolated raw strings for the static text (so {{}} are literal)
        // and concatenate the C# variables separately to avoid brace-escaping issues.
        var prompt =
            """
            You are an expert HTML/CSS developer specializing in insurance document templates.
            Modify the following HTML template according to the instruction below.

            IMPORTANT RULES:
            1. Preserve ALL Handlebars expressions exactly as written (e.g., {{PolicyNumber}}, {{ClientName}}, {{#each CoverageSummary}}, etc.)
            2. Return ONLY the complete modified HTML starting with <!DOCTYPE html> and ending with </html>
            3. Do not add explanations, markdown code fences, or any text outside the HTML
            4. Maintain the same overall structure unless the instruction explicitly says to change it

            CURRENT HTML TEMPLATE:
            """
            + "\n" + currentContent
            + "\n\nINSTRUCTION:\n" + instruction;

        var rawResponse = await ollamaClient.GenerateAsync(options.Value.CoderModel, prompt, ct);
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
