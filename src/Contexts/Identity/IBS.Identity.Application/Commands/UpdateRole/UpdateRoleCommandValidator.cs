using FluentValidation;

namespace IBS.Identity.Application.Commands.UpdateRole;

/// <summary>
/// Validator for UpdateRoleCommand.
/// </summary>
public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleCommandValidator"/> class.
    /// </summary>
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role identifier is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100).WithMessage("Role name must not exceed 100 characters.");
    }
}
