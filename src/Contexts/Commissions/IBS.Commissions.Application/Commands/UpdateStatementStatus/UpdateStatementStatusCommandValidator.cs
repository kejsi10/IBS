using FluentValidation;

namespace IBS.Commissions.Application.Commands.UpdateStatementStatus;

/// <summary>
/// Validator for UpdateStatementStatusCommand.
/// </summary>
public sealed class UpdateStatementStatusCommandValidator : AbstractValidator<UpdateStatementStatusCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public UpdateStatementStatusCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.StatementId)
            .NotEmpty()
            .WithMessage("Statement ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid statement status.");
    }
}
