using FluentValidation;

namespace IBS.Documents.Application.Commands.DeleteDocument;

/// <summary>
/// Validator for DeleteDocumentCommand.
/// </summary>
public sealed class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.DocumentId).NotEmpty().WithMessage("Document ID is required.");
    }
}
