using FluentValidation;

namespace IBS.Tenants.Application.Commands.CancelTenant;

/// <summary>
/// Validator for CancelTenantCommand.
/// </summary>
public sealed class CancelTenantCommandValidator : AbstractValidator<CancelTenantCommand>
{
    /// <summary>
    /// Initializes a new instance of the CancelTenantCommandValidator class.
    /// </summary>
    public CancelTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");
    }
}
