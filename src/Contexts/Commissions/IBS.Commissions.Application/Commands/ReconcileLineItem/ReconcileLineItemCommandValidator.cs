using FluentValidation;

namespace IBS.Commissions.Application.Commands.ReconcileLineItem;

/// <summary>
/// Validator for ReconcileLineItemCommand.
/// </summary>
public sealed class ReconcileLineItemCommandValidator : AbstractValidator<ReconcileLineItemCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public ReconcileLineItemCommandValidator()
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
    }
}
