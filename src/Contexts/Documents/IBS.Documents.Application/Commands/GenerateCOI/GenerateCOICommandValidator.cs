using FluentValidation;

namespace IBS.Documents.Application.Commands.GenerateCOI;

/// <summary>
/// Validator for GenerateCOICommand.
/// </summary>
public sealed class GenerateCOICommandValidator : AbstractValidator<GenerateCOICommand>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public GenerateCOICommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Template ID is required.");
        RuleFor(x => x.PolicyId).NotEmpty().WithMessage("Policy ID is required.");
    }
}
