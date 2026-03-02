using FluentValidation;

namespace IBS.Tenants.Application.Commands.UpdateSubscriptionTier;

/// <summary>
/// Validator for UpdateSubscriptionTierCommand.
/// </summary>
public sealed class UpdateSubscriptionTierCommandValidator : AbstractValidator<UpdateSubscriptionTierCommand>
{
    /// <summary>
    /// Initializes a new instance of the UpdateSubscriptionTierCommandValidator class.
    /// </summary>
    public UpdateSubscriptionTierCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.SubscriptionTier)
            .IsInEnum().WithMessage("Invalid subscription tier.");
    }
}
