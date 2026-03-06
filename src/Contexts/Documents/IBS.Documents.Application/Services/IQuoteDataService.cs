namespace IBS.Documents.Application.Services;

/// <summary>
/// Service for fetching quote data needed to generate proposal documents.
/// </summary>
public interface IQuoteDataService
{
    /// <summary>
    /// Gets quote data required for generating a proposal PDF.
    /// </summary>
    /// <param name="quoteId">The quote identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Quote proposal data, or null if the quote was not found.</returns>
    Task<QuoteProposalData?> GetQuoteProposalDataAsync(Guid quoteId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data for generating a proposal PDF document.
/// </summary>
public sealed class QuoteProposalData
{
    /// <summary>Gets or sets the client name.</summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the client address.</summary>
    public string? ClientAddress { get; set; }

    /// <summary>Gets or sets the line of business display name.</summary>
    public string LineOfBusiness { get; set; } = string.Empty;

    /// <summary>Gets or sets the requested effective date.</summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>Gets or sets the requested expiration date.</summary>
    public DateOnly ExpirationDate { get; set; }

    /// <summary>Gets or sets optional notes for the proposal.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the carrier offers to include in the proposal.</summary>
    public IReadOnlyList<QuoteCarrierProposalData> CarrierOffers { get; set; } = [];
}

/// <summary>
/// Data for a single carrier's offer in a proposal.
/// </summary>
public sealed class QuoteCarrierProposalData
{
    /// <summary>Gets or sets the carrier name.</summary>
    public string CarrierName { get; set; } = string.Empty;

    /// <summary>Gets or sets the offered premium amount.</summary>
    public decimal? PremiumAmount { get; set; }

    /// <summary>Gets or sets any conditions attached to the offer.</summary>
    public string? Conditions { get; set; }

    /// <summary>Gets or sets the proposed coverages description.</summary>
    public string? ProposedCoverages { get; set; }

    /// <summary>Gets or sets the offer status (e.g., Quoted, Declined, Pending).</summary>
    public string Status { get; set; } = string.Empty;
}
