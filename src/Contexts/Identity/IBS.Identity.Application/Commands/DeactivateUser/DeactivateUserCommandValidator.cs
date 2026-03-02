using FluentValidation;

namespace IBS.Identity.Application.Commands.DeactivateUser;

/// <summary>
/// Validator for DeactivateUserCommand.
/// </summary>
public sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeactivateUserCommandValidator"/> class.
    /// </summary>
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User identifier is required.");
    }
}
