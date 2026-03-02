using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.ResetPassword;

/// <summary>
/// Command to reset a user's password.
/// </summary>
/// <param name="Token">The password reset token.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="NewPassword">The new password.</param>
public sealed record ResetPasswordCommand(
    string Token,
    string Email,
    Guid TenantId,
    string NewPassword) : ICommand;
