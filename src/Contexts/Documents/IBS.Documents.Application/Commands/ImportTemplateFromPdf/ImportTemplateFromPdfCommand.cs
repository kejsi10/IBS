using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Documents.Application.Commands.ImportTemplateFromPdf;

/// <summary>
/// Command to import a COI template from an uploaded PDF using AI vision analysis.
/// Returns a generated HTML/Handlebars template for preview without saving to the database.
/// </summary>
public sealed record ImportTemplateFromPdfCommand(
    Guid TenantId,
    string UserId,
    string FileName,
    byte[] PdfBytes
) : ICommand<ImportTemplateFromPdfResult>;
