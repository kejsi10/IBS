using FluentValidation;

namespace IBS.Carriers.Application.Commands.DeactivateCarrier;

/// <summary>
/// Validator for DeactivateCarrierCommand.
/// </summary>
public sealed class DeactivateCarrierCommandValidator : AbstractValidator<DeactivateCarrierCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeactivateCarrierCommandValidator"/> class.
    /// </summary>
    public DeactivateCarrierCommandValidator()
    {
        RuleFor(x => x.CarrierId)
            .NotEmpty().WithMessage("Carrier ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}
