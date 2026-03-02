using IBS.BuildingBlocks.Application.Commands;

namespace IBS.PolicyAssistant.Application.Commands.CreatePolicyFromConversation;

/// <summary>
/// Command to create a policy from the extracted data in a conversation.
/// Dispatches existing Policies context commands via MediatR.
/// </summary>
/// <param name="ConversationId">The conversation identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The user identifier.</param>
/// <param name="ClientId">The client identifier (selected by user).</param>
/// <param name="CarrierId">The carrier identifier (selected by user).</param>
/// <param name="LineOfBusiness">User-confirmed line of business (overrides extracted value).</param>
/// <param name="EffectiveDate">User-confirmed effective date string (overrides extracted value).</param>
/// <param name="ExpirationDate">User-confirmed expiration date string (overrides extracted value).</param>
/// <param name="BillingType">User-confirmed billing type (overrides extracted value).</param>
/// <param name="PaymentPlan">User-confirmed payment plan (overrides extracted value).</param>
public sealed record CreatePolicyFromConversationCommand(
    Guid ConversationId,
    Guid TenantId,
    Guid UserId,
    Guid ClientId,
    Guid CarrierId,
    string? LineOfBusiness = null,
    string? EffectiveDate = null,
    string? ExpirationDate = null,
    string? BillingType = null,
    string? PaymentPlan = null) : ICommand<Guid>;
