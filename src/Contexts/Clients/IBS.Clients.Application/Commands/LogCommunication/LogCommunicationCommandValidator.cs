using FluentValidation;

namespace IBS.Clients.Application.Commands.LogCommunication;

/// <summary>
/// Validator for LogCommunicationCommand.
/// </summary>
public sealed class LogCommunicationCommandValidator : AbstractValidator<LogCommunicationCommand>
{
    /// <summary>
    /// Initializes a new instance of the LogCommunicationCommandValidator class.
    /// </summary>
    public LogCommunicationCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.CommunicationType)
            .IsInEnum()
            .WithMessage("Invalid communication type.");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Subject is required.")
            .MaximumLength(255)
            .WithMessage("Subject cannot exceed 255 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .WithMessage("Notes cannot exceed 4000 characters.");
    }
}
