using System.ClientModel;
using IBS.Documents.Application.Services;
using Microsoft.Extensions.Options;
using OpenAI;

namespace IBS.Documents.Infrastructure.Ai;

/// <summary>
/// Azure OpenAI implementation of <see cref="ITemplateImportService"/>.
/// Extracts text from PDFs using <see cref="IPdfTextExtractor"/> (PdfPig — provider-agnostic)
/// and generates an HTML/Handlebars template via Azure OpenAI instead of Ollama.
/// Configuration is read from <see cref="AzureOpenAIOptions"/>.
/// </summary>
public sealed class AzureOpenAITemplateImportService(
    IPdfTextExtractor pdfTextExtractor,
    IOptions<AzureOpenAIOptions> options) : ITemplateImportService
{
    private readonly AzureOpenAIOptions _options = options.Value;

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

        var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(_options.Endpoint) };
        var client = new OpenAIClient(new ApiKeyCredential(_options.ApiKey), clientOptions);

        var chatClient = client.GetChatClient(_options.DeploymentName);

        var response = await chatClient.CompleteChatAsync(
            [OpenAI.Chat.ChatMessage.CreateUserMessage(prompt)],
            cancellationToken: ct);

        return ExtractHtml(response.Value.Content[0].Text);
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
