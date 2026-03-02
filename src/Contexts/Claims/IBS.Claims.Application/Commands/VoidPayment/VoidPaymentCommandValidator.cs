using FluentValidation;

namespace IBS.Claims.Application.Commands.VoidPayment;

/// <summary>
/// Validator for VoidPaymentCommand.
/// </summary>
public sealed class VoidPaymentCommandValidator : AbstractValidator<VoidPaymentCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public VoidPaymentCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithMessage("Claim ID is required.");

        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Void reason is required.")
            .MaximumLength(1000)
            .WithMessage("Void reason must not exceed 1000 characters.");
    }
}
