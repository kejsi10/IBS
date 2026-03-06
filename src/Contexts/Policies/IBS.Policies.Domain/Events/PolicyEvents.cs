using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Domain.Events;

/// <summary>
/// Event raised when a new policy is created.
/// </summary>
public sealed record PolicyCreatedEvent(
    Guid PolicyId,
    Guid TenantId,
    Guid ClientId,
    Guid CarrierId,
    string PolicyNumber,
    LineOfBusiness LineOfBusiness,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate
) : DomainEvent;

/// <summary>
/// Event raised when a policy is bound.
/// </summary>
public sealed record PolicyBoundEvent(
    Guid PolicyId,
    Guid TenantId,
    string PolicyNumber,
    decimal TotalPremium
) : DomainEvent;

/// <summary>
/// Event raised when a policy becomes active (issued).
/// </summary>
public sealed record PolicyActivatedEvent(
    Guid PolicyId,
    Guid TenantId,
    string PolicyNumber
) : DomainEvent;

/// <summary>
/// Event raised when a policy is cancelled.
/// </summary>
public sealed record PolicyCancelledEvent(
    Guid PolicyId,
    Guid TenantId,
    string PolicyNumber,
    DateOnly CancellationDate,
    string Reason,
    CancellationType CancellationType
) : DomainEvent;

/// <summary>
/// Event raised when a policy expires.
/// </summary>
public sealed record PolicyExpiredEvent(
    Guid PolicyId,
    Guid TenantId,
    string PolicyNumber,
    DateOnly ExpirationDate
) : DomainEvent;

/// <summary>
/// Event raised when a policy is renewed.
/// </summary>
public sealed record PolicyRenewedEvent(
    Guid OriginalPolicyId,
    Guid NewPolicyId,
    Guid TenantId,
    string OriginalPolicyNumber,
    string NewPolicyNumber
) : DomainEvent;

/// <summary>
/// Event raised when a coverage is added to a policy.
/// </summary>
public sealed record CoverageAddedEvent(
    Guid PolicyId,
    Guid CoverageId,
    Guid TenantId,
    string CoverageCode,
    string CoverageName,
    decimal PremiumAmount
) : DomainEvent;

/// <summary>
/// Event raised when a coverage is modified.
/// </summary>
public sealed record CoverageModifiedEvent(
    Guid PolicyId,
    Guid CoverageId,
    Guid TenantId,
    decimal OldPremium,
    decimal NewPremium
) : DomainEvent;

/// <summary>
/// Event raised when a coverage is removed from a policy.
/// </summary>
public sealed record CoverageRemovedEvent(
    Guid PolicyId,
    Guid CoverageId,
    Guid TenantId,
    string CoverageCode
) : DomainEvent;

/// <summary>
/// Event raised when an endorsement is added to a policy.
/// </summary>
public sealed record EndorsementAddedEvent(
    Guid PolicyId,
    Guid EndorsementId,
    Guid TenantId,
    string EndorsementNumber,
    DateOnly EffectiveDate,
    decimal PremiumChange
) : DomainEvent;

/// <summary>
/// Event raised when an endorsement is approved.
/// </summary>
public sealed record EndorsementApprovedEvent(
    Guid PolicyId,
    Guid EndorsementId,
    Guid TenantId,
    string EndorsementNumber
) : DomainEvent;

/// <summary>
/// Event raised when a cancelled policy is reinstated.
/// </summary>
public sealed record PolicyReinstatedEvent(
    Guid PolicyId,
    Guid TenantId,
    string PolicyNumber,
    string Reason
) : DomainEvent;

/// <summary>
/// Event raised when the policy premium changes.
/// </summary>
public sealed record PolicyPremiumChangedEvent(
    Guid PolicyId,
    Guid TenantId,
    decimal OldPremium,
    decimal NewPremium,
    string Reason
) : DomainEvent;

/// <summary>
/// Types of policy cancellation.
/// </summary>
public enum CancellationType
{
    /// <summary>Cancelled at insured's request.</summary>
    InsuredRequest,

    /// <summary>Cancelled by carrier for underwriting reasons.</summary>
    CarrierUnderwriting,

    /// <summary>Cancelled for non-payment of premium.</summary>
    NonPayment,

    /// <summary>Flat cancelled (as if never issued).</summary>
    FlatCancel,

    /// <summary>Cancelled due to material misrepresentation.</summary>
    Misrepresentation
}
