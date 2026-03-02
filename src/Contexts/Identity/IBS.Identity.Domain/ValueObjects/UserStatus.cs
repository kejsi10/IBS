namespace IBS.Identity.Domain.ValueObjects;

/// <summary>
/// Represents the status of a user account.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// User account is pending activation.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// User account is active.
    /// </summary>
    Active = 2,

    /// <summary>
    /// User account has been deactivated.
    /// </summary>
    Inactive = 3,

    /// <summary>
    /// User account has been locked due to failed login attempts.
    /// </summary>
    Locked = 4
}
