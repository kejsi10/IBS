namespace IBS.Documents.Application.Commands.ImportTemplateFromPdf;

/// <summary>
/// Result of an AI-powered PDF template import. Contains the generated HTML content
/// and a suggested template name derived from the source filename.
/// </summary>
public sealed record ImportTemplateFromPdfResult(
    /// <summary>The generated HTML/Handlebars template content.</summary>
    string GeneratedContent,
    /// <summary>A suggested name for the new template, based on the uploaded file name.</summary>
    string SuggestedName
);
