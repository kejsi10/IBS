namespace IBS.Tenants.Domain.ValueObjects;

/// <summary>
/// Represents the subscription tier for a tenant.
/// </summary>
public enum SubscriptionTier
{
    /// <summary>
    /// Basic subscription with limited features.
    /// </summary>
    Basic = 1,

    /// <summary>
    /// Professional subscription with standard features.
    /// </summary>
    Professional = 2,

    /// <summary>
    /// Enterprise subscription with all features.
    /// </summary>
    Enterprise = 3
}
