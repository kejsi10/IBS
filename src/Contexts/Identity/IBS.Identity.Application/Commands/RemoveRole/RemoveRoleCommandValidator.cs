using FluentValidation;

namespace IBS.Identity.Application.Commands.RemoveRole;

/// <summary>
/// Validator for RemoveRoleCommand.
/// </summary>
public sealed class RemoveRoleCommandValidator : AbstractValidator<RemoveRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveRoleCommandValidator"/> class.
    /// </summary>
    public RemoveRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User identifier is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role identifier is required.");
    }
}
