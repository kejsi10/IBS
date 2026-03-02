using FluentValidation;

namespace IBS.Carriers.Application.Commands.AddAppetite;

/// <summary>
/// Validator for AddAppetiteCommand.
/// </summary>
public sealed class AddAppetiteCommandValidator : AbstractValidator<AddAppetiteCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddAppetiteCommandValidator"/> class.
    /// </summary>
    public AddAppetiteCommandValidator()
    {
        RuleFor(x => x.CarrierId)
            .NotEmpty().WithMessage("Carrier ID is required.");

        RuleFor(x => x.LineOfBusiness)
            .IsInEnum().WithMessage("Invalid line of business.");

        RuleFor(x => x.States)
            .NotEmpty().WithMessage("States are required.")
            .MaximumLength(500).WithMessage("States cannot exceed 500 characters.");

        RuleFor(x => x.MinYearsInBusiness)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum years in business cannot be negative.")
            .When(x => x.MinYearsInBusiness.HasValue);

        RuleFor(x => x.MaxYearsInBusiness)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum years in business cannot be negative.")
            .GreaterThanOrEqualTo(x => x.MinYearsInBusiness)
            .WithMessage("Maximum years in business must be greater than or equal to minimum.")
            .When(x => x.MaxYearsInBusiness.HasValue && x.MinYearsInBusiness.HasValue);

        RuleFor(x => x.MinAnnualRevenue)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum annual revenue cannot be negative.")
            .When(x => x.MinAnnualRevenue.HasValue);

        RuleFor(x => x.MaxAnnualRevenue)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum annual revenue cannot be negative.")
            .GreaterThanOrEqualTo(x => x.MinAnnualRevenue)
            .WithMessage("Maximum annual revenue must be greater than or equal to minimum.")
            .When(x => x.MaxAnnualRevenue.HasValue && x.MinAnnualRevenue.HasValue);

        RuleFor(x => x.MinEmployees)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum employees cannot be negative.")
            .When(x => x.MinEmployees.HasValue);

        RuleFor(x => x.MaxEmployees)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum employees cannot be negative.")
            .GreaterThanOrEqualTo(x => x.MinEmployees)
            .WithMessage("Maximum employees must be greater than or equal to minimum.")
            .When(x => x.MaxEmployees.HasValue && x.MinEmployees.HasValue);

        RuleFor(x => x.AcceptedIndustries)
            .MaximumLength(1000).WithMessage("Accepted industries cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.AcceptedIndustries));

        RuleFor(x => x.ExcludedIndustries)
            .MaximumLength(1000).WithMessage("Excluded industries cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.ExcludedIndustries));

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
