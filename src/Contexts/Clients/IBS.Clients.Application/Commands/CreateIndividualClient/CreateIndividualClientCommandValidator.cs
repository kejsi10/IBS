using FluentValidation;

namespace IBS.Clients.Application.Commands.CreateIndividualClient;

/// <summary>
/// Validator for CreateIndividualClientCommand.
/// </summary>
public sealed class CreateIndividualClientCommandValidator : AbstractValidator<CreateIndividualClientCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateIndividualClientCommandValidator"/> class.
    /// </summary>
    public CreateIndividualClientCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(100).WithMessage("Middle name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.Suffix)
            .MaximumLength(20).WithMessage("Suffix cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Suffix));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Date of birth must be in the past.")
            .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-150))).WithMessage("Date of birth is too far in the past.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email address is not valid.")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .Matches(@"^[\d\s\-\(\)\+\.]+$").WithMessage("Phone number contains invalid characters.")
            .MinimumLength(10).WithMessage("Phone number must be at least 10 digits.")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}
