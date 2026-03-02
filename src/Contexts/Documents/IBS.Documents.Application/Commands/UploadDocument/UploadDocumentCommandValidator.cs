using FluentValidation;

namespace IBS.Documents.Application.Commands.UploadDocument;

/// <summary>
/// Validator for UploadDocumentCommand.
/// </summary>
public sealed class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(260).WithMessage("File name is required and must not exceed 260 characters.");
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(100).WithMessage("Content type is required.");
        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File size must be greater than zero.")
            .LessThanOrEqualTo(52_428_800).WithMessage("File size must not exceed 50 MB.");
        RuleFor(x => x.FileContent).NotNull().WithMessage("File content is required.");
        RuleFor(x => x.EntityType).IsInEnum().WithMessage("Invalid entity type.");
        RuleFor(x => x.Category).IsInEnum().WithMessage("Invalid document category.");
    }
}
