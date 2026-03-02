using MediatR;

namespace IBS.BuildingBlocks.Domain;

/// <summary>
/// Marker interface for domain events. Extends INotification for MediatR dispatch.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the unique identifier of this event occurrence.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Gets the type name of the event.
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public string EventType => GetType().Name;
}

/// <summary>
/// Interface for domain event handlers.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle.</typeparam>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles the specified domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
