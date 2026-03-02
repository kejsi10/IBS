using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Aggregates.User;
using IBS.Identity.Domain.Events;
using IBS.Identity.Domain.ValueObjects;

namespace IBS.UnitTests.Identity.Domain;

/// <summary>
/// Unit tests for the User aggregate root.
/// </summary>
public class UserTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_ValidInputs_CreatesUser()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.FromHash("hashed_password_123");

        // Act
        var user = User.Create(_tenantId, email, passwordHash, "John", "Doe");

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.TenantId.Should().Be(_tenantId);
        user.Status.Should().Be(UserStatus.Pending);
        user.EmailConfirmed.Should().BeFalse();
        user.EmailConfirmationToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Create_WithOptionalFields_SetsAllFields()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.FromHash("hashed_password_123");

        // Act
        var user = User.Create(_tenantId, email, passwordHash, "John", "Doe", "Manager", "555-123-4567");

        // Assert
        user.Title.Should().Be("Manager");
        user.PhoneNumber.Should().Be("555-123-4567");
    }

    [Fact]
    public void Create_RaisesUserRegisteredEvent()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.FromHash("hashed_password_123");

        // Act
        var user = User.Create(_tenantId, email, passwordHash, "John", "Doe");

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.First().Should().BeOfType<UserRegisteredEvent>();
    }

    [Fact]
    public void Create_EmptyFirstName_ThrowsException()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.FromHash("hashed_password_123");

        // Act
        var act = () => User.Create(_tenantId, email, passwordHash, "", "Doe");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*first name*");
    }

    [Fact]
    public void Create_EmptyLastName_ThrowsException()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.FromHash("hashed_password_123");

        // Act
        var act = () => User.Create(_tenantId, email, passwordHash, "John", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*last name*");
    }

    [Fact]
    public void FullName_ReturnsFirstAndLastName()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void UpdateProfile_ValidInputs_UpdatesProfile()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.UpdateProfile("Jane", "Smith", "Director", "555-999-0000");

        // Assert
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.Title.Should().Be("Director");
        user.PhoneNumber.Should().Be("555-999-0000");
    }

    [Fact]
    public void UpdateProfile_EmptyFirstName_ThrowsException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var act = () => user.UpdateProfile("", "Smith", null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*first name*");
    }

    [Fact]
    public void ChangePassword_UpdatesPasswordHash()
    {
        // Arrange
        var user = CreateTestUser();
        var newPasswordHash = PasswordHash.FromHash("new_hashed_password");

        // Act
        user.ChangePassword(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
    }

    [Fact]
    public void ChangePassword_ClearsResetToken()
    {
        // Arrange
        var user = CreateTestUser();
        user.GeneratePasswordResetToken();
        var newPasswordHash = PasswordHash.FromHash("new_hashed_password");

        // Act
        user.ChangePassword(newPasswordHash);

        // Assert
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiry.Should().BeNull();
    }

    [Fact]
    public void Activate_PendingUser_ActivatesUser()
    {
        // Arrange
        var user = CreateTestUser();
        user.ClearDomainEvents();

        // Act
        user.Activate();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
        user.EmailConfirmed.Should().BeTrue();
        user.EmailConfirmationToken.Should().BeNull();
        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.First().Should().BeOfType<UserActivatedEvent>();
    }

    [Fact]
    public void Activate_AlreadyActiveUser_DoesNothing()
    {
        // Arrange
        var user = CreateTestUser();
        user.Activate();
        user.ClearDomainEvents();

        // Act
        user.Activate();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_ActiveUser_DeactivatesUser()
    {
        // Arrange
        var user = CreateTestUser();
        user.Activate();
        user.ClearDomainEvents();

        // Act
        user.Deactivate();

        // Assert
        user.Status.Should().Be(UserStatus.Inactive);
        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.First().Should().BeOfType<UserDeactivatedEvent>();
    }

    [Fact]
    public void Deactivate_InactiveUser_DoesNothing()
    {
        // Arrange
        var user = CreateTestUser();
        user.Activate();
        user.Deactivate();
        user.ClearDomainEvents();

        // Act
        user.Deactivate();

        // Assert
        user.Status.Should().Be(UserStatus.Inactive);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Lock_LocksUserForDuration()
    {
        // Arrange
        var user = CreateTestUser();
        user.Activate();
        var lockDuration = TimeSpan.FromMinutes(30);

        // Act
        user.Lock(lockDuration);

        // Assert
        user.Status.Should().Be(UserStatus.Locked);
        user.LockedUntil.Should().NotBeNull();
        user.LockedUntil.Should().BeCloseTo(DateTimeOffset.UtcNow.Add(lockDuration), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Unlock_LockedUser_UnlocksUser()
    {
        // Arrange
        var user = CreateTestUser();
        user.Activate();
        user.Lock(TimeSpan.FromMinutes(30));

        // Act
        user.Unlock();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
        user.LockedUntil.Should().BeNull();
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void RecordSuccessfulLogin_UpdatesLastLoginAndResetsFailedAttempts()
    {
        // Arrange
        var user = CreateTestUser();
        user.Activate();
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        // Act
        user.RecordSuccessfulLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void RecordFailedLogin_IncrementsFailedAttempts()
    {
        // Arrange
        var user = CreateTestUser();
        user.Activate();

        // Act
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        // Assert
        user.FailedLoginAttempts.Should().Be(2);
    }

    [Fact]
    public void RecordFailedLogin_MaxAttemptsReached_LocksUser()
    {
        // Arrange
        var user = CreateTestUser();
        user.Activate();

        // Act
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin(maxAttempts: 5);
        }

        // Assert
        user.Status.Should().Be(UserStatus.Locked);
        user.LockedUntil.Should().NotBeNull();
    }

    [Fact]
    public void GeneratePasswordResetToken_GeneratesValidToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = user.GeneratePasswordResetToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        user.PasswordResetToken.Should().Be(token);
        user.PasswordResetTokenExpiry.Should().NotBeNull();
        user.PasswordResetTokenExpiry.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void ValidatePasswordResetToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestUser();
        var token = user.GeneratePasswordResetToken();

        // Act
        var isValid = user.ValidatePasswordResetToken(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePasswordResetToken_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var user = CreateTestUser();
        user.GeneratePasswordResetToken();

        // Act
        var isValid = user.ValidatePasswordResetToken("invalid_token");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidatePasswordResetToken_NoTokenGenerated_ReturnsFalse()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var isValid = user.ValidatePasswordResetToken("any_token");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void AssignRole_ValidRole_AddsRole()
    {
        // Arrange
        var user = CreateTestUser();
        user.ClearDomainEvents();
        var roleId = Guid.NewGuid();

        // Act
        user.AssignRole(roleId);

        // Assert
        user.UserRoles.Should().HaveCount(1);
        user.UserRoles.First().RoleId.Should().Be(roleId);
        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.First().Should().BeOfType<RoleAssignedEvent>();
    }

    [Fact]
    public void AssignRole_SameRoleTwice_OnlyAddsOnce()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId);

        // Act
        user.AssignRole(roleId);

        // Assert
        user.UserRoles.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveRole_ExistingRole_RemovesRole()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId);

        // Act
        user.RemoveRole(roleId);

        // Assert
        user.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_NonExistingRole_DoesNothing()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.RemoveRole(Guid.NewGuid());

        // Assert
        user.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void AddRefreshToken_AddsToken()
    {
        // Arrange
        var user = CreateTestUser();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var token = user.AddRefreshToken("token_value", expiresAt, "127.0.0.1", "Test Agent");

        // Assert
        token.Should().NotBeNull();
        token.Token.Should().Be("token_value");
        token.ExpiresAt.Should().Be(expiresAt);
        token.CreatedFromIp.Should().Be("127.0.0.1");
        user.RefreshTokens.Should().HaveCount(1);
    }

    [Fact]
    public void RevokeRefreshToken_ExistingToken_RevokesToken()
    {
        // Arrange
        var user = CreateTestUser();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        user.AddRefreshToken("token_value", expiresAt, null, null);

        // Act
        user.RevokeRefreshToken("token_value", "replacement_token");

        // Assert
        var token = user.RefreshTokens.First();
        token.IsRevoked.Should().BeTrue();
        token.ReplacedByToken.Should().Be("replacement_token");
    }

    [Fact]
    public void RevokeAllRefreshTokens_RevokesAllActiveTokens()
    {
        // Arrange
        var user = CreateTestUser();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        user.AddRefreshToken("token1", expiresAt, null, null);
        user.AddRefreshToken("token2", expiresAt, null, null);

        // Act
        user.RevokeAllRefreshTokens();

        // Assert
        user.RefreshTokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }

    private User CreateTestUser()
    {
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.FromHash("hashed_password_123");
        return User.Create(_tenantId, email, passwordHash, "John", "Doe");
    }
}
