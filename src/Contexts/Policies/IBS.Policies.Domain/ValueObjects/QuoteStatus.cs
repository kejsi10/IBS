namespace IBS.Policies.Domain.ValueObjects;

/// <summary>
/// Represents the status of a quote throughout its lifecycle.
/// </summary>
public enum QuoteStatus
{
    /// <summary>Quote is being prepared, carriers can be added/removed.</summary>
    Draft = 0,

    /// <summary>Quote has been submitted to carriers for pricing.</summary>
    Submitted = 1,

    /// <summary>At least one carrier has provided a quote; all have responded.</summary>
    Quoted = 2,

    /// <summary>A carrier quote has been accepted and a policy created.</summary>
    Accepted = 3,

    /// <summary>Quote has expired past its validity date.</summary>
    Expired = 4,

    /// <summary>Quote has been cancelled by the user.</summary>
    Cancelled = 5
}

/// <summary>
/// Extension methods for QuoteStatus.
/// </summary>
public static class QuoteStatusExtensions
{
    /// <summary>
    /// Determines if carriers can be added or removed in this status.
    /// </summary>
    public static bool AllowsCarrierChanges(this QuoteStatus status) => status == QuoteStatus.Draft;

    /// <summary>
    /// Determines if the quote is in a terminal state.
    /// </summary>
    public static bool IsTerminal(this QuoteStatus status) => status switch
    {
        QuoteStatus.Accepted => true,
        QuoteStatus.Expired => true,
        QuoteStatus.Cancelled => true,
        _ => false
    };

    /// <summary>
    /// Determines if the quote can be cancelled.
    /// </summary>
    public static bool CanBeCancelled(this QuoteStatus status) => status switch
    {
        QuoteStatus.Draft => true,
        QuoteStatus.Submitted => true,
        _ => false
    };

    /// <summary>
    /// Gets a display-friendly name for the quote status.
    /// </summary>
    public static string GetDisplayName(this QuoteStatus status) => status switch
    {
        QuoteStatus.Draft => "Draft",
        QuoteStatus.Submitted => "Submitted",
        QuoteStatus.Quoted => "Quoted",
        QuoteStatus.Accepted => "Accepted",
        QuoteStatus.Expired => "Expired",
        QuoteStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };
}
