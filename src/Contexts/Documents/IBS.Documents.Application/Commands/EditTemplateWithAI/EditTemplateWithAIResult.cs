namespace IBS.Documents.Application.Commands.EditTemplateWithAI;

/// <summary>
/// Result of an AI template edit. Contains the original and AI-modified HTML
/// so the user can compare them before deciding to apply the change.
/// </summary>
public sealed record EditTemplateWithAIResult(
    /// <summary>The template content before the AI edit.</summary>
    string OriginalContent,
    /// <summary>The template content after the AI edit.</summary>
    string ModifiedContent
);
