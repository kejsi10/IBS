using FluentValidation;

namespace IBS.Claims.Application.Commands.UpdateClaimStatus;

/// <summary>
/// Validator for UpdateClaimStatusCommand.
/// </summary>
public sealed class UpdateClaimStatusCommandValidator : AbstractValidator<UpdateClaimStatusCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public UpdateClaimStatusCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithMessage("Claim ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid claim status.");
    }
}
