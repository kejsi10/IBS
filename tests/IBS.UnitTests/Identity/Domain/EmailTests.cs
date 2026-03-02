using FluentAssertions;
using IBS.Identity.Domain.ValueObjects;

namespace IBS.UnitTests.Identity.Domain;

/// <summary>
/// Unit tests for the Email value object.
/// </summary>
public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("firstname.lastname@company.com")]
    [InlineData("email@subdomain.example.com")]
    public void Create_ValidEmail_CreatesEmail(string email)
    {
        // Act
        var emailObj = Email.Create(email);

        // Assert
        emailObj.Value.Should().Be(email);
    }

    [Fact]
    public void Create_ValidEmail_SetsNormalizedValue()
    {
        // Act
        var email = Email.Create("Test@Example.COM");

        // Assert
        email.Value.Should().Be("Test@Example.COM");
        email.NormalizedValue.Should().Be("TEST@EXAMPLE.COM");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    [InlineData("spaces in@email.com")]
    public void Create_InvalidEmail_ThrowsException(string email)
    {
        // Act
        var act = () => Email.Create(email);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*email*");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        // Act
        var email = Email.Create("  test@example.com  ");

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void TryCreate_ValidEmail_ReturnsTrueAndSetsResult()
    {
        // Act
        var success = Email.TryCreate("test@example.com", out var email);

        // Assert
        success.Should().BeTrue();
        email.Should().NotBeNull();
        email!.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void TryCreate_InvalidEmail_ReturnsFalse()
    {
        // Act
        var success = Email.TryCreate("invalid", out var email);

        // Assert
        success.Should().BeFalse();
        email.Should().BeNull();
    }

    [Fact]
    public void TryCreate_NullEmail_ReturnsFalse()
    {
        // Act
        var success = Email.TryCreate(null!, out var email);

        // Assert
        success.Should().BeFalse();
        email.Should().BeNull();
    }

    [Fact]
    public void Equality_SameEmailDifferentCase_AreEqual()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("TEST@EXAMPLE.COM");

        // Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equality_DifferentEmails_AreNotEqual()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void ToString_ReturnsOriginalValue()
    {
        // Arrange
        var email = Email.Create("Test@Example.COM");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("Test@Example.COM");
    }

    [Fact]
    public void ImplicitConversion_ConvertsToString()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string value = email;

        // Assert
        value.Should().Be("test@example.com");
    }
}
