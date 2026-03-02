using FluentValidation;

namespace IBS.PolicyAssistant.Application.Commands.SendMessage;

/// <summary>
/// Validator for the <see cref="SendMessageCommand"/>.
/// </summary>
public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SendMessageCommandValidator"/> class.
    /// </summary>
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage("Conversation ID is required.");
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required.")
            .MaximumLength(4000).WithMessage("Message content must not exceed 4000 characters.");
    }
}
