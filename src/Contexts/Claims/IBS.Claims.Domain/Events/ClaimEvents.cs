using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Domain.Events;

/// <summary>
/// Event raised when a new claim is created (FNOL).
/// </summary>
public sealed record ClaimCreatedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber,
    Guid PolicyId,
    Guid ClientId,
    LossType LossType,
    DateTimeOffset LossDate
) : DomainEvent;

/// <summary>
/// Event raised when a claim is acknowledged.
/// </summary>
public sealed record ClaimAcknowledgedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber
) : DomainEvent;

/// <summary>
/// Event raised when an adjuster is assigned to a claim.
/// </summary>
public sealed record AdjusterAssignedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber,
    string AdjusterId
) : DomainEvent;

/// <summary>
/// Event raised when a claim investigation is started.
/// </summary>
public sealed record InvestigationStartedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber
) : DomainEvent;

/// <summary>
/// Event raised when a claim is evaluated.
/// </summary>
public sealed record ClaimEvaluatedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber
) : DomainEvent;

/// <summary>
/// Event raised when a claim is approved.
/// </summary>
public sealed record ClaimApprovedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber,
    decimal ApprovedAmount,
    string Currency
) : DomainEvent;

/// <summary>
/// Event raised when a claim is denied.
/// </summary>
public sealed record ClaimDeniedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber,
    string Reason
) : DomainEvent;

/// <summary>
/// Event raised when a payment is authorized on a claim.
/// </summary>
public sealed record PaymentAuthorizedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber,
    Guid PaymentId,
    decimal Amount,
    string Currency
) : DomainEvent;

/// <summary>
/// Event raised when a payment is issued on a claim.
/// </summary>
public sealed record PaymentIssuedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber,
    Guid PaymentId
) : DomainEvent;

/// <summary>
/// Event raised when a claim is closed.
/// </summary>
public sealed record ClaimClosedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber,
    string ClosureReason
) : DomainEvent;

/// <summary>
/// Event raised when a claim is reopened.
/// </summary>
public sealed record ClaimReopenedEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber
) : DomainEvent;

/// <summary>
/// Event raised when a reserve is set on a claim.
/// </summary>
public sealed record ReserveSetEvent(
    Guid ClaimId,
    Guid TenantId,
    string ClaimNumber,
    string ReserveType,
    decimal Amount,
    string Currency
) : DomainEvent;
