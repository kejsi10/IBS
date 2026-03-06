using FluentValidation;

namespace IBS.Policies.Application.Commands.ReinstatePolicy;

/// <summary>
/// Validator for ReinstatePolicyCommand.
/// </summary>
public sealed class ReinstatePolicyCommandValidator : AbstractValidator<ReinstatePolicyCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public ReinstatePolicyCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.PolicyId)
            .NotEmpty()
            .WithMessage("Policy ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reinstatement reason is required.")
            .MaximumLength(1000)
            .WithMessage("Reinstatement reason must not exceed 1000 characters.");
    }
}
