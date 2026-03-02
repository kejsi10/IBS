using UglyToad.PdfPig;

namespace IBS.Documents.Infrastructure.Ai;

/// <summary>
/// Extracts text from a PDF using PdfPig (pure .NET, no native dependencies).
/// Pages are separated by "---" so the LLM can see page boundaries.
/// </summary>
public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    /// <inheritdoc />
    public string ExtractText(byte[] pdfBytes)
    {
        using var document = PdfDocument.Open(pdfBytes);
        var pages = new List<string>();

        foreach (var page in document.GetPages())
        {
            pages.Add(page.Text);
        }

        return string.Join("\n---\n", pages);
    }
}
