using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Domain.Aggregates.Client;

/// <summary>
/// Represents a communication log entry for a client.
/// </summary>
public sealed class Communication : TenantEntity
{
    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the communication type.
    /// </summary>
    public CommunicationType Type { get; private set; }

    /// <summary>
    /// Gets the subject of the communication.
    /// </summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>
    /// Gets additional notes about the communication.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets the date and time of the communication.
    /// </summary>
    public DateTimeOffset CommunicatedAt { get; private set; }

    /// <summary>
    /// Gets the user who logged this communication.
    /// </summary>
    public Guid LoggedBy { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Communication() { }

    /// <summary>
    /// Creates a new communication log entry.
    /// </summary>
    internal static Communication Create(
        Guid clientId,
        Guid tenantId,
        CommunicationType type,
        string subject,
        string? notes,
        Guid loggedBy)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required.", nameof(subject));

        return new Communication
        {
            ClientId = clientId,
            TenantId = tenantId,
            Type = type,
            Subject = subject.Trim(),
            Notes = notes?.Trim(),
            CommunicatedAt = DateTimeOffset.UtcNow,
            LoggedBy = loggedBy
        };
    }

    /// <summary>
    /// Updates the communication notes.
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkAsUpdated();
    }
}
