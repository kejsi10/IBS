using FluentValidation;

namespace IBS.Identity.Application.Commands.GrantPermission;

/// <summary>
/// Validator for GrantPermissionCommand.
/// </summary>
public sealed class GrantPermissionCommandValidator : AbstractValidator<GrantPermissionCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GrantPermissionCommandValidator"/> class.
    /// </summary>
    public GrantPermissionCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role identifier is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("Permission identifier is required.");
    }
}
