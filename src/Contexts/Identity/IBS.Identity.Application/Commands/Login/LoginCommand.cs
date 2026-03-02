using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.Login;

/// <summary>
/// Command to authenticate a user.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="IpAddress">The client's IP address (optional).</param>
/// <param name="UserAgent">The client's user agent (optional).</param>
public sealed record LoginCommand(
    string Email,
    string Password,
    Guid TenantId,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<LoginResult>;

/// <summary>
/// Result of a successful login.
/// </summary>
/// <param name="AccessToken">The JWT access token.</param>
/// <param name="RefreshToken">The refresh token.</param>
/// <param name="ExpiresIn">The access token expiration in seconds.</param>
/// <param name="UserId">The user identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="Email">The user's email.</param>
/// <param name="FullName">The user's full name.</param>
/// <param name="Roles">The user's roles.</param>
public sealed record LoginResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    Guid UserId,
    Guid TenantId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles);
