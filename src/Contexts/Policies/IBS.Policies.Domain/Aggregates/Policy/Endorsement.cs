using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Domain.Aggregates.Policy;

/// <summary>
/// Represents an endorsement (mid-term policy change) on a policy.
/// </summary>
public sealed class Endorsement : Entity
{
    /// <summary>
    /// Gets the policy identifier.
    /// </summary>
    public Guid PolicyId { get; private set; }

    /// <summary>
    /// Gets the endorsement number.
    /// </summary>
    public string EndorsementNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the effective date of the endorsement.
    /// </summary>
    public DateOnly EffectiveDate { get; private set; }

    /// <summary>
    /// Gets the endorsement type/code.
    /// </summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description of the endorsement.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the premium change (positive for increase, negative for decrease).
    /// </summary>
    public Money PremiumChange { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether this endorsement increases premium.
    /// </summary>
    public bool IsAp => PremiumChange.Amount > 0;

    /// <summary>
    /// Gets a value indicating whether this endorsement decreases premium.
    /// </summary>
    public bool IsRp => PremiumChange.Amount < 0;

    /// <summary>
    /// Gets the endorsement status.
    /// </summary>
    public EndorsementStatus Status { get; private set; }

    /// <summary>
    /// Gets the date the endorsement was processed.
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; private set; }

    /// <summary>
    /// Gets the user who processed the endorsement.
    /// </summary>
    public Guid? ProcessedBy { get; private set; }

    /// <summary>
    /// Gets additional notes for the endorsement.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Endorsement() { }

    /// <summary>
    /// Creates a new endorsement.
    /// </summary>
    internal static Endorsement Create(
        Guid policyId,
        string endorsementNumber,
        DateOnly effectiveDate,
        string type,
        string description,
        Money premiumChange,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(endorsementNumber))
            throw new ArgumentException("Endorsement number is required.", nameof(endorsementNumber));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Endorsement type is required.", nameof(type));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Endorsement description is required.", nameof(description));

        return new Endorsement
        {
            PolicyId = policyId,
            EndorsementNumber = endorsementNumber.Trim().ToUpperInvariant(),
            EffectiveDate = effectiveDate,
            Type = type.Trim(),
            Description = description.Trim(),
            PremiumChange = premiumChange,
            Status = EndorsementStatus.Pending,
            Notes = notes?.Trim()
        };
    }

    /// <summary>
    /// Approves the endorsement.
    /// </summary>
    /// <param name="approvedBy">The user who approved the endorsement.</param>
    internal void Approve(Guid approvedBy)
    {
        if (Status != EndorsementStatus.Pending)
            throw new BusinessRuleViolationException("Only pending endorsements can be approved.");

        Status = EndorsementStatus.Approved;
        ProcessedAt = DateTimeOffset.UtcNow;
        ProcessedBy = approvedBy;
    }

    /// <summary>
    /// Issues the endorsement (makes it effective).
    /// </summary>
    internal void Issue()
    {
        if (Status != EndorsementStatus.Approved)
            throw new BusinessRuleViolationException("Only approved endorsements can be issued.");

        Status = EndorsementStatus.Issued;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Rejects the endorsement.
    /// </summary>
    /// <param name="rejectedBy">The user who rejected the endorsement.</param>
    /// <param name="reason">The reason for rejection.</param>
    internal void Reject(Guid rejectedBy, string reason)
    {
        if (Status != EndorsementStatus.Pending)
            throw new BusinessRuleViolationException("Only pending endorsements can be rejected.");

        Status = EndorsementStatus.Rejected;
        ProcessedAt = DateTimeOffset.UtcNow;
        ProcessedBy = rejectedBy;
        Notes = string.IsNullOrEmpty(Notes) ? reason : $"{Notes}\n\nRejection reason: {reason}";
    }

    /// <summary>
    /// Cancels the endorsement.
    /// </summary>
    internal void Cancel()
    {
        if (Status == EndorsementStatus.Issued)
            throw new BusinessRuleViolationException("Issued endorsements cannot be cancelled.");

        Status = EndorsementStatus.Cancelled;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the notes for this endorsement.
    /// </summary>
    internal void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
    }
}

/// <summary>
/// Represents the status of an endorsement.
/// </summary>
public enum EndorsementStatus
{
    /// <summary>Endorsement is pending review.</summary>
    Pending = 0,

    /// <summary>Endorsement has been approved.</summary>
    Approved = 1,

    /// <summary>Endorsement has been issued (effective).</summary>
    Issued = 2,

    /// <summary>Endorsement was rejected.</summary>
    Rejected = 3,

    /// <summary>Endorsement was cancelled.</summary>
    Cancelled = 4
}
