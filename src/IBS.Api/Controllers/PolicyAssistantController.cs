using IBS.PolicyAssistant.Application.Commands.AbandonConversation;
using IBS.PolicyAssistant.Application.Commands.CreateConversation;
using IBS.PolicyAssistant.Application.Commands.CreatePolicyFromConversation;
using IBS.PolicyAssistant.Application.Commands.ImportReferenceDocument;
using IBS.PolicyAssistant.Application.Commands.SendMessage;
using IBS.PolicyAssistant.Application.Queries.GetConversation;
using IBS.PolicyAssistant.Application.Queries.GetConversations;
using IBS.PolicyAssistant.Application.Queries.SearchReferenceDocuments;
using IBS.PolicyAssistant.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for the AI-powered policy assistant chat feature.
/// Supports guided and freeform policy creation through natural language.
/// </summary>
[Authorize]
[Route("api/v1/policy-assistant")]
public sealed class PolicyAssistantController(IMediator mediator) : ApiControllerBase
{
    // ─── Conversations ──────────────────────────────────────────────────────────

    /// <summary>
    /// Lists all conversations for the current user.
    /// </summary>
    [HttpGet("conversations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversations(CancellationToken cancellationToken)
    {
        var query = new GetConversationsQuery(CurrentTenantId, CurrentUserId);
        var result = await mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a specific conversation with all its messages.
    /// </summary>
    [HttpGet("conversations/{id:guid}", Name = "GetConversationById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversation(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetConversationQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Starts a new policy assistant conversation.
    /// </summary>
    [HttpPost("conversations")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateConversation(
        [FromBody] CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        var title = string.IsNullOrWhiteSpace(request.Title)
            ? $"{request.Mode} Conversation — {DateTime.UtcNow:MMM d, yyyy h:mm tt} UTC"
            : request.Title;

        var command = new CreateConversationCommand(
            CurrentTenantId,
            CurrentUserId,
            title,
            request.Mode,
            request.LineOfBusiness);

        var result = await mediator.Send(command, cancellationToken);
        return ToCreatedResult(result, "GetConversationById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Sends a user message and returns the AI response with extracted policy data and validation.
    /// </summary>
    [HttpPost("conversations/{id:guid}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SendMessageCommand(id, CurrentTenantId, CurrentUserId, request.Content);
        var result = await mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a policy from the extracted conversation data.
    /// </summary>
    [HttpPost("conversations/{id:guid}/create-policy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePolicy(
        Guid id,
        [FromBody] CreatePolicyFromConversationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePolicyFromConversationCommand(
            id,
            CurrentTenantId,
            CurrentUserId,
            request.ClientId,
            request.CarrierId,
            request.LineOfBusiness,
            request.EffectiveDate,
            request.ExpirationDate,
            request.BillingType,
            request.PaymentPlan);

        var result = await mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { policyId = result.Value });
    }

    /// <summary>
    /// Abandons a conversation without creating a policy.
    /// </summary>
    [HttpDelete("conversations/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AbandonConversation(Guid id, CancellationToken cancellationToken)
    {
        var command = new AbandonConversationCommand(id, CurrentTenantId, CurrentUserId);
        var result = await mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    // ─── Reference Documents ─────────────────────────────────────────────────────

    /// <summary>
    /// Lists reference documents (admin only).
    /// </summary>
    [HttpGet("reference-documents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReferenceDocuments(
        [FromQuery] DocumentCategory? category,
        CancellationToken cancellationToken)
    {
        var query = new SearchReferenceDocumentsQuery(category);
        var result = await mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Imports a new reference document for AI-assisted validation (admin only).
    /// </summary>
    [HttpPost("reference-documents")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportReferenceDocument(
        [FromBody] ImportReferenceDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ImportReferenceDocumentCommand(
            request.TenantId,
            request.Title,
            request.Category,
            request.Content,
            request.LineOfBusiness,
            request.State,
            request.Source);

        var result = await mediator.Send(command, cancellationToken);
        return ToCreatedResult(result, "GetConversationById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────

/// <summary>
/// Request to start a new conversation.
/// </summary>
/// <param name="Mode">The conversation mode.</param>
/// <param name="Title">Optional title; auto-generated if omitted.</param>
/// <param name="LineOfBusiness">Optional initial line of business.</param>
public sealed record CreateConversationRequest(
    ConversationMode Mode,
    string? Title = null,
    string? LineOfBusiness = null);

/// <summary>
/// Request to send a message.
/// </summary>
/// <param name="Content">The message content.</param>
public sealed record SendMessageRequest(string Content);

/// <summary>
/// Request to create a policy from a conversation.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="CarrierId">The carrier identifier.</param>
/// <param name="LineOfBusiness">User-confirmed line of business (overrides extracted value).</param>
/// <param name="EffectiveDate">User-confirmed effective date (overrides extracted value).</param>
/// <param name="ExpirationDate">User-confirmed expiration date (overrides extracted value).</param>
/// <param name="BillingType">User-confirmed billing type (overrides extracted value).</param>
/// <param name="PaymentPlan">User-confirmed payment plan (overrides extracted value).</param>
public sealed record CreatePolicyFromConversationRequest(
    Guid ClientId,
    Guid CarrierId,
    string? LineOfBusiness = null,
    string? EffectiveDate = null,
    string? ExpirationDate = null,
    string? BillingType = null,
    string? PaymentPlan = null);

/// <summary>
/// Request to import a reference document.
/// </summary>
public sealed record ImportReferenceDocumentRequest(
    Guid? TenantId,
    string Title,
    DocumentCategory Category,
    string Content,
    string? LineOfBusiness = null,
    string? State = null,
    string? Source = null);
