namespace IBS.Documents.Application.Services;

/// <summary>
/// Service that modifies an HTML/Handlebars template based on a natural language instruction
/// using an AI code model.
/// </summary>
public interface ITemplateEditingService
{
    /// <summary>
    /// Applies the given natural language instruction to the current template content
    /// and returns the modified HTML. Handlebars expressions are preserved.
    /// The result is a preview — callers decide whether to persist it.
    /// </summary>
    /// <param name="currentContent">The existing HTML/Handlebars template.</param>
    /// <param name="instruction">Natural language instruction describing the desired change.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The modified HTML with Handlebars placeholders preserved.</returns>
    Task<string> EditTemplateAsync(string currentContent, string instruction, CancellationToken ct);
}
