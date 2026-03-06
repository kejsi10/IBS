using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Events;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Domain.Aggregates.Policy;

/// <summary>
/// Represents an insurance policy.
/// This is the aggregate root for the Policy aggregate.
/// </summary>
public sealed class Policy : TenantAggregateRoot
{
    private readonly List<Coverage> _coverages = [];
    private readonly List<Endorsement> _endorsements = [];

    /// <summary>
    /// Gets the policy number.
    /// </summary>
    public PolicyNumber PolicyNumber { get; private set; } = null!;

    /// <summary>
    /// Gets the client (insured) identifier.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the carrier identifier.
    /// </summary>
    public Guid CarrierId { get; private set; }

    /// <summary>
    /// Gets the line of business.
    /// </summary>
    public LineOfBusiness LineOfBusiness { get; private set; }

    /// <summary>
    /// Gets the policy type/product name.
    /// </summary>
    public string PolicyType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the policy status.
    /// </summary>
    public PolicyStatus Status { get; private set; }

    /// <summary>
    /// Gets the policy's effective period.
    /// </summary>
    public EffectivePeriod EffectivePeriod { get; private set; } = null!;

    /// <summary>
    /// Gets the total premium amount.
    /// </summary>
    public Money TotalPremium { get; private set; } = null!;

    /// <summary>
    /// Gets the billing type.
    /// </summary>
    public BillingType BillingType { get; private set; }

    /// <summary>
    /// Gets the payment plan.
    /// </summary>
    public PaymentPlan PaymentPlan { get; private set; }

    /// <summary>
    /// Gets the carrier's policy number (may differ from agency policy number).
    /// </summary>
    public string? CarrierPolicyNumber { get; private set; }

    /// <summary>
    /// Gets the quote identifier if policy originated from a quote.
    /// </summary>
    public Guid? QuoteId { get; private set; }

    /// <summary>
    /// Gets the previous policy identifier (for renewals).
    /// </summary>
    public Guid? PreviousPolicyId { get; private set; }

    /// <summary>
    /// Gets the renewal policy identifier.
    /// </summary>
    public Guid? RenewalPolicyId { get; private set; }

    /// <summary>
    /// Gets the date/time when the policy was bound.
    /// </summary>
    public DateTimeOffset? BoundAt { get; private set; }

    /// <summary>
    /// Gets the user who bound the policy.
    /// </summary>
    public Guid? BoundBy { get; private set; }

    /// <summary>
    /// Gets the cancellation date (if cancelled).
    /// </summary>
    public DateOnly? CancellationDate { get; private set; }

    /// <summary>
    /// Gets the cancellation reason (if cancelled).
    /// </summary>
    public string? CancellationReason { get; private set; }

    /// <summary>
    /// Gets the cancellation type (if cancelled).
    /// </summary>
    public CancellationType? CancellationType { get; private set; }

    /// <summary>
    /// Gets the reinstatement date (if reinstated).
    /// </summary>
    public DateTimeOffset? ReinstatementDate { get; private set; }

    /// <summary>
    /// Gets the reinstatement reason (if reinstated).
    /// </summary>
    public string? ReinstatementReason { get; private set; }

    /// <summary>
    /// Gets any additional notes for the policy.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets the user who created this policy.
    /// </summary>
    public Guid CreatedBy { get; private set; }

    /// <summary>
    /// Gets the coverages on this policy.
    /// </summary>
    public IReadOnlyCollection<Coverage> Coverages => _coverages.AsReadOnly();

    /// <summary>
    /// Gets the endorsements on this policy.
    /// </summary>
    public IReadOnlyCollection<Endorsement> Endorsements => _endorsements.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Policy() { }

