namespace IBS.PolicyAssistant.Application.DTOs;

/// <summary>
/// Structured policy data extracted from a conversation by the AI.
/// </summary>
/// <param name="IsComplete">Whether all required policy fields have been collected.</param>
/// <param name="ClientName">The client or insured name.</param>
/// <param name="CarrierName">The insurance carrier name.</param>
/// <param name="LineOfBusiness">The line of business (e.g., PersonalAuto, GeneralLiability).</param>
/// <param name="PolicyType">The policy type.</param>
/// <param name="EffectiveDate">The policy effective date (ISO 8601).</param>
/// <param name="ExpirationDate">The policy expiration date (ISO 8601).</param>
/// <param name="BillingType">The billing type (Agency or Direct).</param>
/// <param name="PaymentPlan">The payment plan.</param>
/// <param name="Coverages">The list of coverages extracted from the conversation.</param>
/// <param name="MissingFields">Fields required to complete the policy that have not yet been provided.</param>
public sealed record PolicyExtractionResult(
    bool IsComplete,
    string? ClientName,
    string? CarrierName,
    string? LineOfBusiness,
    string? PolicyType,
    string? EffectiveDate,
    string? ExpirationDate,
    string? BillingType,
    string? PaymentPlan,
    IReadOnlyList<ExtractedCoverage> Coverages,
    IReadOnlyList<string> MissingFields);

/// <summary>
/// A single coverage extracted from the conversation.
/// </summary>
/// <param name="Code">The coverage code (e.g., GL, BI, PD).</param>
/// <param name="Name">The coverage name.</param>
/// <param name="Premium">The premium amount as a string.</param>
/// <param name="Limit">The coverage limit as a string.</param>
/// <param name="Deductible">The deductible as a string.</param>
public sealed record ExtractedCoverage(
    string? Code,
    string? Name,
    string? Premium,
    string? Limit,
    string? Deductible);
