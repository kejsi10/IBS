using FluentValidation;

namespace IBS.Tenants.Application.Commands.RemoveTenantCarrier;

/// <summary>
/// Validator for RemoveTenantCarrierCommand.
/// </summary>
public sealed class RemoveTenantCarrierCommandValidator : AbstractValidator<RemoveTenantCarrierCommand>
{
    /// <summary>
    /// Initializes a new instance of the RemoveTenantCarrierCommandValidator class.
    /// </summary>
    public RemoveTenantCarrierCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.CarrierId)
            .NotEmpty().WithMessage("Carrier ID is required.");
    }
}
