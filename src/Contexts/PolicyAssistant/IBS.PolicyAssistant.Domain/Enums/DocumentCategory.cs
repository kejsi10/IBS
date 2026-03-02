namespace IBS.PolicyAssistant.Domain.Enums;

/// <summary>
/// Defines the category of a reference document used for AI-assisted policy validation.
/// </summary>
public enum DocumentCategory
{
    /// <summary>
    /// State insurance regulation or statutory requirement.
    /// </summary>
    Regulation,

    /// <summary>
    /// A sample policy document used as a template or example.
    /// </summary>
    SamplePolicy,

    /// <summary>
    /// A validation rule used to check policy data for correctness.
    /// </summary>
    ValidationRule
}
