using IBS.Documents.Application.Commands.ActivateDocumentTemplate;
using IBS.Documents.Application.Commands.CreateDocumentTemplate;
using IBS.Documents.Application.Commands.DeactivateDocumentTemplate;
using IBS.Documents.Application.Commands.DeleteDocument;
using IBS.Documents.Application.Commands.EditTemplateWithAI;
using IBS.Documents.Application.Commands.GenerateCOI;
using IBS.Documents.Application.Commands.GenerateProposal;
using IBS.Documents.Application.Commands.ImportTemplateFromPdf;
using IBS.Documents.Application.Commands.UpdateDocumentTemplate;
using IBS.Documents.Application.Commands.UploadDocument;
using IBS.Documents.Application.Queries.GetDocumentById;
using IBS.Documents.Application.Queries.GetDocumentDownloadUrl;
using IBS.Documents.Application.Queries.GetDocuments;
using IBS.Documents.Application.Queries.GetDocumentTemplateById;
using IBS.Documents.Application.Queries.GetDocumentTemplates;
using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Aggregates.DocumentTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for managing documents and document templates.
/// </summary>
[Authorize]
public sealed class DocumentsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DocumentsController> _logger;

    /// <summary>
    /// Initializes a new instance of the DocumentsController class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public DocumentsController(IMediator mediator, ILogger<DocumentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a new document.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="entityType">The type of entity the document is linked to.</param>
    /// <param name="entityId">The identifier of the linked entity (optional).</param>
    /// <param name="category">The document category.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new document identifier.</returns>
    /// <response code="201">Document uploaded successfully.</response>
    /// <response code="400">If the upload data is invalid.</response>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(
        IFormFile file,
        [FromForm] DocumentEntityType entityType,
        [FromForm] Guid? entityId,
        [FromForm] DocumentCategory category,
        [FromForm] string? description,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");

        var command = new UploadDocumentCommand(
            CurrentTenantId,
            CurrentUserId.ToString(),
            entityType,
            entityId,
            file.FileName,
            file.ContentType,
            file.Length,
            file.OpenReadStream(),
            category,
            description);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess) return ToActionResult(result);
        return CreatedAtAction(nameof(GetDocument), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Gets a paginated list of documents with optional filtering.
    /// </summary>
    /// <param name="searchTerm">Optional search term.</param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="entityType">Optional entity type filter.</param>
    /// <param name="entityId">Optional entity ID filter.</param>
    /// <param name="includeArchived">Whether to include archived documents.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of documents.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] string? searchTerm,
        [FromQuery] DocumentCategory? category,
        [FromQuery] DocumentEntityType? entityType,
        [FromQuery] Guid? entityId,
        [FromQuery] bool includeArchived = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDocumentsQuery(
            CurrentTenantId,
            searchTerm,
            category,
            entityType,
            entityId,
            includeArchived,
            page,
            pageSize);

        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a document by its identifier.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The document metadata.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetDocumentByIdQuery(CurrentTenantId, id);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Returns a temporary download URL for the document.
    /// The client is responsible for opening the URL directly (e.g. in a new tab).
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The temporary download URL as a JSON string.</returns>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadDocument(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetDocumentDownloadUrlQuery(CurrentTenantId, id);
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(result.Value!);
    }

    /// <summary>
    /// Archives a document (soft delete).
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteDocumentCommand(CurrentTenantId, id);
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    // ─── Templates ───────────────────────────────────────────────────────────

    /// <summary>
    /// Gets a paginated list of document templates.
    /// </summary>
    /// <param name="searchTerm">Optional search term.</param>
    /// <param name="templateType">Optional template type filter.</param>
    /// <param name="isActive">Optional active status filter.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of document templates.</returns>
    [HttpGet("templates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] string? searchTerm,
        [FromQuery] TemplateType? templateType,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDocumentTemplatesQuery(CurrentTenantId, searchTerm, templateType, isActive, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new document template.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new template identifier.</returns>
    [HttpPost("templates")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate(
        [FromBody] CreateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDocumentTemplateCommand(
            CurrentTenantId,
            CurrentUserId.ToString(),
            request.Name,
            request.Description ?? string.Empty,
            request.TemplateType,
            request.Content);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess) return ToActionResult(result);
        return CreatedAtAction(nameof(GetTemplate), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Gets a document template by its identifier.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The document template.</returns>
    [HttpGet("templates/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetDocumentTemplateByIdQuery(CurrentTenantId, id);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Updates a document template.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("templates/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplate(
        Guid id,
        [FromBody] UpdateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDocumentTemplateCommand(
            CurrentTenantId,
            id,
            request.Name,
            request.Description ?? string.Empty,
            request.Content);

        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Activates a document template.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("templates/{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateTemplate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ActivateDocumentTemplateCommand(CurrentTenantId, id), cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Deactivates a document template.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("templates/{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateTemplate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeactivateDocumentTemplateCommand(CurrentTenantId, id), cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Imports a COI template from an uploaded PDF using AI vision analysis.
    /// Returns generated HTML/Handlebars content for preview — does not save the template.
    /// </summary>
    /// <param name="file">The PDF file to analyze.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Generated template content and a suggested name.</returns>
    /// <response code="200">Template generated successfully; review before saving.</response>
    /// <response code="400">File missing or invalid.</response>
    [HttpPost("templates/import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportTemplateFromPdfResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportTemplateFromPdf(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("PDF file is required.");

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only PDF files are supported.");

        // Verify %PDF magic bytes (0x25 0x50 0x44 0x46) to prevent content-type spoofing
        var header = new byte[4];
        using (var peek = file.OpenReadStream())
        {
            if (await peek.ReadAsync(header.AsMemory(0, 4)) < 4
                || header[0] != 0x25 || header[1] != 0x50 || header[2] != 0x44 || header[3] != 0x46)
            {
                return BadRequest("File does not appear to be a valid PDF.");
            }
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);

        var command = new ImportTemplateFromPdfCommand(
            CurrentTenantId,
            CurrentUserId.ToString(),
            file.FileName,
            ms.ToArray());

        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Applies a natural language instruction to an existing template using an AI model.
    /// Returns the original and modified HTML for review — does not save the change.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="request">The AI edit request containing the instruction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Original and modified template content.</returns>
    /// <response code="200">AI edit successful; review before applying.</response>
    /// <response code="404">Template not found.</response>
    [HttpPost("templates/{id:guid}/ai-edit")]
    [ProducesResponseType(typeof(EditTemplateWithAIResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AIEditTemplate(
        Guid id,
        [FromBody] AIEditTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EditTemplateWithAICommand(CurrentTenantId, id, request.Instruction);
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Generates a Certificate of Insurance (COI) PDF and stores it as a document.
    /// </summary>
    /// <param name="request">The COI generation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated document identifier.</returns>
    [HttpPost("generate-coi")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateCOI(
        [FromBody] GenerateCOIRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GenerateCOICommand(
            CurrentTenantId,
            CurrentUserId.ToString(),
            request.TemplateId,
            request.PolicyId);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess) return ToActionResult(result);
        return CreatedAtAction(nameof(GetDocument), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Generates a proposal PDF from a quote and stores it as a document.
    /// </summary>
    /// <param name="request">The proposal generation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated document identifier.</returns>
    [HttpPost("generate-proposal")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateProposal(
        [FromBody] GenerateProposalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GenerateProposalCommand(
            CurrentTenantId,
            CurrentUserId.ToString(),
            request.TemplateId,
            request.QuoteId);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess) return ToActionResult(result);
        return CreatedAtAction(nameof(GetDocument), new { id = result.Value }, new { id = result.Value });
    }
}

/// <summary>
/// Request model for creating a document template.
/// </summary>
public sealed record CreateTemplateRequest(
    string Name,
    string? Description,
    TemplateType TemplateType,
    string Content);

/// <summary>
/// Request model for updating a document template.
/// </summary>
public sealed record UpdateTemplateRequest(
    string Name,
    string? Description,
    string Content);

/// <summary>
/// Request model for generating a COI.
/// </summary>
public sealed record GenerateCOIRequest(
    Guid TemplateId,
    Guid PolicyId);

/// <summary>
/// Request model for AI template editing.
/// </summary>
/// <param name="Instruction">Natural language instruction describing the desired template modification.</param>
public sealed record AIEditTemplateRequest(string Instruction);

/// <summary>
/// Request model for generating a proposal PDF from a quote.
/// </summary>
/// <param name="TemplateId">The proposal template identifier.</param>
/// <param name="QuoteId">The quote identifier to generate the proposal for.</param>
public sealed record GenerateProposalRequest(
    Guid TemplateId,
    Guid QuoteId);
