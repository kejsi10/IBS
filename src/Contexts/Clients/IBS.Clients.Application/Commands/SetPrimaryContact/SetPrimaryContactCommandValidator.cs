using FluentValidation;

namespace IBS.Clients.Application.Commands.SetPrimaryContact;

/// <summary>
/// Validator for SetPrimaryContactCommand.
/// </summary>
public sealed class SetPrimaryContactCommandValidator : AbstractValidator<SetPrimaryContactCommand>
{
    /// <summary>
    /// Initializes a new instance of the SetPrimaryContactCommandValidator class.
    /// </summary>
    public SetPrimaryContactCommandValidator()
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
