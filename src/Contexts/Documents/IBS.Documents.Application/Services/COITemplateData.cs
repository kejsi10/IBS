namespace IBS.Documents.Application.Services;

/// <summary>
/// Data model used to render a Certificate of Insurance (COI) template.
/// </summary>
public sealed class COITemplateData
{
    /// <summary>Gets or sets the policy number.</summary>
    public string PolicyNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the client/insured name.</summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the carrier name.</summary>
    public string CarrierName { get; set; } = string.Empty;

    /// <summary>Gets or sets the policy effective date.</summary>
    public DateTimeOffset EffectiveDate { get; set; }

    /// <summary>Gets or sets the policy expiration date.</summary>
    public DateTimeOffset ExpirationDate { get; set; }

    /// <summary>Gets or sets the line of business.</summary>
    public string LineOfBusiness { get; set; } = string.Empty;

    /// <summary>Gets or sets the coverage summary lines.</summary>
    public string[] CoverageSummary { get; set; } = [];

    /// <summary>Gets or sets the broker name.</summary>
    public string BrokerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the issuance date.</summary>
    public DateTimeOffset IssuedDate { get; set; }
}
