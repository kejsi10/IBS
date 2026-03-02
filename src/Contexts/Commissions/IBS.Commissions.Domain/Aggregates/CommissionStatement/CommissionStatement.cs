using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Events;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Domain.Aggregates.CommissionStatement;

/// <summary>
/// Represents a commission statement from a carrier for a billing period.
/// This is the aggregate root for the CommissionStatement aggregate.
/// </summary>
public sealed class CommissionStatement : TenantAggregateRoot
{
    private readonly List<CommissionLineItem> _lineItems = [];
    private readonly List<ProducerSplit> _producerSplits = [];

    /// <summary>
    /// Gets the carrier identifier.
    /// </summary>
    public Guid CarrierId { get; private set; }

    /// <summary>
    /// Gets the carrier name.
    /// </summary>
    public string CarrierName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the statement number.
    /// </summary>
    public string StatementNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the period month (1-12).
    /// </summary>
    public int PeriodMonth { get; private set; }

    /// <summary>
    /// Gets the period year.
    /// </summary>
    public int PeriodYear { get; private set; }

    /// <summary>
    /// Gets the statement date.
    /// </summary>
    public DateOnly StatementDate { get; private set; }

    /// <summary>
    /// Gets the statement status.
    /// </summary>
    public StatementStatus Status { get; private set; }

    /// <summary>
    /// Gets the total premium amount.
    /// </summary>
    public Money TotalPremium { get; private set; } = null!;

    /// <summary>
    /// Gets the total commission amount.
    /// </summary>
    public Money TotalCommission { get; private set; } = null!;

    /// <summary>
    /// Gets the date/time when the statement was received.
    /// </summary>
    public DateTimeOffset ReceivedAt { get; private set; }

    /// <summary>
    /// Gets the line items on this statement.
    /// </summary>
    public IReadOnlyCollection<CommissionLineItem> LineItems => _lineItems.AsReadOnly();

    /// <summary>
    /// Gets the producer splits on this statement.
    /// </summary>
    public IReadOnlyCollection<ProducerSplit> ProducerSplits => _producerSplits.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private CommissionStatement() { }

    /// <summary>
    /// Creates a new commission statement.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="carrierId">The carrier identifier.</param>
    /// <param name="carrierName">The carrier name.</param>
    /// <param name="statementNumber">The statement number.</param>
    /// <param name="periodMonth">The period month (1-12).</param>
    /// <param name="periodYear">The period year.</param>
    /// <param name="statementDate">The statement date.</param>
    /// <param name="totalPremium">The total premium amount.</param>
    /// <param name="totalCommission">The total commission amount.</param>
    /// <returns>A new CommissionStatement instance.</returns>
    public static CommissionStatement Create(
        Guid tenantId,
        Guid carrierId,
        string carrierName,
        string statementNumber,
        int periodMonth,
        int periodYear,
        DateOnly statementDate,
        Money totalPremium,
        Money totalCommission)
    {
        if (string.IsNullOrWhiteSpace(carrierName))
            throw new ArgumentException("Carrier name is required.", nameof(carrierName));

        if (string.IsNullOrWhiteSpace(statementNumber))
            throw new ArgumentException("Statement number is required.", nameof(statementNumber));

        if (periodMonth < 1 || periodMonth > 12)
            throw new BusinessRuleViolationException("Period month must be between 1 and 12.");

        if (periodYear < 2000 || periodYear > 2100)
            throw new BusinessRuleViolationException("Period year must be between 2000 and 2100.");

        var statement = new CommissionStatement
        {
            TenantId = tenantId,
            CarrierId = carrierId,
            CarrierName = carrierName.Trim(),
            StatementNumber = statementNumber.Trim(),
            PeriodMonth = periodMonth,
            PeriodYear = periodYear,
            StatementDate = statementDate,
            Status = StatementStatus.Received,
            TotalPremium = totalPremium,
            TotalCommission = totalCommission,
            ReceivedAt = DateTimeOffset.UtcNow
        };

        statement.RaiseDomainEvent(new StatementReceivedEvent(
            statement.Id,
            statement.TenantId,
            statement.CarrierId,
            statement.CarrierName,
            statement.StatementNumber,
            statement.PeriodMonth,
            statement.PeriodYear));

        return statement;
    }

    /// <summary>
    /// Adds a line item to the statement.
    /// </summary>
    /// <param name="policyNumber">The policy number.</param>
    /// <param name="insuredName">The insured name.</param>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="effectiveDate">The effective date.</param>
    /// <param name="transactionType">The transaction type.</param>
    /// <param name="grossPremium">The gross premium.</param>
    /// <param name="commissionRate">The commission rate.</param>
    /// <param name="commissionAmount">The commission amount.</param>
    /// <param name="policyId">The optional policy identifier.</param>
    /// <returns>The created line item.</returns>
    public CommissionLineItem AddLineItem(
        string policyNumber,
        string insuredName,
        string lineOfBusiness,
        DateOnly effectiveDate,
        TransactionType transactionType,
        Money grossPremium,
        decimal commissionRate,
        Money commissionAmount,
        Guid? policyId = null)
    {
        var lineItem = CommissionLineItem.Create(
            Id, policyNumber, insuredName, lineOfBusiness,
            effectiveDate, transactionType, grossPremium,
            commissionRate, commissionAmount, policyId);

        _lineItems.Add(lineItem);
        MarkAsUpdated();
        return lineItem;
    }

