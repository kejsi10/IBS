using FluentAssertions;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.UnitTests.Clients.Domain;

/// <summary>
/// Unit tests for the EmailAddress value object.
/// </summary>
public class EmailAddressTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    public void Create_ValidEmail_CreatesEmailAddress(string email)
    {
        // Act
        var emailAddress = EmailAddress.Create(email);

        // Assert
        emailAddress.Value.Should().Be(email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    public void Create_InvalidEmail_ThrowsException(string email)
    {
        // Act
        var act = () => EmailAddress.Create(email);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*email*");
    }

    [Fact]
    public void Equality_SameEmail_AreEqual()
    {
        // Arrange
        var email1 = EmailAddress.Create("test@example.com");
        var email2 = EmailAddress.Create("test@example.com");

        // Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equality_DifferentEmail_AreNotEqual()
    {
        // Arrange
        var email1 = EmailAddress.Create("test1@example.com");
        var email2 = EmailAddress.Create("test2@example.com");

        // Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void ToString_ReturnsEmailValue()
    {
        // Arrange
        var email = EmailAddress.Create("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }
}
