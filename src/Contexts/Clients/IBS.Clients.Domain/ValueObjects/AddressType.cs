namespace IBS.Clients.Domain.ValueObjects;

/// <summary>
/// Represents the type of address.
/// </summary>
public enum AddressType
{
    /// <summary>
    /// Primary mailing address.
    /// </summary>
    Mailing = 1,

    /// <summary>
    /// Physical/location address.
    /// </summary>
    Physical = 2,

    /// <summary>
    /// Billing address.
    /// </summary>
    Billing = 3
}
