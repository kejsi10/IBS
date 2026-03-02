using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Domain.Aggregates.CommissionStatement;

/// <summary>
/// Represents a line item on a commission statement.
/// </summary>
public sealed class CommissionLineItem : Entity
{
    /// <summary>
    /// Gets the statement identifier.
    /// </summary>
    public Guid StatementId { get; private set; }

    /// <summary>
    /// Gets the optional policy identifier.
    /// </summary>
    public Guid? PolicyId { get; private set; }

    /// <summary>
    /// Gets the policy number.
    /// </summary>
    public string PolicyNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the insured name.
    /// </summary>
    public string InsuredName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the line of business.
    /// </summary>
    public string LineOfBusiness { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the policy effective date.
    /// </summary>
    public DateOnly EffectiveDate { get; private set; }

    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    public TransactionType TransactionType { get; private set; }

    /// <summary>
    /// Gets the gross premium.
    /// </summary>
    public Money GrossPremium { get; private set; } = null!;

    /// <summary>
    /// Gets the commission rate (0-100%).
    /// </summary>
    public decimal CommissionRate { get; private set; }

    /// <summary>
    /// Gets the commission amount.
    /// </summary>
    public Money CommissionAmount { get; private set; } = null!;

    /// <summary>
    /// Gets whether the line item has been reconciled.
    /// </summary>
    public bool IsReconciled { get; private set; }

    /// <summary>
    /// Gets the date/time when the line item was reconciled.
    /// </summary>
    public DateTimeOffset? ReconciledAt { get; private set; }

    /// <summary>
    /// Gets the dispute reason, if any.
    /// </summary>
    public string? DisputeReason { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private CommissionLineItem() { }

    /// <summary>
    /// Creates a new commission line item.
    /// </summary>
    /// <param name="statementId">The statement identifier.</param>
    /// <param name="policyNumber">The policy number.</param>
    /// <param name="insuredName">The insured name.</param>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="effectiveDate">The effective date.</param>
    /// <param name="transactionType">The transaction type.</param>
    /// <param name="grossPremium">The gross premium.</param>
    /// <param name="commissionRate">The commission rate.</param>
    /// <param name="commissionAmount">The commission amount.</param>
    /// <param name="policyId">The optional policy identifier.</param>
    /// <returns>A new CommissionLineItem.</returns>
    public static CommissionLineItem Create(
        Guid statementId,
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
        if (string.IsNullOrWhiteSpace(policyNumber))
            throw new ArgumentException("Policy number is required.", nameof(policyNumber));

        if (string.IsNullOrWhiteSpace(insuredName))
            throw new ArgumentException("Insured name is required.", nameof(insuredName));

        if (string.IsNullOrWhiteSpace(lineOfBusiness))
            throw new ArgumentException("Line of business is required.", nameof(lineOfBusiness));

        if (commissionRate < 0 || commissionRate > 100)
            throw new BusinessRuleViolationException($"Commission rate must be between 0 and 100%. Got: {commissionRate}");

        return new CommissionLineItem
        {
            StatementId = statementId,
            PolicyId = policyId,
            PolicyNumber = policyNumber.Trim(),
            InsuredName = insuredName.Trim(),
            LineOfBusiness = lineOfBusiness.Trim(),
            EffectiveDate = effectiveDate,
            TransactionType = transactionType,
            GrossPremium = grossPremium,
            CommissionRate = Math.Round(commissionRate, 4),
            CommissionAmount = commissionAmount,
            IsReconciled = false
        };
    }

    /// <summary>
    /// Reconciles this line item.
    /// </summary>
    public void Reconcile()
    {
        if (IsReconciled)
            throw new BusinessRuleViolationException("Line item is already reconciled.");

        IsReconciled = true;
        ReconciledAt = DateTimeOffset.UtcNow;
        DisputeReason = null;
    }

    /// <summary>
    /// Disputes this line item.
    /// </summary>
    /// <param name="reason">The dispute reason.</param>
    public void Dispute(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Dispute reason is required.", nameof(reason));

        IsReconciled = false;
        ReconciledAt = null;
        DisputeReason = reason.Trim();
    }

    /// <summary>
    /// Clears the dispute on this line item.
    /// </summary>
    public void ClearDispute()
    {
        DisputeReason = null;
    }
}
