using FluentValidation;

namespace IBS.Documents.Application.Commands.ActivateDocumentTemplate;

/// <summary>
/// Validator for ActivateDocumentTemplateCommand.
/// </summary>
public sealed class ActivateDocumentTemplateCommandValidator : AbstractValidator<ActivateDocumentTemplateCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public ActivateDocumentTemplateCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Template ID is required.");
    }
}
