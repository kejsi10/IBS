using FluentValidation;

namespace IBS.Commissions.Application.Commands.AddProducerSplit;

/// <summary>
/// Validator for AddProducerSplitCommand.
/// </summary>
public sealed class AddProducerSplitCommandValidator : AbstractValidator<AddProducerSplitCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public AddProducerSplitCommandValidator()
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

        RuleFor(x => x.ProducerName)
            .NotEmpty()
            .WithMessage("Producer name is required.")
            .MaximumLength(200)
            .WithMessage("Producer name must not exceed 200 characters.");

        RuleFor(x => x.ProducerId)
            .NotEmpty()
            .WithMessage("Producer ID is required.");

        RuleFor(x => x.SplitPercentage)
            .GreaterThan(0)
            .WithMessage("Split percentage must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Split percentage must not exceed 100.");

        RuleFor(x => x.SplitAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Split amount must be non-negative.");
    }
}
