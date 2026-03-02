using FluentValidation;

namespace IBS.Policies.Application.Commands.CancelQuote;

/// <summary>
/// Validator for CancelQuoteCommand.
/// </summary>
public sealed class CancelQuoteCommandValidator : AbstractValidator<CancelQuoteCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public CancelQuoteCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.QuoteId)
            .NotEmpty()
            .WithMessage("Quote ID is required.");
    }
}
