using FluentValidation;

namespace IBS.Clients.Application.Commands.SetPrimaryAddress;

/// <summary>
/// Validator for SetPrimaryAddressCommand.
/// </summary>
public sealed class SetPrimaryAddressCommandValidator : AbstractValidator<SetPrimaryAddressCommand>
{
    /// <summary>
    /// Initializes a new instance of the SetPrimaryAddressCommandValidator class.
    /// </summary>
    public SetPrimaryAddressCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.AddressId)
            .NotEmpty()
            .WithMessage("Address ID is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");
    }
}
