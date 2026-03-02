using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Events;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Domain.Aggregates.Quote;

/// <summary>
/// Represents a quote request sent to one or more carriers.
/// This is the aggregate root for the Quote aggregate.
/// </summary>
public sealed class Quote : TenantAggregateRoot
{
    private readonly List<QuoteCarrier> _carriers = [];

    /// <summary>
    /// Gets the client (insured) identifier.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the line of business.
    /// </summary>
    public LineOfBusiness LineOfBusiness { get; private set; }

    /// <summary>
    /// Gets the requested effective period.
    /// </summary>
    public EffectivePeriod EffectivePeriod { get; private set; } = null!;

    /// <summary>
    /// Gets the quote status.
    /// </summary>
    public QuoteStatus Status { get; private set; }

    /// <summary>
    /// Gets the date the quote expires.
    /// </summary>
    public DateOnly ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the accepted carrier identifier.
    /// </summary>
    public Guid? AcceptedCarrierId { get; private set; }

    /// <summary>
    /// Gets the policy identifier created from this quote.
    /// </summary>
    public Guid? PolicyId { get; private set; }

    /// <summary>
    /// Gets any notes for the quote.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets the user who created this quote.
    /// </summary>
    public Guid CreatedBy { get; private set; }

    /// <summary>
    /// Gets the carriers on this quote.
    /// </summary>
    public IReadOnlyCollection<QuoteCarrier> Carriers => _carriers.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Quote() { }

    /// <summary>
    /// Creates a new quote.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="effectivePeriod">The requested effective period.</param>
    /// <param name="createdBy">The user creating the quote.</param>
    /// <param name="notes">Optional notes.</param>
    /// <param name="expiresAt">Optional quote expiry date (defaults to 30 days from now).</param>
    /// <returns>A new Quote instance.</returns>
    public static Quote Create(
        Guid tenantId,
        Guid clientId,
        LineOfBusiness lineOfBusiness,
        EffectivePeriod effectivePeriod,
        Guid createdBy,
        string? notes = null,
        DateOnly? expiresAt = null)
    {
        var quote = new Quote
        {
            TenantId = tenantId,
            ClientId = clientId,
            LineOfBusiness = lineOfBusiness,
            EffectivePeriod = effectivePeriod,
            Status = QuoteStatus.Draft,
            ExpiresAt = expiresAt ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            CreatedBy = createdBy,
            Notes = notes?.Trim()
        };

        quote.RaiseDomainEvent(new QuoteCreatedEvent(
            quote.Id,
            quote.TenantId,
            quote.ClientId,
            quote.LineOfBusiness,
            quote.EffectivePeriod.EffectiveDate,
            quote.EffectivePeriod.ExpirationDate));

        return quote;
    }

    /// <summary>
    /// Adds a carrier to the quote.
    /// </summary>
    /// <param name="carrierId">The carrier identifier.</param>
    /// <returns>The created QuoteCarrier.</returns>
    public QuoteCarrier AddCarrier(Guid carrierId)
    {
        if (!Status.AllowsCarrierChanges())
            throw new BusinessRuleViolationException("Carriers can only be added to draft quotes.");

        if (_carriers.Any(c => c.CarrierId == carrierId))
            throw new BusinessRuleViolationException("This carrier has already been added to the quote.");

        var quoteCarrier = QuoteCarrier.Create(Id, carrierId);
        _carriers.Add(quoteCarrier);
        MarkAsUpdated();

        return quoteCarrier;
    }

    /// <summary>
    /// Removes a carrier from the quote.
    /// </summary>
    /// <param name="quoteCarrierId">The quote carrier identifier.</param>
    public void RemoveCarrier(Guid quoteCarrierId)
    {
        if (!Status.AllowsCarrierChanges())
            throw new BusinessRuleViolationException("Carriers can only be removed from draft quotes.");

        var carrier = _carriers.FirstOrDefault(c => c.Id == quoteCarrierId)
            ?? throw new BusinessRuleViolationException($"Quote carrier with ID '{quoteCarrierId}' not found.");

        _carriers.Remove(carrier);
        MarkAsUpdated();
    }

    /// <summary>
    /// Submits the quote to carriers.
    /// </summary>
    public void Submit()
    {
        if (Status != QuoteStatus.Draft)
            throw new BusinessRuleViolationException("Only draft quotes can be submitted.");

        if (_carriers.Count == 0)
            throw new BusinessRuleViolationException("Quote must have at least one carrier before submitting.");

        Status = QuoteStatus.Submitted;
        MarkAsUpdated();

        RaiseDomainEvent(new QuoteSubmittedEvent(
            Id,
            TenantId,
            _carriers.Select(c => c.CarrierId).ToList()));
    }

