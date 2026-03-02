using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Services;
using IBS.Identity.Domain.Repositories;
using IBS.Identity.Domain.ValueObjects;

namespace IBS.Identity.Application.Commands.ResetPassword;

/// <summary>
/// Handler for the ResetPasswordCommand.
/// </summary>
public sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<ResetPasswordCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await userRepository.GetByEmailAsync(request.TenantId, request.Email, cancellationToken);
        if (user is null)
        {
            return Error.Validation("Invalid reset token.");
        }

        // Validate reset token
        if (!user.ValidatePasswordResetToken(request.Token))
        {
            return Error.Validation("Invalid or expired reset token.");
        }

        // Hash new password and update
        var newPasswordHash = PasswordHash.FromHash(passwordHasher.HashPassword(request.NewPassword));
        user.ChangePassword(newPasswordHash);

        // Revoke all refresh tokens for security
        user.RevokeAllRefreshTokens();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
