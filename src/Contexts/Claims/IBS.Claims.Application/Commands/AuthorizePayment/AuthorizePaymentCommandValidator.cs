using FluentValidation;

namespace IBS.Claims.Application.Commands.AuthorizePayment;

/// <summary>
/// Validator for AuthorizePaymentCommand.
/// </summary>
public sealed class AuthorizePaymentCommandValidator : AbstractValidator<AuthorizePaymentCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public AuthorizePaymentCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithMessage("Claim ID is required.");

        RuleFor(x => x.PaymentType)
            .NotEmpty()
            .WithMessage("Payment type is required.")
            .MaximumLength(100)
            .WithMessage("Payment type must not exceed 100 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Payment amount must be positive.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.PayeeName)
            .NotEmpty()
            .WithMessage("Payee name is required.")
            .MaximumLength(200)
            .WithMessage("Payee name must not exceed 200 characters.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty()
            .WithMessage("Payment date is required.");
    }
}
