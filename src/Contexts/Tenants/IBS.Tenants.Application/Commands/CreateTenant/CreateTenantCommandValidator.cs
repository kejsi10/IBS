using FluentValidation;

namespace IBS.Tenants.Application.Commands.CreateTenant;

/// <summary>
/// Validator for CreateTenantCommand.
/// </summary>
public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    /// <summary>
    /// Initializes a new instance of the CreateTenantCommandValidator class.
    /// </summary>
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(255).WithMessage("Tenant name cannot exceed 255 characters.");

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required.")
            .MinimumLength(3).WithMessage("Subdomain must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Subdomain cannot exceed 50 characters.")
            .Matches("^[a-z0-9-]+$").WithMessage("Subdomain can only contain lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.SubscriptionTier)
            .IsInEnum().WithMessage("Invalid subscription tier.");
    }
}
