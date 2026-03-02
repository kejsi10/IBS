using IBS.BuildingBlocks.Domain;
using IBS.Claims.Domain.Events;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Domain.Aggregates.Claim;

/// <summary>
/// Represents an insurance claim. This is the aggregate root for the Claim aggregate.
/// </summary>
public sealed class Claim : TenantAggregateRoot
{
    private readonly List<ClaimNote> _notes = [];
    private readonly List<Reserve> _reserves = [];
    private readonly List<ClaimPayment> _payments = [];

    /// <summary>
    /// Gets the claim number.
    /// </summary>
    public ClaimNumber ClaimNumber { get; private set; } = null!;

    /// <summary>
    /// Gets the policy identifier.
    /// </summary>
    public Guid PolicyId { get; private set; }

    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the claim status.
    /// </summary>
    public ClaimStatus Status { get; private set; }

    /// <summary>
    /// Gets the date of loss.
    /// </summary>
    public DateTimeOffset LossDate { get; private set; }

    /// <summary>
    /// Gets the date the loss was reported.
    /// </summary>
    public DateTimeOffset ReportedDate { get; private set; }

    /// <summary>
    /// Gets the type of loss.
    /// </summary>
    public LossType LossType { get; private set; }

    /// <summary>
    /// Gets the description of the loss.
    /// </summary>
    public string LossDescription { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the estimated loss amount.
    /// </summary>
    public Money? LossAmount { get; private set; }

    /// <summary>
    /// Gets the approved claim amount.
    /// </summary>
    public Money? ClaimAmount { get; private set; }

    /// <summary>
    /// Gets the assigned adjuster identifier.
    /// </summary>
    public string? AssignedAdjusterId { get; private set; }

    /// <summary>
    /// Gets the denial reason.
    /// </summary>
    public string? DenialReason { get; private set; }

    /// <summary>
    /// Gets the date/time when the claim was closed.
    /// </summary>
    public DateTimeOffset? ClosedAt { get; private set; }

    /// <summary>
    /// Gets the closure reason.
    /// </summary>
    public string? ClosureReason { get; private set; }

    /// <summary>
    /// Gets the user who created this claim.
    /// </summary>
    public Guid CreatedBy { get; private set; }

    /// <summary>
    /// Gets the notes attached to this claim.
    /// </summary>
    public IReadOnlyCollection<ClaimNote> Notes => _notes.AsReadOnly();

    /// <summary>
    /// Gets the reserves set on this claim.
    /// </summary>
    public IReadOnlyCollection<Reserve> Reserves => _reserves.AsReadOnly();

    /// <summary>
    /// Gets the payments on this claim.
    /// </summary>
    public IReadOnlyCollection<ClaimPayment> Payments => _payments.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Claim() { }

    /// <summary>
    /// Creates a new claim (FNOL).
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="policyId">The policy identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="lossDate">The date of loss.</param>
    /// <param name="reportedDate">The date the loss was reported.</param>
    /// <param name="lossType">The type of loss.</param>
    /// <param name="lossDescription">The description of the loss.</param>
    /// <param name="createdBy">The user who created the claim.</param>
    /// <param name="estimatedLossAmount">The estimated loss amount.</param>
    /// <returns>A new Claim instance in FNOL status.</returns>
    public static Claim Create(
        Guid tenantId,
        Guid policyId,
        Guid clientId,
        DateTimeOffset lossDate,
        DateTimeOffset reportedDate,
        LossType lossType,
        string lossDescription,
        Guid createdBy,
        Money? estimatedLossAmount = null)
    {
        if (lossDate > reportedDate)
            throw new BusinessRuleViolationException("Loss date cannot be after reported date.");

        if (string.IsNullOrWhiteSpace(lossDescription))
            throw new ArgumentException("Loss description is required.", nameof(lossDescription));

        if (lossDescription.Length > 4000)
            throw new ArgumentException("Loss description must not exceed 4000 characters.", nameof(lossDescription));

        var claim = new Claim
        {
            TenantId = tenantId,
            PolicyId = policyId,
            ClientId = clientId,
            Status = ClaimStatus.FNOL,
            LossDate = lossDate,
            ReportedDate = reportedDate,
            LossType = lossType,
            LossDescription = lossDescription.Trim(),
            LossAmount = estimatedLossAmount,
            CreatedBy = createdBy,
            ClaimNumber = ClaimNumber.Generate()
        };

        claim.RaiseDomainEvent(new ClaimCreatedEvent(
            claim.Id,
            claim.TenantId,
            claim.ClaimNumber.Value,
            claim.PolicyId,
            claim.ClientId,
            claim.LossType,
            claim.LossDate));

        return claim;
    }

    /// <summary>
    /// Acknowledges the claim.
    /// </summary>
    public void Acknowledge()
    {
        EnsureCanTransitionTo(ClaimStatus.Acknowledged);
        Status = ClaimStatus.Acknowledged;
        MarkAsUpdated();
        RaiseDomainEvent(new ClaimAcknowledgedEvent(Id, TenantId, ClaimNumber.Value));
    }

    /// <summary>
    /// Assigns an adjuster to the claim.
    /// </summary>
    /// <param name="adjusterId">The adjuster identifier.</param>
    public void AssignAdjuster(string adjusterId)
    {
        if (string.IsNullOrWhiteSpace(adjusterId))
            throw new ArgumentException("Adjuster ID is required.", nameof(adjusterId));

        EnsureCanTransitionTo(ClaimStatus.Assigned);
        AssignedAdjusterId = adjusterId.Trim();
        Status = ClaimStatus.Assigned;
        MarkAsUpdated();
        RaiseDomainEvent(new AdjusterAssignedEvent(Id, TenantId, ClaimNumber.Value, adjusterId));
    }

    /// <summary>
    /// Starts the investigation.
    /// </summary>
    public void StartInvestigation()
    {
        EnsureCanTransitionTo(ClaimStatus.UnderInvestigation);
        Status = ClaimStatus.UnderInvestigation;
        MarkAsUpdated();
        RaiseDomainEvent(new InvestigationStartedEvent(Id, TenantId, ClaimNumber.Value));
    }

    /// <summary>
    /// Moves the claim to evaluation.
    /// </summary>
    public void Evaluate()
    {
        EnsureCanTransitionTo(ClaimStatus.Evaluation);
        Status = ClaimStatus.Evaluation;
        MarkAsUpdated();
        RaiseDomainEvent(new ClaimEvaluatedEvent(Id, TenantId, ClaimNumber.Value));
    }

    /// <summary>
    /// Approves the claim with an approved amount.
    /// </summary>
    /// <param name="claimAmount">The approved claim amount.</param>
    public void Approve(Money claimAmount)
    {
        EnsureCanTransitionTo(ClaimStatus.Approved);

        if (claimAmount.Amount <= 0)
            throw new BusinessRuleViolationException("Approved claim amount must be positive.");

        ClaimAmount = claimAmount;
        Status = ClaimStatus.Approved;
        MarkAsUpdated();
        RaiseDomainEvent(new ClaimApprovedEvent(Id, TenantId, ClaimNumber.Value, claimAmount.Amount, claimAmount.Currency));
    }

    /// <summary>
    /// Denies the claim.
    /// </summary>
    /// <param name="denialReason">The reason for denial.</param>
    public void Deny(string denialReason)
    {
        EnsureCanTransitionTo(ClaimStatus.Denied);

        if (string.IsNullOrWhiteSpace(denialReason))
            throw new ArgumentException("Denial reason is required.", nameof(denialReason));

        DenialReason = denialReason.Trim();
        Status = ClaimStatus.Denied;
        MarkAsUpdated();
        RaiseDomainEvent(new ClaimDeniedEvent(Id, TenantId, ClaimNumber.Value, denialReason));
    }

    /// <summary>
    /// Moves the claim to settlement.
    /// </summary>
    public void MoveToSettlement()
    {
        EnsureCanTransitionTo(ClaimStatus.Settlement);
        Status = ClaimStatus.Settlement;
        MarkAsUpdated();
    }

    /// <summary>
    /// Closes the claim.
    /// </summary>
    /// <param name="closureReason">The reason for closure.</param>
    public void Close(string closureReason)
    {
        if (Status != ClaimStatus.Settlement && Status != ClaimStatus.Denied)
            throw new BusinessRuleViolationException(
                $"Cannot close a claim in '{Status.GetDisplayName()}' status. Must be in Settlement or Denied status.");

        if (_payments.Any(p => p.IsPending))
            throw new BusinessRuleViolationException("Cannot close claim with pending payments.");

        if (string.IsNullOrWhiteSpace(closureReason))
            throw new ArgumentException("Closure reason is required.", nameof(closureReason));

        Status = ClaimStatus.Closed;
        ClosedAt = DateTimeOffset.UtcNow;
        ClosureReason = closureReason.Trim();
        MarkAsUpdated();
        RaiseDomainEvent(new ClaimClosedEvent(Id, TenantId, ClaimNumber.Value, closureReason));
    }

    /// <summary>
    /// Reopens a closed claim.
    /// </summary>
    public void Reopen()
    {
        if (Status != ClaimStatus.Closed)
            throw new BusinessRuleViolationException("Only closed claims can be reopened.");

        Status = ClaimStatus.UnderInvestigation;
        ClosedAt = null;
        ClosureReason = null;
        MarkAsUpdated();
        RaiseDomainEvent(new ClaimReopenedEvent(Id, TenantId, ClaimNumber.Value));
    }

    /// <summary>
    /// Adds a note to the claim.
    /// </summary>
    /// <param name="content">The note content.</param>
    /// <param name="authorBy">The author identifier.</param>
    /// <param name="isInternal">Whether the note is internal.</param>
    /// <returns>The created note.</returns>
    public ClaimNote AddNote(string content, string authorBy, bool isInternal = false)
    {
        var note = ClaimNote.Create(Id, content, authorBy, isInternal);
        _notes.Add(note);
        MarkAsUpdated();
        return note;
    }

    /// <summary>
    /// Sets a reserve on the claim.
    /// </summary>
    /// <param name="reserveType">The reserve type.</param>
    /// <param name="amount">The reserve amount.</param>
    /// <param name="setBy">The user who set the reserve.</param>
    /// <param name="notes">Optional notes.</param>
    /// <returns>The created reserve.</returns>
    public Reserve SetReserve(string reserveType, Money amount, string setBy, string? notes = null)
    {
        var reserve = Reserve.Create(Id, reserveType, amount, setBy, notes);
        _reserves.Add(reserve);
        MarkAsUpdated();
        RaiseDomainEvent(new ReserveSetEvent(Id, TenantId, ClaimNumber.Value, reserveType, amount.Amount, amount.Currency));
        return reserve;
    }

    /// <summary>
    /// Authorizes a payment on the claim.
    /// </summary>
    /// <param name="paymentType">The payment type.</param>
    /// <param name="amount">The payment amount.</param>
    /// <param name="payeeName">The payee name.</param>
    /// <param name="paymentDate">The payment date.</param>
    /// <param name="authorizedBy">The user authorizing the payment.</param>
    /// <param name="checkNumber">Optional check number.</param>
    /// <returns>The created payment.</returns>
    public ClaimPayment AuthorizePayment(
        string paymentType,
        Money amount,
        string payeeName,
        DateOnly paymentDate,
        string authorizedBy,
        string? checkNumber = null)
    {
        if (Status == ClaimStatus.Denied)
            throw new BusinessRuleViolationException("Cannot authorize payments on denied claims.");

        if (ClaimAmount != null)
        {
            var totalPayments = GetTotalActivePayments();
            var newTotal = totalPayments + amount.Amount;
            if (newTotal > ClaimAmount.Amount)
                throw new BusinessRuleViolationException(
                    $"Total payments ({newTotal:N2}) would exceed approved claim amount ({ClaimAmount.Amount:N2}).");
        }

        var payment = ClaimPayment.Create(Id, paymentType, amount, payeeName, paymentDate, authorizedBy, checkNumber);
        _payments.Add(payment);
        MarkAsUpdated();
        RaiseDomainEvent(new PaymentAuthorizedEvent(Id, TenantId, ClaimNumber.Value, payment.Id, amount.Amount, amount.Currency));
        return payment;
    }

    /// <summary>
    /// Issues an authorized payment.
    /// </summary>
    /// <param name="paymentId">The payment identifier.</param>
    public void IssuePayment(Guid paymentId)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == paymentId)
            ?? throw new BusinessRuleViolationException($"Payment with ID '{paymentId}' not found.");

