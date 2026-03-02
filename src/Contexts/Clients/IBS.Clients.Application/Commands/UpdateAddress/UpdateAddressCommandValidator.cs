using FluentValidation;

namespace IBS.Clients.Application.Commands.UpdateAddress;

/// <summary>
/// Validator for UpdateAddressCommand.
/// </summary>
public sealed class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    /// <summary>
    /// Initializes a new instance of the UpdateAddressCommandValidator class.
    /// </summary>
    public UpdateAddressCommandValidator()
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
