using IBS.BuildingBlocks.Application.Commands;
using IBS.Identity.Application.Commands.Login;

namespace IBS.Identity.Application.Commands.RefreshToken;

/// <summary>
/// Command to refresh an access token.
/// </summary>
/// <param name="RefreshToken">The refresh token.</param>
/// <param name="IpAddress">The client's IP address (optional).</param>
/// <param name="UserAgent">The client's user agent (optional).</param>
public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<LoginResult>;
