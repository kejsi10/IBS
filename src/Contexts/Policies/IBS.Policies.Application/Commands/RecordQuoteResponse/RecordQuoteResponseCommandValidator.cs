using FluentValidation;

namespace IBS.Policies.Application.Commands.RecordQuoteResponse;

/// <summary>
/// Validator for RecordQuoteResponseCommand.
/// </summary>
public sealed class RecordQuoteResponseCommandValidator : AbstractValidator<RecordQuoteResponseCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public RecordQuoteResponseCommandValidator()
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

        When(x => x.IsQuoted, () =>
        {
            RuleFor(x => x.PremiumAmount)
                .NotNull()
                .WithMessage("Premium amount is required when carrier provides a quote.")
                .GreaterThan(0)
                .WithMessage("Premium amount must be greater than zero.");

            RuleFor(x => x.PremiumCurrency)
                .NotEmpty()
                .WithMessage("Premium currency is required.")
                .Length(3)
                .WithMessage("Currency must be a 3-letter ISO code.");
        });

        When(x => !x.IsQuoted, () =>
        {
            RuleFor(x => x.DeclinationReason)
                .MaximumLength(500)
                .WithMessage("Declination reason must not exceed 500 characters.");
        });

        RuleFor(x => x.Conditions)
            .MaximumLength(2000)
            .WithMessage("Conditions must not exceed 2000 characters.");
    }
}
