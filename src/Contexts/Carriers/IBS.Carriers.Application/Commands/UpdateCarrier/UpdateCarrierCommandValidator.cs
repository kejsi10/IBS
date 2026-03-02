using FluentValidation;

namespace IBS.Carriers.Application.Commands.UpdateCarrier;

/// <summary>
/// Validator for UpdateCarrierCommand.
/// </summary>
public sealed class UpdateCarrierCommandValidator : AbstractValidator<UpdateCarrierCommand>
{
    /// <summary>
    /// Valid A.M. Best ratings.
    /// </summary>
    private static readonly string[] ValidAmBestRatings =
    [
        "A++", "A+", "A", "A-",
        "B++", "B+", "B", "B-",
        "C++", "C+", "C", "C-",
        "D", "E", "F", "S", "NR"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCarrierCommandValidator"/> class.
    /// </summary>
    public UpdateCarrierCommandValidator()
    {
        RuleFor(x => x.CarrierId)
            .NotEmpty().WithMessage("Carrier ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Carrier name is required.")
            .MaximumLength(255).WithMessage("Carrier name cannot exceed 255 characters.");

        RuleFor(x => x.LegalName)
            .MaximumLength(500).WithMessage("Legal name cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.LegalName));

        RuleFor(x => x.AmBestRating)
            .Must(rating => string.IsNullOrEmpty(rating) || ValidAmBestRatings.Contains(rating.ToUpperInvariant()))
            .WithMessage($"Invalid A.M. Best rating. Valid values are: {string.Join(", ", ValidAmBestRatings)}")
            .When(x => !string.IsNullOrEmpty(x.AmBestRating));

        RuleFor(x => x.NaicCode)
            .Length(5).WithMessage("NAIC code must be exactly 5 digits.")
            .Matches("^[0-9]+$").WithMessage("NAIC code must contain only digits.")
            .When(x => !string.IsNullOrEmpty(x.NaicCode));

        RuleFor(x => x.WebsiteUrl)
            .Must(BeAValidUrl).WithMessage("Website URL must be a valid HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl));

        RuleFor(x => x.ApiEndpoint)
            .Must(BeAValidUrl).WithMessage("API endpoint must be a valid HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrEmpty(x.ApiEndpoint));

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
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
