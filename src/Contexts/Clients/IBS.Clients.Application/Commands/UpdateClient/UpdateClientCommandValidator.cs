using FluentValidation;

namespace IBS.Clients.Application.Commands.UpdateClient;

/// <summary>
/// Validator for UpdateClientCommand.
/// </summary>
public sealed class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    /// <summary>
    /// Valid business types.
    /// </summary>
    private static readonly string[] ValidBusinessTypes =
    [
        "LLC", "Corporation", "S-Corp", "C-Corp", "Partnership",
        "Sole Proprietorship", "Non-Profit", "Government", "Other"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateClientCommandValidator"/> class.
    /// </summary>
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.MiddleName)
            .MaximumLength(100).WithMessage("Middle name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.Suffix)
            .MaximumLength(20).WithMessage("Suffix cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Suffix));

        RuleFor(x => x.BusinessName)
            .MaximumLength(255).WithMessage("Business name cannot exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.BusinessName));

        RuleFor(x => x.BusinessType)
            .Must(t => string.IsNullOrEmpty(t) || ValidBusinessTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Business type must be one of: {string.Join(", ", ValidBusinessTypes)}")
            .When(x => !string.IsNullOrEmpty(x.BusinessType));

        RuleFor(x => x.DbaName)
            .MaximumLength(255).WithMessage("DBA name cannot exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.DbaName));

        RuleFor(x => x.Industry)
            .MaximumLength(100).WithMessage("Industry cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Industry));

        RuleFor(x => x.YearEstablished)
            .InclusiveBetween(1800, DateTime.UtcNow.Year)
            .WithMessage($"Year established must be between 1800 and {DateTime.UtcNow.Year}.")
            .When(x => x.YearEstablished.HasValue);

        RuleFor(x => x.NumberOfEmployees)
            .GreaterThanOrEqualTo(0).WithMessage("Number of employees cannot be negative.")
            .When(x => x.NumberOfEmployees.HasValue);

        RuleFor(x => x.AnnualRevenue)
            .GreaterThanOrEqualTo(0).WithMessage("Annual revenue cannot be negative.")
            .When(x => x.AnnualRevenue.HasValue);

        RuleFor(x => x.Website)
            .Must(BeAValidUrl).WithMessage("Website must be a valid HTTP or HTTPS URL.")
            .MaximumLength(500).WithMessage("Website cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Website));

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

    /// <summary>
    /// Validates that the string is a valid HTTP/HTTPS URL.
    /// </summary>
    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
