using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.ForgotPassword;

/// <summary>
/// Command to initiate a password reset.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record ForgotPasswordCommand(
    string Email,
    Guid TenantId) : ICommand;
