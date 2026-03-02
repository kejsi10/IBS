namespace IBS.Documents.Infrastructure.Ai;

/// <summary>
/// Extracts plain text from a PDF document.
/// Abstracted to allow mocking in unit tests.
/// </summary>
public interface IPdfTextExtractor
{
    /// <summary>
    /// Extracts all text from the given PDF bytes, with pages separated by "---".
    /// CPU-bound; callers should wrap in <see cref="Task.Run(System.Action)"/> if needed.
    /// </summary>
    /// <param name="pdfBytes">The raw PDF bytes.</param>
    /// <returns>Extracted text from all pages.</returns>
    string ExtractText(byte[] pdfBytes);
}
