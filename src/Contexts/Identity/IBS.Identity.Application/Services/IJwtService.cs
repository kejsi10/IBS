namespace IBS.Identity.Application.Services;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The generated access token and its expiration.</returns>
    (string Token, int ExpiresInSeconds) GenerateAccessToken(
        Guid userId,
        Guid tenantId,
        string email,
        IEnumerable<string> roles);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <returns>The generated refresh token.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Gets the refresh token expiration time.
    /// </summary>
    /// <returns>The refresh token expiration as DateTimeOffset.</returns>
    DateTimeOffset GetRefreshTokenExpiration();
}
