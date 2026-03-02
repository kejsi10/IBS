using FluentValidation;

namespace IBS.Claims.Application.Commands.SetReserve;

/// <summary>
/// Validator for SetReserveCommand.
/// </summary>
public sealed class SetReserveCommandValidator : AbstractValidator<SetReserveCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public SetReserveCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithMessage("Claim ID is required.");

        RuleFor(x => x.ReserveType)
            .NotEmpty()
            .WithMessage("Reserve type is required.")
            .MaximumLength(100)
            .WithMessage("Reserve type must not exceed 100 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Reserve amount must be positive.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code.");
    }
}
