using FluentValidation;

namespace IBS.Policies.Application.Commands.CreateQuote;

/// <summary>
/// Validator for CreateQuoteCommand.
/// </summary>
public sealed class CreateQuoteCommandValidator : AbstractValidator<CreateQuoteCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public CreateQuoteCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.LineOfBusiness)
            .IsInEnum()
            .WithMessage("Invalid line of business.");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty()
            .WithMessage("Effective date is required.");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty()
            .WithMessage("Expiration date is required.")
            .GreaterThan(x => x.EffectiveDate)
            .WithMessage("Expiration date must be after effective date.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes must not exceed 2000 characters.");
    }
}
