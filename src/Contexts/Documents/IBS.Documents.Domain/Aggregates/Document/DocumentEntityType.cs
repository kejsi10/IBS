namespace IBS.Documents.Domain.Aggregates.Document;

/// <summary>
/// Represents the type of entity a document is linked to.
/// </summary>
public enum DocumentEntityType
{
    /// <summary>Document linked to a policy.</summary>
    Policy,

    /// <summary>Document linked to a client.</summary>
    Client,

    /// <summary>Document linked to a claim.</summary>
    Claim,

    /// <summary>Document linked to a carrier.</summary>
    Carrier,

    /// <summary>Document linked to a quote.</summary>
    Quote,

    /// <summary>General document not linked to a specific entity.</summary>
    General
}
