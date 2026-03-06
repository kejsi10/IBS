namespace IBS.Documents.Domain.Aggregates.DocumentTemplate;

/// <summary>
/// Represents the type of a document template.
/// </summary>
public enum TemplateType
{
    /// <summary>Certificate of Insurance template.</summary>
    CertificateOfInsurance,

    /// <summary>Policy summary template.</summary>
    PolicySummary,

    /// <summary>Proposal document template for presenting carrier offers to clients.</summary>
    Proposal
}
