using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Aggregates.Policy;
using IBS.Policies.Domain.Events;
using IBS.Policies.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IBS.Policies.Application.EventHandlers;

/// <summary>
/// Records a history entry when a policy lifecycle event occurs.
/// Handles: Created, Bound, Activated, Cancelled, Reinstated, Renewed, Expired.
/// </summary>
public sealed class PolicyLifecycleHistoryHandler(
    IPolicyHistoryRepository historyRepository,
    IUnitOfWork unitOfWork,
    ILogger<PolicyLifecycleHistoryHandler> logger)
    : INotificationHandler<PolicyCreatedEvent>,
      INotificationHandler<PolicyBoundEvent>,
      INotificationHandler<PolicyActivatedEvent>,
      INotificationHandler<PolicyCancelledEvent>,
      INotificationHandler<PolicyReinstatedEvent>,
      INotificationHandler<PolicyRenewedEvent>,
      INotificationHandler<PolicyExpiredEvent>
{
    /// <inheritdoc />
    public async Task Handle(PolicyCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("Recording history for PolicyCreated {PolicyId}", notification.PolicyId);
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.Created,
            $"Policy {notification.PolicyNumber} created.");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PolicyBoundEvent notification, CancellationToken cancellationToken)
    {
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.Bound,
            $"Policy bound with total premium {notification.TotalPremium:C}.");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PolicyActivatedEvent notification, CancellationToken cancellationToken)
    {
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.Activated,
            "Policy activated (issued).");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PolicyCancelledEvent notification, CancellationToken cancellationToken)
    {
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.Cancelled,
            $"Policy cancelled effective {notification.CancellationDate:d}. Type: {notification.CancellationType}. Reason: {notification.Reason}");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PolicyReinstatedEvent notification, CancellationToken cancellationToken)
    {
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.Reinstated,
            $"Policy reinstated. Reason: {notification.Reason}");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PolicyRenewedEvent notification, CancellationToken cancellationToken)
    {
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.OriginalPolicyId,
            PolicyHistoryEventType.Renewed,
            $"Policy renewed as {notification.NewPolicyNumber}.");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PolicyExpiredEvent notification, CancellationToken cancellationToken)
    {
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.Expired,
            $"Policy expired on {notification.ExpirationDate:d}.");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Records a history entry when coverage changes occur on a policy.
/// </summary>
public sealed class CoverageHistoryHandler(
    IPolicyHistoryRepository historyRepository,
    IUnitOfWork unitOfWork)
    : INotificationHandler<CoverageAddedEvent>,
      INotificationHandler<CoverageModifiedEvent>,
      INotificationHandler<CoverageRemovedEvent>
{
    /// <inheritdoc />
    public async Task Handle(CoverageAddedEvent notification, CancellationToken cancellationToken)
    {
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.CoverageAdded,
            $"Coverage '{notification.CoverageName}' ({notification.CoverageCode}) added with premium {notification.PremiumAmount:C}.");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(CoverageModifiedEvent notification, CancellationToken cancellationToken)
    {
        var changesJson = JsonSerializer.Serialize(new
        {
            oldPremium = notification.OldPremium,
            newPremium = notification.NewPremium,
        });
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.CoverageModified,
            $"Coverage premium changed from {notification.OldPremium:C} to {notification.NewPremium:C}.",
            changesJson);
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(CoverageRemovedEvent notification, CancellationToken cancellationToken)
    {
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.CoverageRemoved,
            $"Coverage '{notification.CoverageCode}' removed.");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Records a history entry when endorsement events occur on a policy.
/// </summary>
public sealed class EndorsementHistoryHandler(
    IPolicyHistoryRepository historyRepository,
    IUnitOfWork unitOfWork)
    : INotificationHandler<EndorsementAddedEvent>,
      INotificationHandler<EndorsementApprovedEvent>
{
    /// <inheritdoc />
    public async Task Handle(EndorsementAddedEvent notification, CancellationToken cancellationToken)
    {
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.EndorsementAdded,
            $"Endorsement {notification.EndorsementNumber} added, effective {notification.EffectiveDate:d}. Premium change: {notification.PremiumChange:C}.");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(EndorsementApprovedEvent notification, CancellationToken cancellationToken)
    {
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.EndorsementApproved,
            $"Endorsement {notification.EndorsementNumber} approved.");
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Records a history entry when the policy premium changes.
/// </summary>
public sealed class PremiumChangeHistoryHandler(
    IPolicyHistoryRepository historyRepository,
    IUnitOfWork unitOfWork)
    : INotificationHandler<PolicyPremiumChangedEvent>
{
    /// <inheritdoc />
    public async Task Handle(PolicyPremiumChangedEvent notification, CancellationToken cancellationToken)
    {
        var changesJson = JsonSerializer.Serialize(new
        {
            oldPremium = notification.OldPremium,
            newPremium = notification.NewPremium,
            reason = notification.Reason,
        });
        var entry = PolicyHistory.Create(
            notification.TenantId,
            notification.PolicyId,
            PolicyHistoryEventType.PremiumChanged,
            $"Premium changed from {notification.OldPremium:C} to {notification.NewPremium:C}. Reason: {notification.Reason}",
            changesJson);
        await historyRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
