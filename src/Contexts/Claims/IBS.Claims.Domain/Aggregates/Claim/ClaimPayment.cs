using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Domain.Aggregates.Claim;

/// <summary>
/// Represents a payment on a claim.
/// </summary>
public sealed class ClaimPayment : Entity
{
    /// <summary>
    /// Gets the claim identifier.
    /// </summary>
    public Guid ClaimId { get; private set; }

    /// <summary>
    /// Gets the payment type (e.g., "Indemnity", "Expense", "Legal").
    /// </summary>
    public string PaymentType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the payment amount.
    /// </summary>
    public Money Amount { get; private set; } = null!;

    /// <summary>
    /// Gets the payee name.
    /// </summary>
    public string PayeeName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the payment date.
    /// </summary>
    public DateOnly PaymentDate { get; private set; }

    /// <summary>
    /// Gets the check number (if applicable).
    /// </summary>
    public string? CheckNumber { get; private set; }

    /// <summary>
    /// Gets the payment status.
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// Gets the identifier of the user who authorized the payment.
    /// </summary>
    public string AuthorizedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date/time when the payment was authorized.
    /// </summary>
    public DateTimeOffset AuthorizedAt { get; private set; }

    /// <summary>
    /// Gets the date/time when the payment was issued.
    /// </summary>
    public DateTimeOffset? IssuedAt { get; private set; }

    /// <summary>
    /// Gets the date/time when the payment was voided.
    /// </summary>
    public DateTimeOffset? VoidedAt { get; private set; }

    /// <summary>
    /// Gets the reason for voiding the payment.
    /// </summary>
    public string? VoidReason { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private ClaimPayment() { }

    /// <summary>
    /// Creates a new authorized claim payment.
    /// </summary>
    /// <param name="claimId">The claim identifier.</param>
    /// <param name="paymentType">The payment type.</param>
    /// <param name="amount">The payment amount.</param>
    /// <param name="payeeName">The payee name.</param>
    /// <param name="paymentDate">The payment date.</param>
    /// <param name="authorizedBy">The user who authorized the payment.</param>
    /// <param name="checkNumber">Optional check number.</param>
    /// <returns>A new ClaimPayment instance.</returns>
    public static ClaimPayment Create(
        Guid claimId,
        string paymentType,
        Money amount,
        string payeeName,
        DateOnly paymentDate,
        string authorizedBy,
        string? checkNumber = null)
    {
        if (string.IsNullOrWhiteSpace(paymentType))
            throw new ArgumentException("Payment type is required.", nameof(paymentType));

        if (amount.Amount <= 0)
            throw new ArgumentException("Payment amount must be positive.", nameof(amount));

        if (string.IsNullOrWhiteSpace(payeeName))
            throw new ArgumentException("Payee name is required.", nameof(payeeName));

        if (string.IsNullOrWhiteSpace(authorizedBy))
            throw new ArgumentException("Authorized by is required.", nameof(authorizedBy));

        return new ClaimPayment
        {
            ClaimId = claimId,
            PaymentType = paymentType.Trim(),
            Amount = amount,
            PayeeName = payeeName.Trim(),
            PaymentDate = paymentDate,
            CheckNumber = checkNumber?.Trim(),
            Status = PaymentStatus.Authorized,
            AuthorizedBy = authorizedBy.Trim(),
            AuthorizedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Issues the payment.
    /// </summary>
    /// <exception cref="BusinessRuleViolationException">Thrown when payment cannot be issued.</exception>
    public void Issue()
    {
        if (Status != PaymentStatus.Authorized)
            throw new BusinessRuleViolationException("Only authorized payments can be issued.");

        Status = PaymentStatus.Issued;
        IssuedAt = DateTimeOffset.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Voids the payment.
    /// </summary>
    /// <param name="reason">The reason for voiding.</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when payment cannot be voided.</exception>
    public void Void(string reason)
    {
        if (Status == PaymentStatus.Voided)
            throw new BusinessRuleViolationException("Payment is already voided.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Void reason is required.", nameof(reason));

        Status = PaymentStatus.Voided;
        VoidedAt = DateTimeOffset.UtcNow;
        VoidReason = reason.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Gets whether this payment is pending (authorized but not yet issued or voided).
    /// </summary>
    public bool IsPending => Status == PaymentStatus.Authorized;
}
