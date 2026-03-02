using FluentValidation;

namespace IBS.Tenants.Application.Commands.UpdateTenant;

/// <summary>
/// Validator for UpdateTenantCommand.
/// </summary>
public sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    /// <summary>
    /// Initializes a new instance of the UpdateTenantCommandValidator class.
    /// </summary>
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(255).WithMessage("Tenant name cannot exceed 255 characters.");
    }
}
