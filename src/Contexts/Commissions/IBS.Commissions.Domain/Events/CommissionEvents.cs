using IBS.BuildingBlocks.Domain;

namespace IBS.Commissions.Domain.Events;

/// <summary>
/// Event raised when a commission schedule is created.
/// </summary>
public sealed record ScheduleCreatedEvent(
    Guid ScheduleId,
    Guid TenantId,
    Guid CarrierId,
    string CarrierName,
    string LineOfBusiness
) : DomainEvent;

/// <summary>
/// Event raised when a commission statement is received.
/// </summary>
public sealed record StatementReceivedEvent(
    Guid StatementId,
    Guid TenantId,
    Guid CarrierId,
    string CarrierName,
    string StatementNumber,
    int PeriodMonth,
    int PeriodYear
) : DomainEvent;

/// <summary>
/// Event raised when a line item is reconciled.
/// </summary>
public sealed record LineItemReconciledEvent(
    Guid StatementId,
    Guid TenantId,
    Guid LineItemId,
    string PolicyNumber
) : DomainEvent;

/// <summary>
/// Event raised when a line item is disputed.
/// </summary>
public sealed record LineItemDisputedEvent(
    Guid StatementId,
    Guid TenantId,
    Guid LineItemId,
    string PolicyNumber,
    string Reason
) : DomainEvent;

/// <summary>
/// Event raised when a statement is fully reconciled.
/// </summary>
public sealed record StatementReconciledEvent(
    Guid StatementId,
    Guid TenantId,
    string StatementNumber
) : DomainEvent;

/// <summary>
/// Event raised when a statement is paid.
/// </summary>
public sealed record StatementPaidEvent(
    Guid StatementId,
    Guid TenantId,
    string StatementNumber,
    decimal Amount,
    string Currency
) : DomainEvent;

/// <summary>
/// Event raised when a producer credit is assigned to a line item.
/// </summary>
public sealed record ProducerCreditAssignedEvent(
    Guid StatementId,
    Guid TenantId,
    Guid LineItemId,
    Guid ProducerId,
    string ProducerName,
    decimal SplitPercentage
) : DomainEvent;
