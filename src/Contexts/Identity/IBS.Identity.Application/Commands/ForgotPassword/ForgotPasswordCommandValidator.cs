using FluentValidation;

namespace IBS.Identity.Application.Commands.ForgotPassword;

/// <summary>
/// Validator for ForgotPasswordCommand.
/// </summary>
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordCommandValidator"/> class.
    /// </summary>
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant is required.");
    }
}
