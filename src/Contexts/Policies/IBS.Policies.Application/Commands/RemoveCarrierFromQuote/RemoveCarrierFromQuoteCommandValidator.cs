using FluentValidation;

namespace IBS.Policies.Application.Commands.RemoveCarrierFromQuote;

/// <summary>
/// Validator for RemoveCarrierFromQuoteCommand.
/// </summary>
public sealed class RemoveCarrierFromQuoteCommandValidator : AbstractValidator<RemoveCarrierFromQuoteCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public RemoveCarrierFromQuoteCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.QuoteId)
            .NotEmpty()
            .WithMessage("Quote ID is required.");

        RuleFor(x => x.QuoteCarrierId)
            .NotEmpty()
            .WithMessage("Quote carrier ID is required.");
    }
}
