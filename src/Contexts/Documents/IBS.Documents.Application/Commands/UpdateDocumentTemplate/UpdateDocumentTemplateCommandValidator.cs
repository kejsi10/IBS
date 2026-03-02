using FluentValidation;

namespace IBS.Documents.Application.Commands.UpdateDocumentTemplate;

/// <summary>
/// Validator for UpdateDocumentTemplateCommand.
/// </summary>
public sealed class UpdateDocumentTemplateCommandValidator : AbstractValidator<UpdateDocumentTemplateCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public UpdateDocumentTemplateCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Template ID is required.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Name is required and must not exceed 200 characters.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Template content is required.");
    }
}
