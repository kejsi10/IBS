using FluentValidation;

namespace IBS.Tenants.Application.Commands.AddTenantCarrier;

/// <summary>
/// Validator for AddTenantCarrierCommand.
/// </summary>
public sealed class AddTenantCarrierCommandValidator : AbstractValidator<AddTenantCarrierCommand>
{
    /// <summary>
    /// Initializes a new instance of the AddTenantCarrierCommandValidator class.
    /// </summary>
    public AddTenantCarrierCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.CarrierId)
            .NotEmpty().WithMessage("Carrier ID is required.");

        RuleFor(x => x.AgencyCode)
            .MaximumLength(50).WithMessage("Agency code cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.AgencyCode));

        RuleFor(x => x.CommissionRate)
            .InclusiveBetween(0, 1).WithMessage("Commission rate must be between 0 and 1 (0% to 100%).")
            .When(x => x.CommissionRate.HasValue);
    }
}
