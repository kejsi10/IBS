using FluentValidation;

namespace IBS.Policies.Application.Commands.CancelPolicy;

/// <summary>
/// Validator for CancelPolicyCommand.
/// </summary>
public sealed class CancelPolicyCommandValidator : AbstractValidator<CancelPolicyCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public CancelPolicyCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.PolicyId)
            .NotEmpty()
            .WithMessage("Policy ID is required.");

        RuleFor(x => x.CancellationDate)
            .NotEmpty()
            .WithMessage("Cancellation date is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Cancellation reason is required.")
            .MaximumLength(1000)
            .WithMessage("Cancellation reason must not exceed 1000 characters.");

        RuleFor(x => x.CancellationType)
            .IsInEnum()
            .WithMessage("Invalid cancellation type.");
    }
}
