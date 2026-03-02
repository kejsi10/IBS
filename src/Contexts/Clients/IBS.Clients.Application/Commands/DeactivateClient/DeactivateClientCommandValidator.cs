using FluentValidation;

namespace IBS.Clients.Application.Commands.DeactivateClient;

/// <summary>
/// Validator for DeactivateClientCommand.
/// </summary>
public sealed class DeactivateClientCommandValidator : AbstractValidator<DeactivateClientCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeactivateClientCommandValidator"/> class.
    /// </summary>
    public DeactivateClientCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required.");
    }
}
