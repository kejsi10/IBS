namespace IBS.BuildingBlocks.Domain;

/// <summary>
/// Base class for domain exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Gets the error code for this exception.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    protected DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with an inner exception.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleViolationException"/> class.
    /// </summary>
    /// <param name="message">The error message describing the rule violation.</param>
    public BusinessRuleViolationException(string message)
        : base("BUSINESS_RULE_VIOLATION", message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleViolationException"/> class with a code.
    /// </summary>
    /// <param name="code">The specific error code.</param>
    /// <param name="message">The error message describing the rule violation.</param>
    public BusinessRuleViolationException(string code, string message)
        : base(code, message)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    /// <summary>
    /// Gets the type of entity that was not found.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the identifier of the entity that was not found.
    /// </summary>
    public object EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="entityType">The type of entity that was not found.</param>
    /// <param name="entityId">The identifier of the entity.</param>
    public EntityNotFoundException(string entityType, object entityId)
        : base("ENTITY_NOT_FOUND", $"{entityType} with id '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Creates a new EntityNotFoundException for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="id">The entity identifier.</param>
    /// <returns>A new EntityNotFoundException.</returns>
    public static EntityNotFoundException For<TEntity>(object id)
    {
        return new EntityNotFoundException(typeof(TEntity).Name, id);
    }
}

/// <summary>
/// Exception thrown when there is a conflict with an existing entity.
/// </summary>
public class EntityConflictException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityConflictException"/> class.
    /// </summary>
    /// <param name="message">The error message describing the conflict.</param>
    public EntityConflictException(string message)
        : base("ENTITY_CONFLICT", message)
    {
    }
}