    /// <summary>
    /// Reconciles a line item.
    /// </summary>
    /// <param name="lineItemId">The line item identifier.</param>
    public void ReconcileLineItem(Guid lineItemId)
    {
        var lineItem = GetLineItemOrThrow(lineItemId);
        lineItem.Reconcile();
        MarkAsUpdated();

        RaiseDomainEvent(new LineItemReconciledEvent(
            Id, TenantId, lineItemId, lineItem.PolicyNumber));
    }

    /// <summary>
    /// Disputes a line item.
    /// </summary>
    /// <param name="lineItemId">The line item identifier.</param>
    /// <param name="reason">The dispute reason.</param>
    public void DisputeLineItem(Guid lineItemId, string reason)
    {
        var lineItem = GetLineItemOrThrow(lineItemId);
        lineItem.Dispute(reason);
        MarkAsUpdated();

        RaiseDomainEvent(new LineItemDisputedEvent(
            Id, TenantId, lineItemId, lineItem.PolicyNumber, reason));
    }

    /// <summary>
    /// Adds a producer split for a line item.
    /// </summary>
    /// <param name="lineItemId">The line item identifier.</param>
    /// <param name="producerName">The producer name.</param>
    /// <param name="producerId">The producer identifier.</param>
    /// <param name="splitPercentage">The split percentage.</param>
    /// <param name="splitAmount">The split amount.</param>
    /// <returns>The created producer split.</returns>
    public ProducerSplit AddProducerSplit(
        Guid lineItemId,
        string producerName,
        Guid producerId,
        decimal splitPercentage,
        Money splitAmount)
    {
        // Verify line item exists
        GetLineItemOrThrow(lineItemId);

        // Validate total splits for this line item don't exceed 100%
        var existingSplitTotal = _producerSplits
            .Where(s => s.LineItemId == lineItemId)
            .Sum(s => s.SplitPercentage);

        if (existingSplitTotal + splitPercentage > 100)
            throw new BusinessRuleViolationException(
                $"Total producer splits for line item cannot exceed 100%. Current: {existingSplitTotal}%, requested: {splitPercentage}%.");

        var split = ProducerSplit.Create(Id, lineItemId, producerName, producerId, splitPercentage, splitAmount);
        _producerSplits.Add(split);
        MarkAsUpdated();

        RaiseDomainEvent(new ProducerCreditAssignedEvent(
            Id, TenantId, lineItemId, producerId, producerName, splitPercentage));

        return split;
    }

    /// <summary>
    /// Starts the reconciliation process.
    /// </summary>
    public void StartReconciling()
    {
        EnsureCanTransitionTo(StatementStatus.Reconciling);
        Status = StatementStatus.Reconciling;
        MarkAsUpdated();
    }

    /// <summary>
    /// Marks the statement as reconciled. Requires all line items to be reconciled or disputed.
    /// </summary>
    public void MarkReconciled()
    {
        EnsureCanTransitionTo(StatementStatus.Reconciled);

        if (_lineItems.Count == 0)
            throw new BusinessRuleViolationException("Cannot reconcile a statement with no line items.");

        var unreconciledCount = _lineItems.Count(li => !li.IsReconciled && li.DisputeReason == null);
        if (unreconciledCount > 0)
            throw new BusinessRuleViolationException(
                $"Cannot mark statement as reconciled. {unreconciledCount} line item(s) are neither reconciled nor disputed.");

        Status = StatementStatus.Reconciled;
        MarkAsUpdated();

        RaiseDomainEvent(new StatementReconciledEvent(Id, TenantId, StatementNumber));
    }

    /// <summary>
    /// Marks the statement as disputed.
    /// </summary>
    public void MarkDisputed()
    {
        EnsureCanTransitionTo(StatementStatus.Disputed);
        Status = StatementStatus.Disputed;
        MarkAsUpdated();
    }

    /// <summary>
    /// Marks the statement as paid.
    /// </summary>
    public void MarkPaid()
    {
        EnsureCanTransitionTo(StatementStatus.Paid);
        Status = StatementStatus.Paid;
        MarkAsUpdated();

        RaiseDomainEvent(new StatementPaidEvent(
            Id, TenantId, StatementNumber, TotalCommission.Amount, TotalCommission.Currency));
    }

    /// <summary>
    /// Reopens the statement for re-reconciliation (from Disputed).
    /// </summary>
    public void Reopen()
    {
        if (Status != StatementStatus.Disputed)
            throw new BusinessRuleViolationException("Only disputed statements can be reopened.");

        Status = StatementStatus.Reconciling;
        MarkAsUpdated();
    }

    private CommissionLineItem GetLineItemOrThrow(Guid lineItemId)
    {
        return _lineItems.FirstOrDefault(li => li.Id == lineItemId)
            ?? throw new BusinessRuleViolationException($"Line item with ID '{lineItemId}' not found.");
    }

    private void EnsureCanTransitionTo(StatementStatus targetStatus)
    {
        if (!Status.CanTransitionTo(targetStatus))
            throw new BusinessRuleViolationException(
                $"Cannot transition from '{Status.GetDisplayName()}' to '{targetStatus.GetDisplayName()}'.");
    }
}
