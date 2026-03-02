using FluentValidation;

namespace IBS.Claims.Application.Commands.AddClaimNote;

/// <summary>
/// Validator for AddClaimNoteCommand.
/// </summary>
public sealed class AddClaimNoteCommandValidator : AbstractValidator<AddClaimNoteCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public AddClaimNoteCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithMessage("Claim ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Note content is required.")
            .MaximumLength(4000)
            .WithMessage("Note content must not exceed 4000 characters.");
    }
}
