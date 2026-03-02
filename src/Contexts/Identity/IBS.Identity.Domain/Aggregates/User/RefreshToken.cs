using IBS.BuildingBlocks.Domain;

namespace IBS.Identity.Domain.Aggregates.User;

/// <summary>
/// Represents a refresh token for JWT authentication.
/// </summary>
public sealed class RefreshToken : Entity
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the token value.
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date and time when this token expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the date and time when this token was revoked.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// Gets the token that replaced this one (if any).
    /// </summary>
    public string? ReplacedByToken { get; private set; }

    /// <summary>
    /// Gets the IP address from which this token was created.
    /// </summary>
    public string? CreatedFromIp { get; private set; }

    /// <summary>
    /// Gets the user agent from which this token was created.
    /// </summary>
    public string? CreatedFromUserAgent { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this token has expired.
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    /// <summary>
    /// Gets a value indicating whether this token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Gets a value indicating whether this token is active (not expired and not revoked).
    /// </summary>
    public bool IsActive => !IsExpired && !IsRevoked;

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private RefreshToken() { }

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="token">The token value.</param>
    /// <param name="expiresAt">The expiry date.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <param name="userAgent">The user agent of the client.</param>
    /// <returns>A new RefreshToken instance.</returns>
    internal static RefreshToken Create(
        Guid userId,
        string token,
        DateTimeOffset expiresAt,
        string? ipAddress,
        string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required.", nameof(token));

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedFromIp = ipAddress,
            CreatedFromUserAgent = userAgent
        };
    }

    /// <summary>
    /// Revokes this token.
    /// </summary>
    /// <param name="replacedByToken">The token that replaces this one (if any).</param>
    internal void Revoke(string? replacedByToken = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
