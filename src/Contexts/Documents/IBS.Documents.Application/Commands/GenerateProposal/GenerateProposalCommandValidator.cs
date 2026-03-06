using FluentValidation;

namespace IBS.Documents.Application.Commands.GenerateProposal;

/// <summary>
/// Validator for the GenerateProposalCommand.
/// </summary>
public sealed class GenerateProposalCommandValidator : AbstractValidator<GenerateProposalCommand>
{
    /// <summary>
    /// Initializes a new instance of the GenerateProposalCommandValidator class.
    /// </summary>
    public GenerateProposalCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.QuoteId).NotEmpty();
    }
}
