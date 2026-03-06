namespace IBS.Policies.Domain.Aggregates.Policy;

/// <summary>
/// Types of events that can appear in a policy's version history.
/// </summary>
public enum PolicyHistoryEventType
{
    /// <summary>Policy was created.</summary>
    Created,

    /// <summary>Policy was bound.</summary>
    Bound,

    /// <summary>Policy was activated (issued).</summary>
    Activated,

    /// <summary>Policy was cancelled.</summary>
    Cancelled,

    /// <summary>Policy was reinstated.</summary>
    Reinstated,

    /// <summary>Policy was renewed.</summary>
    Renewed,

    /// <summary>Policy expired.</summary>
    Expired,

    /// <summary>Policy was non-renewed.</summary>
    NonRenewed,

    /// <summary>A coverage was added to the policy.</summary>
    CoverageAdded,

    /// <summary>A coverage was modified.</summary>
    CoverageModified,

    /// <summary>A coverage was removed.</summary>
    CoverageRemoved,

    /// <summary>An endorsement was added.</summary>
    EndorsementAdded,

    /// <summary>An endorsement was approved.</summary>
    EndorsementApproved,

    /// <summary>An endorsement was issued.</summary>
    EndorsementIssued,

    /// <summary>An endorsement was rejected.</summary>
    EndorsementRejected,

    /// <summary>The policy premium changed.</summary>
    PremiumChanged,
}
