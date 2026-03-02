using FluentValidation;

namespace IBS.Identity.Application.Commands.RevokePermission;

/// <summary>
/// Validator for RevokePermissionCommand.
/// </summary>
public sealed class RevokePermissionCommandValidator : AbstractValidator<RevokePermissionCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RevokePermissionCommandValidator"/> class.
    /// </summary>
    public RevokePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role identifier is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("Permission identifier is required.");
    }
}
