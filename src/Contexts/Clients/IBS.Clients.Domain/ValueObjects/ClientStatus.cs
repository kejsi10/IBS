namespace IBS.Clients.Domain.ValueObjects;

/// <summary>
/// Represents the status of a client.
/// </summary>
public enum ClientStatus
{
    /// <summary>
    /// Client is active and can have policies.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Client has been deactivated.
    /// </summary>
    Inactive = 2
}
