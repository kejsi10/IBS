using FluentValidation;

namespace IBS.Clients.Application.Commands.AddAddress;

/// <summary>
/// Validator for AddAddressCommand.
/// </summary>
public sealed class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    /// <summary>
    /// Initializes a new instance of the AddAddressCommandValidator class.
    /// </summary>
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.AddressType)
            .IsInEnum()
            .WithMessage("Invalid address type.");

        RuleFor(x => x.StreetLine1)
            .NotEmpty()
            .WithMessage("Street address is required.")
            .MaximumLength(255)
            .WithMessage("Street address cannot exceed 255 characters.");

        RuleFor(x => x.StreetLine2)
            .MaximumLength(255)
            .WithMessage("Street address line 2 cannot exceed 255 characters.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required.")
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required.")
            .MaximumLength(50)
            .WithMessage("State cannot exceed 50 characters.");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("Postal code is required.")
            .MaximumLength(20)
            .WithMessage("Postal code cannot exceed 20 characters.");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required.")
            .MaximumLength(50)
            .WithMessage("Country cannot exceed 50 characters.");
    }
}
