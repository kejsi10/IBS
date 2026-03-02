using FluentValidation;

namespace IBS.Policies.Application.Commands.AcceptQuoteCarrier;

/// <summary>
/// Validator for AcceptQuoteCarrierCommand.
/// </summary>
public sealed class AcceptQuoteCarrierCommandValidator : AbstractValidator<AcceptQuoteCarrierCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public AcceptQuoteCarrierCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.QuoteId)
            .NotEmpty()
            .WithMessage("Quote ID is required.");

        RuleFor(x => x.QuoteCarrierId)
            .NotEmpty()
            .WithMessage("Quote carrier ID is required.");
    }
}
