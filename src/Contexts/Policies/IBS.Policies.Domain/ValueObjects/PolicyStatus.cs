namespace IBS.Policies.Domain.ValueObjects;

/// <summary>
/// Represents the status of a policy throughout its lifecycle.
/// </summary>
public enum PolicyStatus
{
    /// <summary>
    /// Policy is in draft/quote stage, not yet bound.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Policy has been bound but not yet issued by the carrier.
    /// </summary>
    Bound = 1,

    /// <summary>
    /// Policy has been issued and is currently active.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Policy is pending cancellation.
    /// </summary>
    PendingCancellation = 3,

    /// <summary>
    /// Policy has been cancelled.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Policy has expired (end date has passed).
    /// </summary>
    Expired = 5,

    /// <summary>
    /// Policy is pending renewal.
    /// </summary>
    PendingRenewal = 6,

    /// <summary>
    /// Policy has been renewed (new policy created).
    /// </summary>
    Renewed = 7,

    /// <summary>
    /// Policy has been non-renewed by carrier or insured.
    /// </summary>
    NonRenewed = 8
}

/// <summary>
/// Extension methods for PolicyStatus.
/// </summary>
public static class PolicyStatusExtensions
{
    /// <summary>
    /// Determines if the policy status allows coverage modifications (add/remove/update coverages).
    /// Only Draft policies allow direct coverage changes.
    /// </summary>
    public static bool AllowsCoverageChanges(this PolicyStatus status) => status switch
    {
        PolicyStatus.Draft => true,
        _ => false
    };

    /// <summary>
    /// Determines if the policy status allows endorsements.
    /// Bound, Active, and PendingRenewal policies can have endorsements.
    /// </summary>
    public static bool AllowsEndorsements(this PolicyStatus status) => status switch
    {
        PolicyStatus.Draft => true,
        PolicyStatus.Bound => true,
        PolicyStatus.Active => true,
        PolicyStatus.PendingRenewal => true,
        _ => false
    };

    /// <summary>
    /// Determines if the policy status allows modifications (coverages or endorsements).
    /// </summary>
    public static bool AllowsModifications(this PolicyStatus status) =>
        status.AllowsCoverageChanges() || status.AllowsEndorsements();

    /// <summary>
    /// Determines if the policy is in a terminal state.
    /// </summary>
    public static bool IsTerminal(this PolicyStatus status) => status switch
    {
        PolicyStatus.Cancelled => true,
        PolicyStatus.Expired => true,
        PolicyStatus.Renewed => true,
        PolicyStatus.NonRenewed => true,
        _ => false
    };

    /// <summary>
    /// Determines if the policy is currently in force.
    /// </summary>
    public static bool IsInForce(this PolicyStatus status) => status switch
    {
        PolicyStatus.Active => true,
        PolicyStatus.PendingCancellation => true,
        PolicyStatus.PendingRenewal => true,
        _ => false
    };

    /// <summary>
    /// Gets a display-friendly name for the policy status.
    /// </summary>
    public static string GetDisplayName(this PolicyStatus status) => status switch
    {
        PolicyStatus.Draft => "Draft",
        PolicyStatus.Bound => "Bound",
        PolicyStatus.Active => "Active",
        PolicyStatus.PendingCancellation => "Pending Cancellation",
        PolicyStatus.Cancelled => "Cancelled",
        PolicyStatus.Expired => "Expired",
        PolicyStatus.PendingRenewal => "Pending Renewal",
        PolicyStatus.Renewed => "Renewed",
        PolicyStatus.NonRenewed => "Non-Renewed",
        _ => status.ToString()
    };
}
