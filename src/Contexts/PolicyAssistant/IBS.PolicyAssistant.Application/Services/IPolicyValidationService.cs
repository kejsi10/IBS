using IBS.PolicyAssistant.Application.DTOs;

namespace IBS.PolicyAssistant.Application.Services;

/// <summary>
/// Service for validating extracted policy data against insurance regulations and rules.
/// Uses AI + reference documents to check for compliance issues.
/// </summary>
public interface IPolicyValidationService
{
    /// <summary>
    /// Validates the extracted policy data against applicable regulations and rules.
    /// </summary>
    /// <param name="extracted">The extracted policy data to validate.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The validation result including any issues or warnings.</returns>
    Task<PolicyValidationResult> ValidateAsync(PolicyExtractionResult extracted, CancellationToken ct);
}
