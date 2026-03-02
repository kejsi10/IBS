using FluentValidation;

namespace IBS.Clients.Application.Commands.AddContact;

/// <summary>
/// Validator for AddContactCommand.
/// </summary>
public sealed class AddContactCommandValidator : AbstractValidator<AddContactCommand>
{
    /// <summary>
    /// Initializes a new instance of the AddContactCommandValidator class.
    /// </summary>
    public AddContactCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MaximumLength(100)
            .WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MaximumLength(100)
            .WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(100)
            .WithMessage("Middle name cannot exceed 100 characters.");

        RuleFor(x => x.Suffix)
            .MaximumLength(20)
            .WithMessage("Suffix cannot exceed 20 characters.");

        RuleFor(x => x.Title)
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone cannot exceed 20 characters.");
    }
}