    /// <summary>
    /// Creates a new policy.
    /// </summary>
    public static Policy Create(
        Guid tenantId,
        Guid clientId,
        Guid carrierId,
        LineOfBusiness lineOfBusiness,
        string policyType,
        EffectivePeriod effectivePeriod,
        Guid createdBy,
        string? policyNumber = null,
        BillingType billingType = BillingType.DirectBill,
        PaymentPlan paymentPlan = PaymentPlan.Annual,
        Guid? quoteId = null)
    {
        if (string.IsNullOrWhiteSpace(policyType))
            throw new ArgumentException("Policy type is required.", nameof(policyType));

        var policy = new Policy
        {
            TenantId = tenantId,
            ClientId = clientId,
            CarrierId = carrierId,
            LineOfBusiness = lineOfBusiness,
            PolicyType = policyType.Trim(),
            Status = PolicyStatus.Draft,
            EffectivePeriod = effectivePeriod,
            TotalPremium = Money.Zero(),
            BillingType = billingType,
            PaymentPlan = paymentPlan,
            QuoteId = quoteId,
            CreatedBy = createdBy,
            PolicyNumber = policyNumber != null
                ? PolicyNumber.Create(policyNumber)
                : PolicyNumber.Generate(lineOfBusiness.ToString()[..3].ToUpperInvariant())
        };

        policy.RaiseDomainEvent(new PolicyCreatedEvent(
            policy.Id,
            policy.TenantId,
            policy.ClientId,
            policy.CarrierId,
            policy.PolicyNumber.Value,
            policy.LineOfBusiness,
            policy.EffectivePeriod.EffectiveDate,
            policy.EffectivePeriod.ExpirationDate));

        return policy;
    }

    /// <summary>
    /// Creates a renewal policy based on this policy.
    /// </summary>
    public Policy CreateRenewal(Guid createdBy)
    {
        if (Status != PolicyStatus.Active && Status != PolicyStatus.PendingRenewal)
            throw new BusinessRuleViolationException("Only active or pending renewal policies can be renewed.");

        var renewalPeriod = EffectivePeriod.CreateRenewalPeriod();

        var renewal = new Policy
        {
            TenantId = TenantId,
            ClientId = ClientId,
            CarrierId = CarrierId,
            LineOfBusiness = LineOfBusiness,
            PolicyType = PolicyType,
            Status = PolicyStatus.Draft,
            EffectivePeriod = renewalPeriod,
            TotalPremium = TotalPremium, // Starting premium, may be adjusted
            BillingType = BillingType,
            PaymentPlan = PaymentPlan,
            PreviousPolicyId = Id,
            CreatedBy = createdBy,
            PolicyNumber = PolicyNumber.Generate(LineOfBusiness.ToString()[..3].ToUpperInvariant())
        };

        // Copy coverages to renewal
        foreach (var coverage in _coverages.Where(c => c.IsActive))
        {
            renewal.AddCoverage(
                coverage.Code,
                coverage.Name,
                coverage.PremiumAmount,
                coverage.Description,
                coverage.LimitAmount,
                coverage.DeductibleAmount,
                coverage.IsOptional);
        }

        return renewal;
    }

    /// <summary>
    /// Binds the policy (commits to the coverage).
    /// </summary>
    public void Bind(Guid boundBy)
    {
        if (Status != PolicyStatus.Draft)
            throw new BusinessRuleViolationException("Only draft policies can be bound.");

        if (!_coverages.Any(c => c.IsActive))
            throw new BusinessRuleViolationException("Policy must have at least one coverage before binding.");

        Status = PolicyStatus.Bound;
        BoundAt = DateTimeOffset.UtcNow;
        BoundBy = boundBy;
        MarkAsUpdated();

        RaiseDomainEvent(new PolicyBoundEvent(
            Id,
            TenantId,
            PolicyNumber.Value,
            TotalPremium.Amount));
    }

    /// <summary>
    /// Activates the policy (issues it).
    /// </summary>
    public void Activate()
    {
        if (Status != PolicyStatus.Bound)
            throw new BusinessRuleViolationException("Only bound policies can be activated.");

        Status = PolicyStatus.Active;
        MarkAsUpdated();

        RaiseDomainEvent(new PolicyActivatedEvent(Id, TenantId, PolicyNumber.Value));
    }

