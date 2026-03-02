namespace IBS.Carriers.Domain.ValueObjects;

/// <summary>
/// Represents the status of a carrier in the system.
/// </summary>
public enum CarrierStatus
{
    /// <summary>Carrier is active and accepting new business.</summary>
    Active,

    /// <summary>Carrier is temporarily suspended (not accepting new business).</summary>
    Suspended,

    /// <summary>Carrier is inactive/discontinued.</summary>
    Inactive
}
