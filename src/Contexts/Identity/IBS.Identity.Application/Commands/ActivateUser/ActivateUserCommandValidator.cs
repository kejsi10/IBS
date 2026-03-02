using FluentValidation;

namespace IBS.Identity.Application.Commands.ActivateUser;

/// <summary>
/// Validator for ActivateUserCommand.
/// </summary>
public sealed class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActivateUserCommandValidator"/> class.
    /// </summary>
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User identifier is required.");
    }
}
