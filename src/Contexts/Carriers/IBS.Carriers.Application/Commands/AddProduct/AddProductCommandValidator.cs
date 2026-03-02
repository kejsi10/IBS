using FluentValidation;

namespace IBS.Carriers.Application.Commands.AddProduct;

/// <summary>
/// Validator for AddProductCommand.
/// </summary>
public sealed class AddProductCommandValidator : AbstractValidator<AddProductCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddProductCommandValidator"/> class.
    /// </summary>
    public AddProductCommandValidator()
    {
        RuleFor(x => x.CarrierId)
            .NotEmpty().WithMessage("Carrier ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(255).WithMessage("Product name cannot exceed 255 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Product code is required.")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters.")
            .Matches("^[A-Za-z0-9-]+$").WithMessage("Product code must contain only letters, numbers, and hyphens.");

        RuleFor(x => x.LineOfBusiness)
            .IsInEnum().WithMessage("Invalid line of business.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.MinimumPremium)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum premium cannot be negative.")
            .When(x => x.MinimumPremium.HasValue);

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(x => x.EffectiveDate)
            .WithMessage("Expiration date must be after effective date.")
            .When(x => x.EffectiveDate.HasValue && x.ExpirationDate.HasValue);
    }
}
