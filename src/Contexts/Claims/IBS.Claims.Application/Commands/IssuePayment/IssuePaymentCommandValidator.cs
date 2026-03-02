using FluentValidation;

namespace IBS.Claims.Application.Commands.IssuePayment;

/// <summary>
/// Validator for IssuePaymentCommand.
/// </summary>
public sealed class IssuePaymentCommandValidator : AbstractValidator<IssuePaymentCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public IssuePaymentCommandValidator()
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
    }
}
