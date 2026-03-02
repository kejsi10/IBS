namespace IBS.BuildingBlocks.Application;

/// <summary>
/// Interface for accessing the current user context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Gets a value indicating whether a user context is available.
    /// </summary>
    bool IsAuthenticated { get; }
}
