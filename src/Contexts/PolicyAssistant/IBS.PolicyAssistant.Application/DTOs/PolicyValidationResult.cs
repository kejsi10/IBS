namespace IBS.PolicyAssistant.Application.DTOs;

/// <summary>
/// The result of validating extracted policy data against insurance regulations.
/// </summary>
/// <param name="IsValid">Whether the policy passes all validation checks.</param>
/// <param name="Issues">Blocking issues that must be resolved before a policy can be created.</param>
/// <param name="Warnings">Non-blocking warnings that may require attention.</param>
/// <param name="Summary">A human-readable summary of the validation result.</param>
public sealed record PolicyValidationResult(
    bool IsValid,
    IReadOnlyList<ValidationIssue> Issues,
    IReadOnlyList<string> Warnings,
    string Summary);

/// <summary>
/// A single validation issue found in the extracted policy data.
/// </summary>
/// <param name="Field">The policy field that has the issue.</param>
/// <param name="Rule">The rule or regulation that was violated.</param>
/// <param name="Description">A human-readable description of the issue.</param>
/// <param name="Severity">The severity of the issue (Error or Warning).</param>
public sealed record ValidationIssue(
    string Field,
    string Rule,
    string Description,
    string Severity);