    /// <summary>
    /// Cancels the policy.
    /// </summary>
    public void Cancel(DateOnly cancellationDate, string reason, CancellationType cancellationType)
    {
        if (Status.IsTerminal())
            throw new BusinessRuleViolationException("Cannot cancel a policy that is already in a terminal state.");

        if (cancellationDate < EffectivePeriod.EffectiveDate)
            throw new BusinessRuleViolationException("Cancellation date cannot be before policy effective date.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason is required.", nameof(reason));

        // Flat cancel if cancellation date equals effective date
        if (cancellationDate == EffectivePeriod.EffectiveDate)
        {
            cancellationType = Events.CancellationType.FlatCancel;
        }

        Status = PolicyStatus.Cancelled;
        CancellationDate = cancellationDate;
        CancellationReason = reason.Trim();
        CancellationType = cancellationType;
        MarkAsUpdated();

        RaiseDomainEvent(new PolicyCancelledEvent(
            Id,
            TenantId,
            PolicyNumber.Value,
            cancellationDate,
            reason,
            cancellationType));
    }

    /// <summary>
    /// Marks the policy as expired.
    /// </summary>
    public void Expire()
    {
        if (Status != PolicyStatus.Active && Status != PolicyStatus.PendingRenewal)
            throw new BusinessRuleViolationException("Only active or pending renewal policies can expire.");

        Status = PolicyStatus.Expired;
        MarkAsUpdated();

        RaiseDomainEvent(new PolicyExpiredEvent(
            Id,
            TenantId,
            PolicyNumber.Value,
            EffectivePeriod.ExpirationDate));
    }

    /// <summary>
    /// Marks this policy as renewed and links to the renewal policy.
    /// </summary>
    public void MarkAsRenewed(Guid renewalPolicyId, string renewalPolicyNumber)
    {
        if (Status != PolicyStatus.Active && Status != PolicyStatus.PendingRenewal)
            throw new BusinessRuleViolationException("Only active or pending renewal policies can be renewed.");

        Status = PolicyStatus.Renewed;
        RenewalPolicyId = renewalPolicyId;
        MarkAsUpdated();

        RaiseDomainEvent(new PolicyRenewedEvent(
            Id,
            renewalPolicyId,
            TenantId,
            PolicyNumber.Value,
            renewalPolicyNumber));
    }

    /// <summary>
    /// Sets the policy to pending renewal status.
    /// </summary>
    public void SetPendingRenewal()
    {
        if (Status != PolicyStatus.Active)
            throw new BusinessRuleViolationException("Only active policies can be set to pending renewal.");

        Status = PolicyStatus.PendingRenewal;
        MarkAsUpdated();
    }

    /// <summary>
    /// Reinstates a cancelled policy, restoring it to Active status.
    /// Cannot reinstate flat-cancelled or misrepresentation-cancelled policies.
    /// </summary>
    public void Reinstate(string reason)
    {
        if (Status != PolicyStatus.Cancelled)
            throw new BusinessRuleViolationException("Only cancelled policies can be reinstated.");

        if (CancellationType == Events.CancellationType.FlatCancel)
            throw new BusinessRuleViolationException("Flat-cancelled policies cannot be reinstated.");

        if (CancellationType == Events.CancellationType.Misrepresentation)
            throw new BusinessRuleViolationException("Policies cancelled for misrepresentation cannot be reinstated.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reinstatement reason is required.", nameof(reason));

        Status = PolicyStatus.Active;
        ReinstatementDate = DateTimeOffset.UtcNow;
        ReinstatementReason = reason.Trim();
        CancellationDate = null;
        CancellationReason = null;
        CancellationType = null;
        MarkAsUpdated();

        RaiseDomainEvent(new PolicyReinstatedEvent(Id, TenantId, PolicyNumber.Value, reason));
    }

    /// <summary>
    /// Non-renews the policy.
    /// </summary>
    public void NonRenew(string reason)
    {
        if (Status != PolicyStatus.Active && Status != PolicyStatus.PendingRenewal)
            throw new BusinessRuleViolationException("Only active or pending renewal policies can be non-renewed.");

        Status = PolicyStatus.NonRenewed;
        Notes = string.IsNullOrEmpty(Notes)
            ? $"Non-renewal reason: {reason}"
            : $"{Notes}\n\nNon-renewal reason: {reason}";
        MarkAsUpdated();
    }

