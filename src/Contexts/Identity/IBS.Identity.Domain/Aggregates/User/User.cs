using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Events;
using IBS.Identity.Domain.ValueObjects;

namespace IBS.Identity.Domain.Aggregates.User;

/// <summary>
/// Represents a user in the system.
/// </summary>
public sealed class User : TenantAggregateRoot
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Gets the user's password hash.
    /// </summary>
    public PasswordHash PasswordHash { get; private set; } = null!;

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's title/position.
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// Gets the user's phone number.
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Gets the user's status.
    /// </summary>
    public UserStatus Status { get; private set; }

    /// <summary>
    /// Gets the date and time of the last login.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; private set; }

    /// <summary>
    /// Gets the number of consecutive failed login attempts.
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// Gets the date and time when the account was locked.
    /// </summary>
    public DateTimeOffset? LockedUntil { get; private set; }

    /// <summary>
    /// Gets the password reset token.
    /// </summary>
    public string? PasswordResetToken { get; private set; }

    /// <summary>
    /// Gets the password reset token expiry date.
    /// </summary>
    public DateTimeOffset? PasswordResetTokenExpiry { get; private set; }

    /// <summary>
    /// Gets the email confirmation token.
    /// </summary>
    public string? EmailConfirmationToken { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the email has been confirmed.
    /// </summary>
    public bool EmailConfirmed { get; private set; }

    /// <summary>
    /// Gets the roles assigned to this user.
    /// </summary>
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    /// <summary>
    /// Gets the refresh tokens for this user.
    /// </summary>
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private User() { }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="email">The email address.</param>
    /// <param name="passwordHash">The password hash.</param>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="title">The title (optional).</param>
    /// <param name="phoneNumber">The phone number (optional).</param>
    /// <returns>A new User instance.</returns>
    public static User Create(
        Guid tenantId,
        Email email,
        PasswordHash passwordHash,
        string firstName,
        string lastName,
        string? title = null,
        string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        var user = new User
        {
            TenantId = tenantId,
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Title = title?.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            Status = UserStatus.Pending,
            EmailConfirmationToken = Guid.NewGuid().ToString("N")
        };

        user.RaiseDomainEvent(new UserRegisteredEvent(
            user.Id,
            user.TenantId,
            user.Email.Value));

        return user;
    }

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="title">The title (optional).</param>
    /// <param name="phoneNumber">The phone number (optional).</param>
    public void UpdateProfile(string firstName, string lastName, string? title, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Title = title?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    /// <param name="newPasswordHash">The new password hash.</param>
    public void ChangePassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the user account.
    /// </summary>
    public void Activate()
    {
        if (Status == UserStatus.Active)
            return;

        Status = UserStatus.Active;
        EmailConfirmed = true;
        EmailConfirmationToken = null;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        MarkAsUpdated();

        RaiseDomainEvent(new UserActivatedEvent(Id, TenantId));
    }

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    public void Deactivate()
    {
        if (Status == UserStatus.Inactive)
            return;

        Status = UserStatus.Inactive;
        MarkAsUpdated();

        RaiseDomainEvent(new UserDeactivatedEvent(Id, TenantId));
    }

    /// <summary>
    /// Locks the user account.
    /// </summary>
    /// <param name="lockDuration">The duration of the lock.</param>
    public void Lock(TimeSpan lockDuration)
    {
        Status = UserStatus.Locked;
        LockedUntil = DateTimeOffset.UtcNow.Add(lockDuration);
        MarkAsUpdated();
    }

    /// <summary>
    /// Unlocks the user account.
    /// </summary>
    public void Unlock()
    {
        if (Status != UserStatus.Locked)
            return;

        Status = UserStatus.Active;
        LockedUntil = null;
        FailedLoginAttempts = 0;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records a successful login.
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
        FailedLoginAttempts = 0;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records a failed login attempt.
    /// </summary>
    /// <param name="maxAttempts">Maximum allowed failed attempts before locking.</param>
    /// <param name="lockDuration">Duration to lock the account.</param>
    public void RecordFailedLogin(int maxAttempts = 5, TimeSpan? lockDuration = null)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            Lock(lockDuration ?? TimeSpan.FromMinutes(30));
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Generates a password reset token.
    /// </summary>
    /// <param name="expiry">Token expiry duration.</param>
    /// <returns>The generated reset token.</returns>
    public string GeneratePasswordResetToken(TimeSpan? expiry = null)
    {
        PasswordResetToken = Guid.NewGuid().ToString("N");
        PasswordResetTokenExpiry = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromHours(24));
        MarkAsUpdated();
        return PasswordResetToken;
    }

    /// <summary>
    /// Validates a password reset token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    public bool ValidatePasswordResetToken(string token)
    {
        return !string.IsNullOrEmpty(PasswordResetToken) &&
               PasswordResetToken == token &&
               PasswordResetTokenExpiry.HasValue &&
               PasswordResetTokenExpiry.Value > DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Assigns a role to this user.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    public void AssignRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            return;

        var userRole = UserRole.Create(Id, roleId);
        _userRoles.Add(userRole);
        MarkAsUpdated();

        RaiseDomainEvent(new RoleAssignedEvent(Id, TenantId, roleId));
    }

    /// <summary>
    /// Removes a role from this user.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole == null)
            return;

        _userRoles.Remove(userRole);
        MarkAsUpdated();
    }

    /// <summary>
    /// Adds a refresh token.
    /// </summary>
    /// <param name="token">The token value.</param>
    /// <param name="expiresAt">The expiry date.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <param name="userAgent">The user agent of the client.</param>
    /// <returns>The created refresh token.</returns>
    public RefreshToken AddRefreshToken(string token, DateTimeOffset expiresAt, string? ipAddress, string? userAgent)
    {
        var refreshToken = RefreshToken.Create(Id, token, expiresAt, ipAddress, userAgent);
        _refreshTokens.Add(refreshToken);
        MarkAsUpdated();
        return refreshToken;
    }

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="token">The token to revoke.</param>
    /// <param name="replacedByToken">The replacement token (if any).</param>
    public void RevokeRefreshToken(string token, string? replacedByToken = null)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
        refreshToken?.Revoke(replacedByToken);
        MarkAsUpdated();
    }

    /// <summary>
    /// Revokes all refresh tokens for this user.
    /// </summary>
    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke();
        }
        MarkAsUpdated();
    }
}
