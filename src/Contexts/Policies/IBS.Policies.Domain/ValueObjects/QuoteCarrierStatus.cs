namespace IBS.Policies.Domain.ValueObjects;

/// <summary>
/// Represents the status of a carrier's participation in a quote.
/// </summary>
public enum QuoteCarrierStatus
{
    /// <summary>Awaiting response from the carrier.</summary>
    Pending = 0,

    /// <summary>Carrier has provided a premium quote.</summary>
    Quoted = 1,

    /// <summary>Carrier has declined to quote.</summary>
    Declined = 2,

    /// <summary>Carrier's quote has expired.</summary>
    Expired = 3
}

/// <summary>
/// Extension methods for QuoteCarrierStatus.
/// </summary>
public static class QuoteCarrierStatusExtensions
{
    /// <summary>
    /// Gets a display-friendly name for the carrier status.
    /// </summary>
    public static string GetDisplayName(this QuoteCarrierStatus status) => status switch
    {
        QuoteCarrierStatus.Pending => "Pending",
        QuoteCarrierStatus.Quoted => "Quoted",
        QuoteCarrierStatus.Declined => "Declined",
        QuoteCarrierStatus.Expired => "Expired",
        _ => status.ToString()
    };
}
