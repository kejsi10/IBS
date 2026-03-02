using FluentValidation;

namespace IBS.Claims.Application.Commands.CreateClaim;

/// <summary>
/// Validator for CreateClaimCommand.
/// </summary>
public sealed class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public CreateClaimCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.PolicyId)
            .NotEmpty()
            .WithMessage("Policy ID is required.");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.LossDate)
            .NotEmpty()
            .WithMessage("Loss date is required.");

        RuleFor(x => x.ReportedDate)
            .NotEmpty()
            .WithMessage("Reported date is required.")
            .GreaterThanOrEqualTo(x => x.LossDate)
            .WithMessage("Reported date must be on or after loss date.");

        RuleFor(x => x.LossType)
            .IsInEnum()
            .WithMessage("Invalid loss type.");

        RuleFor(x => x.LossDescription)
            .NotEmpty()
            .WithMessage("Loss description is required.")
            .MaximumLength(4000)
            .WithMessage("Loss description must not exceed 4000 characters.");

        RuleFor(x => x.EstimatedLossAmount)
            .GreaterThan(0)
            .When(x => x.EstimatedLossAmount.HasValue)
            .WithMessage("Estimated loss amount must be positive.");
    }
}
