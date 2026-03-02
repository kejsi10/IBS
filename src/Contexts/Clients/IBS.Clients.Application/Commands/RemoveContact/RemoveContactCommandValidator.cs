using FluentValidation;

namespace IBS.Clients.Application.Commands.RemoveContact;

/// <summary>
/// Validator for RemoveContactCommand.
/// </summary>
public sealed class RemoveContactCommandValidator : AbstractValidator<RemoveContactCommand>
{
    /// <summary>
    /// Initializes a new instance of the RemoveContactCommandValidator class.
    /// </summary>
    public RemoveContactCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.ContactId)
            .NotEmpty()
            .WithMessage("Contact ID is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");
    }
}
