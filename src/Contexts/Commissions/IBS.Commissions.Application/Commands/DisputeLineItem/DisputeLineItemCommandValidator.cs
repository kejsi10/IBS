using FluentValidation;

namespace IBS.Commissions.Application.Commands.DisputeLineItem;

/// <summary>
/// Validator for DisputeLineItemCommand.
/// </summary>
public sealed class DisputeLineItemCommandValidator : AbstractValidator<DisputeLineItemCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public DisputeLineItemCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.StatementId)
            .NotEmpty()
            .WithMessage("Statement ID is required.");

        RuleFor(x => x.LineItemId)
            .NotEmpty()
            .WithMessage("Line item ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Dispute reason is required.")
            .MaximumLength(2000)
            .WithMessage("Dispute reason must not exceed 2000 characters.");
    }
}
