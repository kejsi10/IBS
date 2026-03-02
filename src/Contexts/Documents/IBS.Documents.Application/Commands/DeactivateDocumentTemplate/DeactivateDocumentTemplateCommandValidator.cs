using FluentValidation;

namespace IBS.Documents.Application.Commands.DeactivateDocumentTemplate;

/// <summary>
/// Validator for DeactivateDocumentTemplateCommand.
/// </summary>
public sealed class DeactivateDocumentTemplateCommandValidator : AbstractValidator<DeactivateDocumentTemplateCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public DeactivateDocumentTemplateCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Template ID is required.");
    }
}
