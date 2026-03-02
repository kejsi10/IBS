using FluentAssertions;
using IBS.Identity.Domain.ValueObjects;

namespace IBS.UnitTests.Identity.Domain;

/// <summary>
/// Unit tests for the PasswordHash value object.
/// </summary>
public class PasswordHashTests
{
    [Fact]
    public void FromHash_ValidHash_CreatesPasswordHash()
    {
        // Arrange
        var hashedValue = "hashed_password_value_123";

        // Act
        var passwordHash = PasswordHash.FromHash(hashedValue);

        // Assert
        passwordHash.Value.Should().Be(hashedValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FromHash_EmptyOrNullHash_ThrowsException(string? hash)
    {
        // Act
        var act = () => PasswordHash.FromHash(hash!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password hash*");
    }

    [Fact]
    public void Equality_SameHash_AreEqual()
    {
        // Arrange
        var hash1 = PasswordHash.FromHash("hash_value");
        var hash2 = PasswordHash.FromHash("hash_value");

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Equality_DifferentHash_AreNotEqual()
    {
        // Arrange
        var hash1 = PasswordHash.FromHash("hash_value_1");
        var hash2 = PasswordHash.FromHash("hash_value_2");

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void ToString_ReturnsMaskedValue()
    {
        // Arrange
        var passwordHash = PasswordHash.FromHash("sensitive_hash_value");

        // Act
        var result = passwordHash.ToString();

        // Assert
        result.Should().Be("***");
    }

    [Fact]
    public void ImplicitConversion_ConvertsToString()
    {
        // Arrange
        var passwordHash = PasswordHash.FromHash("hash_value");

        // Act
        string value = passwordHash;

        // Assert
        value.Should().Be("hash_value");
    }
}
