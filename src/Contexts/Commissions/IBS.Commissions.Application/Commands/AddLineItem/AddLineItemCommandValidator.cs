using FluentValidation;

namespace IBS.Commissions.Application.Commands.AddLineItem;

/// <summary>
/// Validator for AddLineItemCommand.
/// </summary>
public sealed class AddLineItemCommandValidator : AbstractValidator<AddLineItemCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public AddLineItemCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.StatementId)
            .NotEmpty()
            .WithMessage("Statement ID is required.");

        RuleFor(x => x.PolicyNumber)
            .NotEmpty()
            .WithMessage("Policy number is required.")
            .MaximumLength(50)
            .WithMessage("Policy number must not exceed 50 characters.");

        RuleFor(x => x.InsuredName)
            .NotEmpty()
            .WithMessage("Insured name is required.")
            .MaximumLength(200)
            .WithMessage("Insured name must not exceed 200 characters.");

        RuleFor(x => x.LineOfBusiness)
            .NotEmpty()
            .WithMessage("Line of business is required.")
            .MaximumLength(100)
            .WithMessage("Line of business must not exceed 100 characters.");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty()
            .WithMessage("Effective date is required.");

        RuleFor(x => x.TransactionType)
            .IsInEnum()
            .WithMessage("Invalid transaction type.");

        RuleFor(x => x.CommissionRate)
            .InclusiveBetween(0, 100)
            .WithMessage("Commission rate must be between 0 and 100.");
    }
}
