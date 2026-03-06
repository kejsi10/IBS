namespace IBS.Documents.Application.Services;

/// <summary>
/// Data model used to render a proposal document template via Handlebars.
/// All date fields are pre-formatted as strings for direct template output.
/// </summary>
public sealed class ProposalTemplateData
{
    /// <summary>Gets or sets the client name.</summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the client address (optional).</summary>
    public string? ClientAddress { get; set; }

    /// <summary>Gets or sets the line of business display name.</summary>
    public string LineOfBusiness { get; set; } = string.Empty;

    /// <summary>Gets or sets the effective date formatted as MM/dd/yyyy.</summary>
    public string EffectiveDate { get; set; } = string.Empty;

    /// <summary>Gets or sets the expiration date formatted as MM/dd/yyyy.</summary>
    public string ExpirationDate { get; set; } = string.Empty;

    /// <summary>Gets or sets optional notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the carrier offers to present.</summary>
    public IReadOnlyList<ProposalCarrierData> CarrierOffers { get; set; } = [];

    /// <summary>Gets or sets the date the proposal was generated, formatted as MM/dd/yyyy.</summary>
    public string GeneratedDate { get; set; } = string.Empty;
}

/// <summary>
/// Data for a single carrier offer row in the proposal template.
/// </summary>
public sealed class ProposalCarrierData
{
    /// <summary>Gets or sets the carrier name.</summary>
    public string CarrierName { get; set; } = string.Empty;

    /// <summary>Gets or sets the offer status display value (e.g., Quoted, Declined, Pending).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the premium formatted as currency, or null if not yet quoted.</summary>
    public string? PremiumAmount { get; set; }

    /// <summary>Gets or sets any conditions attached to the offer.</summary>
    public string? Conditions { get; set; }

    /// <summary>Gets or sets the proposed coverages description.</summary>
    public string? ProposedCoverages { get; set; }
}
