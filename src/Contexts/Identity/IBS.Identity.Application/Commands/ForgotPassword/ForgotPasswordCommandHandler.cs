using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Repositories;

namespace IBS.Identity.Application.Commands.ForgotPassword;

/// <summary>
/// Handler for the ForgotPasswordCommand.
/// </summary>
public sealed class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IEmailService emailService) : ICommandHandler<ForgotPasswordCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Get user by email - always return success to prevent email enumeration
        var user = await userRepository.GetByEmailAsync(request.TenantId, request.Email, cancellationToken);

        if (user is not null)
        {
            // Generate reset token
            var resetToken = user.GeneratePasswordResetToken();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Send password reset email
            await emailService.SendEmailAsync(new EmailMessage(
                To: user.Email.Value,
                Subject: "IBS - Password Reset Request",
                HtmlBody: $"""
                    <h2>Password Reset</h2>
                    <p>You requested a password reset for your IBS account.</p>
                    <p>Your reset token is: <strong>{resetToken}</strong></p>
                    <p>This token expires in 1 hour.</p>
                    <p>If you did not request this reset, please ignore this email.</p>
                    """), cancellationToken);
        }

        // Always return success to prevent email enumeration
        return Result.Success();
    }
}
