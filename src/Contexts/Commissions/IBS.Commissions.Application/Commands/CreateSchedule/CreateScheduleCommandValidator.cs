using FluentValidation;

namespace IBS.Commissions.Application.Commands.CreateSchedule;

/// <summary>
/// Validator for CreateScheduleCommand.
/// </summary>
public sealed class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public CreateScheduleCommandValidator()
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

        RuleFor(x => x.LineOfBusiness)
            .NotEmpty()
            .WithMessage("Line of business is required.")
            .MaximumLength(100)
            .WithMessage("Line of business must not exceed 100 characters.");

        RuleFor(x => x.NewBusinessRate)
            .InclusiveBetween(0, 100)
            .WithMessage("New business rate must be between 0 and 100.");

        RuleFor(x => x.RenewalRate)
            .InclusiveBetween(0, 100)
            .WithMessage("Renewal rate must be between 0 and 100.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty()
            .WithMessage("Effective from date is required.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("Effective to date must be on or after effective from date.");
    }
}
