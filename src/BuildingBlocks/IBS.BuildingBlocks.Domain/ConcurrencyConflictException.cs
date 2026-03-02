namespace IBS.BuildingBlocks.Domain;

/// <summary>
/// Exception thrown when a concurrency conflict is detected via ETag/If-Match mismatch.
/// </summary>
public class ConcurrencyConflictException : DomainException
{
    /// <summary>
    /// Gets the current ETag value of the resource.
    /// </summary>
    public string CurrentETag { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyConflictException"/> class.
    /// </summary>
    /// <param name="currentETag">The current ETag value of the resource.</param>
    public ConcurrencyConflictException(string currentETag)
        : base("CONCURRENCY_CONFLICT", "The resource has been modified since it was last retrieved.")
    {
        CurrentETag = currentETag;
    }
}
