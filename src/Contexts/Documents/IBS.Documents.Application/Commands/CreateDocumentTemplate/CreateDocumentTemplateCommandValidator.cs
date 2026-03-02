using FluentValidation;

namespace IBS.Documents.Application.Commands.CreateDocumentTemplate;

/// <summary>
/// Validator for CreateDocumentTemplateCommand.
/// </summary>
public sealed class CreateDocumentTemplateCommandValidator : AbstractValidator<CreateDocumentTemplateCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public CreateDocumentTemplateCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Name is required and must not exceed 200 characters.");
        RuleFor(x => x.Description).MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
        RuleFor(x => x.TemplateType).IsInEnum().WithMessage("Invalid template type.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Template content is required.");
    }
}
