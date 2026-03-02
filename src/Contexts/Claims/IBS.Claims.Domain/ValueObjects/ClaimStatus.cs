namespace IBS.Claims.Domain.ValueObjects;

/// <summary>
/// Represents the status of a claim in its lifecycle.
/// </summary>
public enum ClaimStatus
{
    /// <summary>First Notice of Loss — initial filing.</summary>
    FNOL,

    /// <summary>Claim has been acknowledged by the insurer.</summary>
    Acknowledged,

    /// <summary>An adjuster has been assigned to the claim.</summary>
    Assigned,

    /// <summary>Claim is under active investigation.</summary>
    UnderInvestigation,

    /// <summary>Claim is being evaluated for coverage and damages.</summary>
    Evaluation,

    /// <summary>Claim has been approved for payment.</summary>
    Approved,

    /// <summary>Claim has been denied.</summary>
    Denied,

    /// <summary>Claim is in settlement process.</summary>
    Settlement,

    /// <summary>Claim has been closed.</summary>
    Closed
}

/// <summary>
/// Extension methods for ClaimStatus.
/// </summary>
public static class ClaimStatusExtensions
{
    /// <summary>
    /// Gets the display name for a claim status.
    /// </summary>
    public static string GetDisplayName(this ClaimStatus status) => status switch
    {
        ClaimStatus.FNOL => "FNOL",
        ClaimStatus.Acknowledged => "Acknowledged",
        ClaimStatus.Assigned => "Assigned",
        ClaimStatus.UnderInvestigation => "Under Investigation",
        ClaimStatus.Evaluation => "Evaluation",
        ClaimStatus.Approved => "Approved",
        ClaimStatus.Denied => "Denied",
        ClaimStatus.Settlement => "Settlement",
        ClaimStatus.Closed => "Closed",
        _ => status.ToString()
    };

    /// <summary>
    /// Determines if the status is a terminal state.
    /// </summary>
    public static bool IsTerminal(this ClaimStatus status) => status == ClaimStatus.Closed;

    /// <summary>
    /// Determines if the status is an open (non-terminal) state.
    /// </summary>
    public static bool IsOpen(this ClaimStatus status) => !status.IsTerminal();

    /// <summary>
    /// Gets the valid transitions from a given status.
    /// </summary>
    public static IReadOnlyList<ClaimStatus> GetValidTransitions(this ClaimStatus status) => status switch
    {
        ClaimStatus.FNOL => [ClaimStatus.Acknowledged],
        ClaimStatus.Acknowledged => [ClaimStatus.Assigned],
        ClaimStatus.Assigned => [ClaimStatus.UnderInvestigation],
        ClaimStatus.UnderInvestigation => [ClaimStatus.Evaluation],
        ClaimStatus.Evaluation => [ClaimStatus.Approved, ClaimStatus.Denied],
        ClaimStatus.Approved => [ClaimStatus.Settlement],
        ClaimStatus.Denied => [ClaimStatus.Closed],
        ClaimStatus.Settlement => [ClaimStatus.Closed],
        ClaimStatus.Closed => [ClaimStatus.FNOL], // Reopen re-enters as FNOL->Acknowledged->...->UnderInvestigation
        _ => []
    };

    /// <summary>
    /// Determines if a transition to the specified status is valid.
    /// </summary>
    public static bool CanTransitionTo(this ClaimStatus current, ClaimStatus target)
    {
        return current.GetValidTransitions().Contains(target);
    }
}
