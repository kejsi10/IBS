using FluentValidation;

namespace IBS.Policies.Application.Commands.UpdateCoverage;

/// <summary>
/// Validator for the UpdateCoverageCommand.
/// </summary>
public sealed class UpdateCoverageCommandValidator : AbstractValidator<UpdateCoverageCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCoverageCommandValidator"/> class.
    /// </summary>
    public UpdateCoverageCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.PolicyId)
            .NotEmpty()
            .WithMessage("Policy ID is required.");

        RuleFor(x => x.CoverageId)
            .NotEmpty()
            .WithMessage("Coverage ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Coverage name is required.")
            .MaximumLength(200)
            .WithMessage("Coverage name must not exceed 200 characters.");

        RuleFor(x => x.PremiumAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Premium amount must be zero or greater.");

        RuleFor(x => x.LimitAmount)
            .GreaterThan(0)
            .When(x => x.LimitAmount.HasValue)
            .WithMessage("Limit amount must be greater than zero.");

        RuleFor(x => x.DeductibleAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DeductibleAmount.HasValue)
            .WithMessage("Deductible amount must be zero or greater.");
    }
}
