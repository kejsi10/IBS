using FluentValidation;

namespace IBS.Policies.Application.Commands.AddCarrierToQuote;

/// <summary>
/// Validator for AddCarrierToQuoteCommand.
/// </summary>
public sealed class AddCarrierToQuoteCommandValidator : AbstractValidator<AddCarrierToQuoteCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public AddCarrierToQuoteCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.QuoteId)
            .NotEmpty()
            .WithMessage("Quote ID is required.");

        RuleFor(x => x.CarrierId)
            .NotEmpty()
            .WithMessage("Carrier ID is required.");
    }
}
