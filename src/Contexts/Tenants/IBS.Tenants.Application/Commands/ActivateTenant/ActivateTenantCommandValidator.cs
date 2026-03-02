using FluentValidation;

namespace IBS.Tenants.Application.Commands.ActivateTenant;

/// <summary>
/// Validator for ActivateTenantCommand.
/// </summary>
public sealed class ActivateTenantCommandValidator : AbstractValidator<ActivateTenantCommand>
{
    /// <summary>
    /// Initializes a new instance of the ActivateTenantCommandValidator class.
    /// </summary>
    public ActivateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");
    }
}
