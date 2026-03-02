using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Domain.Aggregates.Quote;

/// <summary>
/// Represents a carrier's participation in a quote request.
/// </summary>
public sealed class QuoteCarrier : Entity
{
    /// <summary>
    /// Gets the parent quote identifier.
    /// </summary>
    public Guid QuoteId { get; private set; }

    /// <summary>
    /// Gets the carrier identifier.
    /// </summary>
    public Guid CarrierId { get; private set; }

    /// <summary>
    /// Gets the carrier's response status.
    /// </summary>
    public QuoteCarrierStatus Status { get; private set; }

    /// <summary>
    /// Gets the quoted premium amount.
    /// </summary>
    public decimal? PremiumAmount { get; private set; }

    /// <summary>
    /// Gets the quoted premium currency.
    /// </summary>
    public string? PremiumCurrency { get; private set; }

    /// <summary>
    /// Gets the reason the carrier declined to quote.
    /// </summary>
    public string? DeclinationReason { get; private set; }

    /// <summary>
    /// Gets any conditions attached to the carrier's quote.
    /// </summary>
    public string? Conditions { get; private set; }

    /// <summary>
    /// Gets the proposed coverages as JSON.
    /// </summary>
    public string? ProposedCoverages { get; private set; }

    /// <summary>
    /// Gets the date/time the carrier responded.
    /// </summary>
    public DateTimeOffset? RespondedAt { get; private set; }

    /// <summary>
    /// Gets the date the carrier's quote expires.
    /// </summary>
    public DateOnly? ExpiresAt { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private QuoteCarrier() { }

    /// <summary>
    /// Creates a new quote carrier entry.
    /// </summary>
    /// <param name="quoteId">The parent quote identifier.</param>
    /// <param name="carrierId">The carrier identifier.</param>
    /// <returns>A new QuoteCarrier instance.</returns>
    internal static QuoteCarrier Create(Guid quoteId, Guid carrierId)
    {
        return new QuoteCarrier
        {
            QuoteId = quoteId,
            CarrierId = carrierId,
            Status = QuoteCarrierStatus.Pending
        };
    }

    /// <summary>
    /// Records a quoted response from the carrier.
    /// </summary>
    /// <param name="premiumAmount">The quoted premium amount.</param>
    /// <param name="premiumCurrency">The premium currency code.</param>
    /// <param name="conditions">Any conditions attached to the quote.</param>
    /// <param name="proposedCoverages">Proposed coverages as JSON.</param>
    /// <param name="expiresAt">When the carrier's quote expires.</param>
    internal void RecordQuoted(
        decimal premiumAmount,
        string premiumCurrency = "USD",
        string? conditions = null,
        string? proposedCoverages = null,
        DateOnly? expiresAt = null)
    {
        if (Status != QuoteCarrierStatus.Pending)
            throw new BusinessRuleViolationException("Can only record a response for a pending carrier.");

        if (premiumAmount <= 0)
            throw new BusinessRuleViolationException("Premium amount must be greater than zero.");

        Status = QuoteCarrierStatus.Quoted;
        PremiumAmount = premiumAmount;
        PremiumCurrency = premiumCurrency;
        Conditions = conditions?.Trim();
        ProposedCoverages = proposedCoverages;
        ExpiresAt = expiresAt;
        RespondedAt = DateTimeOffset.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records a declination from the carrier.
    /// </summary>
    /// <param name="reason">The reason for declination.</param>
    internal void RecordDeclined(string? reason = null)
    {
        if (Status != QuoteCarrierStatus.Pending)
            throw new BusinessRuleViolationException("Can only record a response for a pending carrier.");

        Status = QuoteCarrierStatus.Declined;
        DeclinationReason = reason?.Trim();
        RespondedAt = DateTimeOffset.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Marks the carrier's quote as expired.
    /// </summary>
    internal void Expire()
    {
        if (Status != QuoteCarrierStatus.Quoted)
            throw new BusinessRuleViolationException("Only quoted carriers can expire.");

        Status = QuoteCarrierStatus.Expired;
        MarkAsUpdated();
    }
}