    /// <summary>
    /// Adds a coverage to the policy.
    /// </summary>
    public Coverage AddCoverage(
        string code,
        string name,
        Money premium,
        string? description = null,
        Money? limitAmount = null,
        Money? deductibleAmount = null,
        bool isOptional = false)
    {
        if (!Status.AllowsCoverageChanges())
            throw new BusinessRuleViolationException("Cannot add coverage to a policy in this status.");

        // Check for duplicate coverage code
        var normalizedCode = code.Trim().ToUpperInvariant();
        if (_coverages.Any(c => c.Code == normalizedCode && c.IsActive))
            throw new BusinessRuleViolationException($"Coverage with code '{normalizedCode}' already exists on this policy.");

        var coverage = Coverage.Create(Id, code, name, premium, description, limitAmount, deductibleAmount, isOptional);
        _coverages.Add(coverage);

        RecalculateTotalPremium("Coverage added");
        MarkAsUpdated();

        RaiseDomainEvent(new CoverageAddedEvent(
            Id,
            coverage.Id,
            TenantId,
            coverage.Code,
            coverage.Name,
            coverage.PremiumAmount.Amount));

        return coverage;
    }

    /// <summary>
    /// Gets a coverage by its identifier.
    /// </summary>
    public Coverage? GetCoverage(Guid coverageId)
    {
        return _coverages.FirstOrDefault(c => c.Id == coverageId);
    }

