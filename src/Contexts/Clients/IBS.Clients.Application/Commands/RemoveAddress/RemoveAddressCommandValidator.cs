using FluentValidation;

namespace IBS.Clients.Application.Commands.RemoveAddress;

/// <summary>
/// Validator for RemoveAddressCommand.
/// </summary>
public sealed class RemoveAddressCommandValidator : AbstractValidator<RemoveAddressCommand>
{
    /// <summary>
    /// Initializes a new instance of the RemoveAddressCommandValidator class.
    /// </summary>
    public RemoveAddressCommandValidator()
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
