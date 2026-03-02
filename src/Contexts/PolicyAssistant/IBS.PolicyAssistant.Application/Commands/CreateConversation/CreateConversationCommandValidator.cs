using FluentValidation;

namespace IBS.PolicyAssistant.Application.Commands.CreateConversation;

/// <summary>
/// Validator for the <see cref="CreateConversationCommand"/>.
/// </summary>
public sealed class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConversationCommandValidator"/> class.
    /// </summary>
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");
    }
}
