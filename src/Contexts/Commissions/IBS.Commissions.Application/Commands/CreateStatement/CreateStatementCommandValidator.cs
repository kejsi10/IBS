using FluentValidation;

namespace IBS.Commissions.Application.Commands.CreateStatement;

/// <summary>
/// Validator for CreateStatementCommand.
/// </summary>
public sealed class CreateStatementCommandValidator : AbstractValidator<CreateStatementCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public CreateStatementCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.CarrierId)
            .NotEmpty()
            .WithMessage("Carrier ID is required.");

        RuleFor(x => x.CarrierName)
            .NotEmpty()
            .WithMessage("Carrier name is required.")
            .MaximumLength(200)
            .WithMessage("Carrier name must not exceed 200 characters.");

        RuleFor(x => x.StatementNumber)
            .NotEmpty()
            .WithMessage("Statement number is required.")
            .MaximumLength(50)
            .WithMessage("Statement number must not exceed 50 characters.");

        RuleFor(x => x.PeriodMonth)
            .InclusiveBetween(1, 12)
            .WithMessage("Period month must be between 1 and 12.");

        RuleFor(x => x.PeriodYear)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Period year must be between 2000 and 2100.");

        RuleFor(x => x.StatementDate)
            .NotEmpty()
            .WithMessage("Statement date is required.");

        RuleFor(x => x.TotalPremium)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total premium must be non-negative.");

        RuleFor(x => x.TotalCommission)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total commission must be non-negative.");
    }
}
