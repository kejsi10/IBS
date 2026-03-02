using FluentValidation;

namespace IBS.Tenants.Application.Commands.SuspendTenant;

/// <summary>
/// Validator for SuspendTenantCommand.
/// </summary>
public sealed class SuspendTenantCommandValidator : AbstractValidator<SuspendTenantCommand>
{
    /// <summary>
    /// Initializes a new instance of the SuspendTenantCommandValidator class.
    /// </summary>
    public SuspendTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");
    }
}
