namespace IBS.Documents.Domain.Aggregates.Document;

/// <summary>
/// Represents the category of a document.
/// </summary>
public enum DocumentCategory
{
    /// <summary>Policy document.</summary>
    Policy,

    /// <summary>Endorsement document.</summary>
    Endorsement,

    /// <summary>Certificate of Insurance.</summary>
    COI,

    /// <summary>Claim report document.</summary>
    ClaimReport,

    /// <summary>Know Your Customer (KYC) document.</summary>
    KYC,

    /// <summary>Invoice document.</summary>
    Invoice,

    /// <summary>Proposal document presenting carrier renewal/new business offers.</summary>
    Proposal,

    /// <summary>Other/unclassified document.</summary>
    Other
}
