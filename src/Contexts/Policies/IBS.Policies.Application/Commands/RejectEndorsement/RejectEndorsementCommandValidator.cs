using FluentValidation;

namespace IBS.Policies.Application.Commands.RejectEndorsement;

/// <summary>
/// Validator for the RejectEndorsementCommand.
/// </summary>
public sealed class RejectEndorsementCommandValidator : AbstractValidator<RejectEndorsementCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RejectEndorsementCommandValidator"/> class.
    /// </summary>
    public RejectEndorsementCommandValidator()
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

        RuleFor(x => x.EndorsementId)
            .NotEmpty()
            .WithMessage("Endorsement ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Rejection reason is required.")
            .MaximumLength(500)
            .WithMessage("Rejection reason must not exceed 500 characters.");
    }
}
