namespace IBS.Infrastructure.Persistence;

/// <summary>
/// Represents an audit log entry recording entity changes.
/// </summary>
public sealed class AuditLog
{
    /// <summary>
    /// Gets or sets the audit log identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier who made the change.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the email of the user who made the change.
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// Gets or sets the action performed (Create, Update, Delete).
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type name of the entity that was changed.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the entity that was changed.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON diff of changes (old vs new values).
    /// </summary>
    public string? Changes { get; set; }

    /// <summary>
    /// Gets or sets when the change occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}
