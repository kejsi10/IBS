using FluentValidation;

namespace IBS.Identity.Application.Commands.AssignRole;

/// <summary>
/// Validator for AssignRoleCommand.
/// </summary>
public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssignRoleCommandValidator"/> class.
    /// </summary>
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User identifier is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role identifier is required.");
    }
}
