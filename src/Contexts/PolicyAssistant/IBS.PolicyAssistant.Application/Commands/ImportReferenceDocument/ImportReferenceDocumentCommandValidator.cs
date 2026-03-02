using FluentValidation;

namespace IBS.PolicyAssistant.Application.Commands.ImportReferenceDocument;

/// <summary>
/// Validator for the <see cref="ImportReferenceDocumentCommand"/>.
/// </summary>
public sealed class ImportReferenceDocumentCommandValidator : AbstractValidator<ImportReferenceDocumentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportReferenceDocumentCommandValidator"/> class.
    /// </summary>
    public ImportReferenceDocumentCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x.State)
            .MaximumLength(2).WithMessage("State must be a 2-letter code.")
            .When(x => x.State is not null);
    }
}