    /// <summary>
    /// Gets a coverage by its code.
    /// </summary>
    public Coverage? GetCoverageByCode(string code)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return _coverages.FirstOrDefault(c => c.Code == normalizedCode && c.IsActive);
    }

    /// <summary>
    /// Updates a coverage on the policy.
    /// </summary>
    public void UpdateCoverage(
        Guid coverageId,
        string name,
        string? description,
        Money? limitAmount,
        Money? perOccurrenceLimit,
        Money? aggregateLimit,
        Money? deductibleAmount,
        Money premium)
    {
        if (!Status.AllowsCoverageChanges())
            throw new BusinessRuleViolationException("Cannot modify coverage on a policy in this status.");

        var coverage = _coverages.FirstOrDefault(c => c.Id == coverageId)
            ?? throw new BusinessRuleViolationException($"Coverage with ID '{coverageId}' not found.");

        var oldPremium = coverage.PremiumAmount.Amount;
        coverage.Update(name, description, limitAmount, perOccurrenceLimit, aggregateLimit, deductibleAmount, premium);

        RecalculateTotalPremium("Coverage modified");
        MarkAsUpdated();

        RaiseDomainEvent(new CoverageModifiedEvent(Id, coverageId, TenantId, oldPremium, premium.Amount));
    }

    /// <summary>
    /// Removes a coverage from the policy.
    /// </summary>
    public void RemoveCoverage(Guid coverageId)
    {
        if (!Status.AllowsCoverageChanges())
            throw new BusinessRuleViolationException("Cannot remove coverage from a policy in this status.");

        var coverage = _coverages.FirstOrDefault(c => c.Id == coverageId)
            ?? throw new BusinessRuleViolationException($"Coverage with ID '{coverageId}' not found.");

        // Instead of removing, we deactivate to maintain history
        coverage.Deactivate();

        RecalculateTotalPremium("Coverage removed");
        MarkAsUpdated();

        RaiseDomainEvent(new CoverageRemovedEvent(Id, coverageId, TenantId, coverage.Code));
    }

    /// <summary>
    /// Adds an endorsement to the policy.
    /// </summary>
    public Endorsement AddEndorsement(
        DateOnly effectiveDate,
        string type,
        string description,
        Money premiumChange,
        string? notes = null)
    {
        if (!Status.AllowsEndorsements())
            throw new BusinessRuleViolationException("Cannot add endorsement to a policy in this status.");

        if (!EffectivePeriod.Contains(effectiveDate))
            throw new BusinessRuleViolationException("Endorsement effective date must be within the policy period.");

        var endorsementNumber = GenerateEndorsementNumber();
        var endorsement = Endorsement.Create(Id, endorsementNumber, effectiveDate, type, description, premiumChange, notes);
        _endorsements.Add(endorsement);
        MarkAsUpdated();

        RaiseDomainEvent(new EndorsementAddedEvent(
            Id,
            endorsement.Id,
            TenantId,
            endorsementNumber,
            effectiveDate,
            premiumChange.Amount));

        return endorsement;
    }

    /// <summary>
    /// Gets an endorsement by its identifier.
    /// </summary>
    public Endorsement? GetEndorsement(Guid endorsementId)
    {
        return _endorsements.FirstOrDefault(e => e.Id == endorsementId);
    }

    /// <summary>
    /// Approves an endorsement.
    /// </summary>
    public void ApproveEndorsement(Guid endorsementId, Guid approvedBy)
    {
        var endorsement = _endorsements.FirstOrDefault(e => e.Id == endorsementId)
            ?? throw new BusinessRuleViolationException($"Endorsement with ID '{endorsementId}' not found.");

        endorsement.Approve(approvedBy);
        MarkAsUpdated();

        RaiseDomainEvent(new EndorsementApprovedEvent(Id, endorsementId, TenantId, endorsement.EndorsementNumber));
    }

    /// <summary>
    /// Issues an endorsement and applies premium change.
    /// </summary>
    public void IssueEndorsement(Guid endorsementId)
    {
        var endorsement = _endorsements.FirstOrDefault(e => e.Id == endorsementId)
            ?? throw new BusinessRuleViolationException($"Endorsement with ID '{endorsementId}' not found.");

        endorsement.Issue();

        // Apply premium change
        var newPremium = TotalPremium.Amount + endorsement.PremiumChange.Amount;
        if (newPremium < 0)
            throw new BusinessRuleViolationException("Endorsement would result in negative total premium.");

        var oldPremium = TotalPremium.Amount;
        TotalPremium = Money.Create(newPremium, TotalPremium.Currency);
        MarkAsUpdated();

        RaiseDomainEvent(new PolicyPremiumChangedEvent(
            Id,
            TenantId,
            oldPremium,
            newPremium,
            $"Endorsement {endorsement.EndorsementNumber}"));
    }

    /// <summary>
    /// Rejects an endorsement.
    /// </summary>
    public void RejectEndorsement(Guid endorsementId, Guid rejectedBy, string reason)
    {
        var endorsement = _endorsements.FirstOrDefault(e => e.Id == endorsementId)
            ?? throw new BusinessRuleViolationException($"Endorsement with ID '{endorsementId}' not found.");

        endorsement.Reject(rejectedBy, reason);
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets the carrier's policy number.
    /// </summary>
    public void SetCarrierPolicyNumber(string carrierPolicyNumber)
    {
        CarrierPolicyNumber = carrierPolicyNumber?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the billing information.
    /// </summary>
    public void UpdateBilling(BillingType billingType, PaymentPlan paymentPlan)
    {
        BillingType = billingType;
        PaymentPlan = paymentPlan;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets notes for the policy.
    /// </summary>
    public void SetNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Gets the total issued endorsement premium changes.
    /// </summary>
    public decimal GetTotalEndorsementPremium()
    {
        return _endorsements
            .Where(e => e.Status == EndorsementStatus.Issued)
            .Sum(e => e.PremiumChange.Amount);
    }

    /// <summary>
    /// Gets the base premium (total premium minus endorsement changes).
    /// </summary>
    public decimal GetBasePremium()
    {
        return TotalPremium.Amount - GetTotalEndorsementPremium();
    }

    /// <summary>
    /// Checks if the policy is currently in force.
    /// </summary>
    public bool IsInForce()
    {
        return Status.IsInForce() && EffectivePeriod.IsCurrentlyInForce;
    }

    private void RecalculateTotalPremium(string reason)
    {
        var oldPremium = TotalPremium.Amount;
        var basePremium = _coverages.Where(c => c.IsActive).Sum(c => c.PremiumAmount.Amount);
        var endorsementPremium = GetTotalEndorsementPremium();
        var newPremium = basePremium + endorsementPremium;

        TotalPremium = Money.Create(newPremium, TotalPremium.Currency);

        if (oldPremium != newPremium)
        {
            RaiseDomainEvent(new PolicyPremiumChangedEvent(Id, TenantId, oldPremium, newPremium, reason));
        }
    }

    private string GenerateEndorsementNumber()
    {
        var sequence = _endorsements.Count + 1;
        return $"{PolicyNumber.Value}-END{sequence:D3}";
    }
}
