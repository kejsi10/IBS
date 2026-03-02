using FluentValidation;

namespace IBS.Policies.Application.Commands.AddCoverage;

/// <summary>
/// Validator for AddCoverageCommand.
/// </summary>
public sealed class AddCoverageCommandValidator : AbstractValidator<AddCoverageCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public AddCoverageCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.PolicyId)
            .NotEmpty()
            .WithMessage("Policy ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Coverage code is required.")
            .MaximumLength(20)
            .WithMessage("Coverage code must not exceed 20 characters.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Coverage name is required.")
            .MaximumLength(200)
            .WithMessage("Coverage name must not exceed 200 characters.");

        RuleFor(x => x.PremiumAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Premium amount cannot be negative.");

        RuleFor(x => x.LimitAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LimitAmount.HasValue)
            .WithMessage("Limit amount cannot be negative.");

        RuleFor(x => x.DeductibleAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DeductibleAmount.HasValue)
            .WithMessage("Deductible amount cannot be negative.");
    }
}
