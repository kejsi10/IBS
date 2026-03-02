using FluentValidation;

namespace IBS.Policies.Application.Commands.CreatePolicy;

/// <summary>
/// Validator for CreatePolicyCommand.
/// </summary>
public sealed class CreatePolicyCommandValidator : AbstractValidator<CreatePolicyCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public CreatePolicyCommandValidator()
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

        RuleFor(x => x.CarrierId)
            .NotEmpty()
            .WithMessage("Carrier ID is required.");

        RuleFor(x => x.PolicyType)
            .NotEmpty()
            .WithMessage("Policy type is required.")
            .MaximumLength(100)
            .WithMessage("Policy type must not exceed 100 characters.");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty()
            .WithMessage("Effective date is required.");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty()
            .WithMessage("Expiration date is required.")
            .GreaterThan(x => x.EffectiveDate)
            .WithMessage("Expiration date must be after effective date.");

        RuleFor(x => x.LineOfBusiness)
            .IsInEnum()
            .WithMessage("Invalid line of business.");

        RuleFor(x => x.BillingType)
            .IsInEnum()
            .WithMessage("Invalid billing type.");

        RuleFor(x => x.PaymentPlan)
            .IsInEnum()
            .WithMessage("Invalid payment plan.");
    }
}
