using FluentValidation;

namespace IBS.Commissions.Application.Commands.DeactivateSchedule;

/// <summary>
/// Validator for DeactivateScheduleCommand.
/// </summary>
public sealed class DeactivateScheduleCommandValidator : AbstractValidator<DeactivateScheduleCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public DeactivateScheduleCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID is required.");
    }
}
