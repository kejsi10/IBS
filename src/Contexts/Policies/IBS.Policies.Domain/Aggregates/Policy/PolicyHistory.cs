using IBS.BuildingBlocks.Domain;

namespace IBS.Policies.Domain.Aggregates.Policy;

/// <summary>
/// Records a single event in a policy's audit history.
/// This is a child entity — it does not raise domain events of its own.
/// </summary>
public sealed class PolicyHistory : TenantEntity
{
    /// <summary>
    /// Gets the policy identifier this history entry belongs to.
    /// </summary>
    public Guid PolicyId { get; private set; }

    /// <summary>
    /// Gets the type of event that occurred.
    /// </summary>
    public PolicyHistoryEventType EventType { get; private set; }

    /// <summary>
    /// Gets a human-readable description of the event.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets optional JSON payload describing the before/after changes.
    /// </summary>
    public string? ChangesJson { get; private set; }

    /// <summary>
    /// Gets the user who triggered this event, if known.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private PolicyHistory() { }

    /// <summary>
    /// Creates a new policy history entry.
    /// </summary>
    public static PolicyHistory Create(
        Guid tenantId,
        Guid policyId,
        PolicyHistoryEventType eventType,
        string description,
        string? changesJson = null,
        Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        return new PolicyHistory
        {
            TenantId = tenantId,
            PolicyId = policyId,
            EventType = eventType,
            Description = description.Trim(),
            ChangesJson = changesJson,
            UserId = userId,
        };
    }
}
