namespace IBS.BuildingBlocks.Application;

/// <summary>
/// Represents an error that can occur during an operation.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Message">The error message.</param>
public record Error(string Code, string Message)
{
    /// <summary>
    /// Represents no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    /// <param name="entityName">The name of the entity.</param>
    /// <param name="id">The identifier that was not found.</param>
    /// <returns>A not found error.</returns>
    public static Error NotFound(string entityName, object id) =>
        new($"{entityName}.NotFound", $"{entityName} with id '{id}' was not found.");

    /// <summary>
    /// Creates a not found error with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A not found error.</returns>
    public static Error NotFound(string message) =>
        new("NotFound", message);

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <param name="message">The validation error message.</param>
    /// <returns>A validation error.</returns>
    public static Error Validation(string message) =>
        new("Validation.Error", message);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    /// <param name="message">The conflict message.</param>
    /// <returns>A conflict error.</returns>
    public static Error Conflict(string message) =>
        new("Conflict.Error", message);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    /// <returns>An unauthorized error.</returns>
    public static Error Unauthorized() =>
        new("Unauthorized", "You are not authorized to perform this action.");

    /// <summary>
    /// Creates an unauthorized error with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>An unauthorized error.</returns>
    public static Error Unauthorized(string message) =>
        new("Unauthorized", message);

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    /// <returns>A forbidden error.</returns>
    public static Error Forbidden() =>
        new("Forbidden", "You do not have permission to access this resource.");

    /// <summary>
    /// Creates an internal error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>An internal error.</returns>
    public static Error Internal(string message) =>
        new("Internal.Error", message);
}

/// <summary>
/// Represents a collection of validation errors.
/// </summary>
public sealed record ValidationError : Error
{
    /// <summary>
    /// Gets the validation errors by property.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public ValidationError(IReadOnlyDictionary<string, string[]> errors)
        : base("Validation.Error", "One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
