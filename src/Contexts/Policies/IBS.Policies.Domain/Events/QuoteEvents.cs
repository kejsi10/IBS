using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Domain.Events;

/// <summary>
/// Event raised when a new quote is created.
/// </summary>
public sealed record QuoteCreatedEvent(
    Guid QuoteId,
    Guid TenantId,
    Guid ClientId,
    LineOfBusiness LineOfBusiness,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate
) : DomainEvent;

/// <summary>
/// Event raised when a quote is submitted to carriers.
/// </summary>
public sealed record QuoteSubmittedEvent(
    Guid QuoteId,
    Guid TenantId,
    IReadOnlyList<Guid> CarrierIds
) : DomainEvent;

/// <summary>
/// Event raised when a carrier responds to a quote request.
/// </summary>
public sealed record QuoteResponseReceivedEvent(
    Guid QuoteId,
    Guid TenantId,
    Guid CarrierId,
    QuoteCarrierStatus ResponseStatus,
    decimal? PremiumAmount
) : DomainEvent;

/// <summary>
/// Event raised when a carrier's quote is accepted and a policy is created.
/// </summary>
public sealed record QuoteAcceptedEvent(
    Guid QuoteId,
    Guid TenantId,
    Guid CarrierId,
    Guid PolicyId,
    decimal PremiumAmount
) : DomainEvent;

/// <summary>
/// Event raised when a quote is cancelled.
/// </summary>
public sealed record QuoteCancelledEvent(
    Guid QuoteId,
    Guid TenantId
) : DomainEvent;

/// <summary>
/// Event raised when a quote expires.
/// </summary>
public sealed record QuoteExpiredEvent(
    Guid QuoteId,
    Guid TenantId,
    DateOnly ExpiresAt
) : DomainEvent;
