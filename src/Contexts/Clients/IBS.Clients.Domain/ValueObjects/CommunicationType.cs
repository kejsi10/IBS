namespace IBS.Clients.Domain.ValueObjects;

/// <summary>
/// Represents the type of communication with a client.
/// </summary>
public enum CommunicationType
{
    /// <summary>
    /// Email communication.
    /// </summary>
    Email = 1,

    /// <summary>
    /// Phone call.
    /// </summary>
    Phone = 2,

    /// <summary>
    /// In-person meeting.
    /// </summary>
    Meeting = 3,

    /// <summary>
    /// Text/SMS message.
    /// </summary>
    Text = 4,

    /// <summary>
    /// Physical mail/letter.
    /// </summary>
    Mail = 5,

    /// <summary>
    /// Other communication type.
    /// </summary>
    Other = 99
}