    /// <summary>
    /// Records a quoted response from a carrier.
    /// </summary>
    /// <param name="quoteCarrierId">The quote carrier identifier.</param>
    /// <param name="premiumAmount">The quoted premium amount.</param>
    /// <param name="premiumCurrency">The premium currency.</param>
    /// <param name="conditions">Any conditions.</param>
    /// <param name="proposedCoverages">Proposed coverages as JSON.</param>
    /// <param name="expiresAt">When the carrier's quote expires.</param>
    public void RecordQuotedResponse(
        Guid quoteCarrierId,
        decimal premiumAmount,
        string premiumCurrency = "USD",
        string? conditions = null,
        string? proposedCoverages = null,
        DateOnly? expiresAt = null)
    {
        if (Status != QuoteStatus.Submitted && Status != QuoteStatus.Quoted)
            throw new BusinessRuleViolationException("Can only record responses for submitted or quoted quotes.");

        var carrier = _carriers.FirstOrDefault(c => c.Id == quoteCarrierId)
            ?? throw new BusinessRuleViolationException($"Quote carrier with ID '{quoteCarrierId}' not found.");

        carrier.RecordQuoted(premiumAmount, premiumCurrency, conditions, proposedCoverages, expiresAt);
        EvaluateStatus();
        MarkAsUpdated();

        RaiseDomainEvent(new QuoteResponseReceivedEvent(
            Id, TenantId, carrier.CarrierId, QuoteCarrierStatus.Quoted, premiumAmount));
    }

    /// <summary>
    /// Records a declination from a carrier.
    /// </summary>
    /// <param name="quoteCarrierId">The quote carrier identifier.</param>
    /// <param name="reason">The declination reason.</param>
    public void RecordDeclinedResponse(Guid quoteCarrierId, string? reason = null)
    {
        if (Status != QuoteStatus.Submitted && Status != QuoteStatus.Quoted)
            throw new BusinessRuleViolationException("Can only record responses for submitted or quoted quotes.");

        var carrier = _carriers.FirstOrDefault(c => c.Id == quoteCarrierId)
            ?? throw new BusinessRuleViolationException($"Quote carrier with ID '{quoteCarrierId}' not found.");

        carrier.RecordDeclined(reason);
        EvaluateStatus();
        MarkAsUpdated();

        RaiseDomainEvent(new QuoteResponseReceivedEvent(
            Id, TenantId, carrier.CarrierId, QuoteCarrierStatus.Declined, null));
    }

    /// <summary>
    /// Accepts a carrier's quote and links to a created policy.
    /// </summary>
    /// <param name="quoteCarrierId">The quote carrier identifier to accept.</param>
    /// <param name="policyId">The created policy identifier.</param>
    public void Accept(Guid quoteCarrierId, Guid policyId)
    {
        if (Status != QuoteStatus.Quoted && Status != QuoteStatus.Submitted)
            throw new BusinessRuleViolationException("Only quoted or submitted quotes can be accepted.");

        if (ExpiresAt < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleViolationException("Cannot accept an expired quote.");

        var carrier = _carriers.FirstOrDefault(c => c.Id == quoteCarrierId)
            ?? throw new BusinessRuleViolationException($"Quote carrier with ID '{quoteCarrierId}' not found.");

        if (carrier.Status != QuoteCarrierStatus.Quoted)
            throw new BusinessRuleViolationException("Can only accept a carrier that has provided a quote.");

        Status = QuoteStatus.Accepted;
        AcceptedCarrierId = carrier.CarrierId;
        PolicyId = policyId;
        MarkAsUpdated();

        RaiseDomainEvent(new QuoteAcceptedEvent(
            Id, TenantId, carrier.CarrierId, policyId, carrier.PremiumAmount!.Value));
    }

    /// <summary>
    /// Cancels the quote.
    /// </summary>
    public void Cancel()
    {
        if (!Status.CanBeCancelled())
            throw new BusinessRuleViolationException("Only draft or submitted quotes can be cancelled.");

        Status = QuoteStatus.Cancelled;
        MarkAsUpdated();

        RaiseDomainEvent(new QuoteCancelledEvent(Id, TenantId));
    }

    /// <summary>
    /// Expires the quote.
    /// </summary>
    public void Expire()
    {
        if (Status.IsTerminal())
            throw new BusinessRuleViolationException("Cannot expire a quote that is already in a terminal state.");

        Status = QuoteStatus.Expired;
        MarkAsUpdated();

        RaiseDomainEvent(new QuoteExpiredEvent(Id, TenantId, ExpiresAt));
    }

    /// <summary>
    /// Gets a carrier by its identifier.
    /// </summary>
    /// <param name="quoteCarrierId">The quote carrier identifier.</param>
    /// <returns>The QuoteCarrier if found; otherwise null.</returns>
    public QuoteCarrier? GetCarrier(Guid quoteCarrierId)
    {
        return _carriers.FirstOrDefault(c => c.Id == quoteCarrierId);
    }

    private void EvaluateStatus()
    {
        var allResponded = _carriers.All(c => c.Status != QuoteCarrierStatus.Pending);
        var anyQuoted = _carriers.Any(c => c.Status == QuoteCarrierStatus.Quoted);

        if (allResponded && anyQuoted)
        {
            Status = QuoteStatus.Quoted;
        }
    }
}