        payment.Issue();
        MarkAsUpdated();
        RaiseDomainEvent(new PaymentIssuedEvent(Id, TenantId, ClaimNumber.Value, paymentId));
    }

    /// <summary>
    /// Voids a payment.
    /// </summary>
    /// <param name="paymentId">The payment identifier.</param>
    /// <param name="reason">The reason for voiding.</param>
    public void VoidPayment(Guid paymentId, string reason)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == paymentId)
            ?? throw new BusinessRuleViolationException($"Payment with ID '{paymentId}' not found.");

        payment.Void(reason);
        MarkAsUpdated();
    }

    /// <summary>
    /// Gets the total amount of active (non-voided) payments.
    /// </summary>
    public decimal GetTotalActivePayments()
    {
        return _payments
            .Where(p => p.Status != PaymentStatus.Voided)
            .Sum(p => p.Amount.Amount);
    }

    /// <summary>
    /// Gets the total reserve amount.
    /// </summary>
    public decimal GetTotalReserves()
    {
        return _reserves.Sum(r => r.Amount.Amount);
    }

    private void EnsureCanTransitionTo(ClaimStatus targetStatus)
    {
        if (!Status.CanTransitionTo(targetStatus))
            throw new BusinessRuleViolationException(
                $"Cannot transition from '{Status.GetDisplayName()}' to '{targetStatus.GetDisplayName()}'.");
    }
}
