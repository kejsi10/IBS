using FluentValidation;

namespace IBS.Policies.Application.Commands.SubmitQuote;

/// <summary>
/// Validator for SubmitQuoteCommand.
/// </summary>
public sealed class SubmitQuoteCommandValidator : AbstractValidator<SubmitQuoteCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public SubmitQuoteCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.QuoteId)
            .NotEmpty()
            .WithMessage("Quote ID is required.");
    }
}
