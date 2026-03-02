namespace IBS.Commissions.Domain.ValueObjects;

/// <summary>
/// Represents the status of a commission statement in its lifecycle.
/// </summary>
public enum StatementStatus
{
    /// <summary>Statement has been received from the carrier.</summary>
    Received,

    /// <summary>Statement is being reconciled against policies.</summary>
    Reconciling,

    /// <summary>All line items have been reconciled or disputed.</summary>
    Reconciled,

    /// <summary>One or more line items are disputed.</summary>
    Disputed,

    /// <summary>Commission has been paid out.</summary>
    Paid
}

/// <summary>
/// Extension methods for StatementStatus.
/// </summary>
public static class StatementStatusExtensions
{
    /// <summary>
    /// Gets the display name for a statement status.
    /// </summary>
    public static string GetDisplayName(this StatementStatus status) => status switch
    {
        StatementStatus.Received => "Received",
        StatementStatus.Reconciling => "Reconciling",
        StatementStatus.Reconciled => "Reconciled",
        StatementStatus.Disputed => "Disputed",
        StatementStatus.Paid => "Paid",
        _ => status.ToString()
    };

    /// <summary>
    /// Gets the valid transitions from a given status.
    /// </summary>
    public static IReadOnlyList<StatementStatus> GetValidTransitions(this StatementStatus status) => status switch
    {
        StatementStatus.Received => [StatementStatus.Reconciling],
        StatementStatus.Reconciling => [StatementStatus.Reconciled, StatementStatus.Disputed],
        StatementStatus.Reconciled => [StatementStatus.Paid],
        StatementStatus.Disputed => [StatementStatus.Reconciling],
        StatementStatus.Paid => [],
        _ => []
    };

    /// <summary>
    /// Determines if a transition to the specified status is valid.
    /// </summary>
    public static bool CanTransitionTo(this StatementStatus current, StatementStatus target)
    {
        return current.GetValidTransitions().Contains(target);
    }
}
