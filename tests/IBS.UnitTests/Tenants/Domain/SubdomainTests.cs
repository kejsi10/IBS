using FluentAssertions;
using IBS.Tenants.Domain.ValueObjects;

namespace IBS.UnitTests.Tenants.Domain;

/// <summary>
/// Unit tests for the Subdomain value object.
/// </summary>
public class SubdomainTests
{
    [Theory]
    [InlineData("acme")]
    [InlineData("my-company")]
    [InlineData("company123")]
    [InlineData("abc")]
    [InlineData("a1b2c3")]
    [InlineData("test-123-broker")]
    public void Create_ValidSubdomain_CreatesSubdomain(string subdomain)
    {
        // Act
        var result = Subdomain.Create(subdomain);

        // Assert
        result.Value.Should().Be(subdomain.ToLowerInvariant());
    }

    [Fact]
    public void Create_ConvertsToLowercase()
    {
        // Act
        var subdomain = Subdomain.Create("ACME");

        // Assert
        subdomain.Value.Should().Be("acme");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        // Act
        var subdomain = Subdomain.Create("  acme  ");

        // Assert
        subdomain.Value.Should().Be("acme");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_ThrowsException(string value)
    {
        // Act
        var act = () => Subdomain.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*empty*");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void Create_TooShort_ThrowsException(string value)
    {
        // Act
        var act = () => Subdomain.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least 3 characters*");
    }

    [Fact]
    public void Create_TooLong_ThrowsException()
    {
        // Arrange
        var value = new string('a', 64);

        // Act
        var act = () => Subdomain.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 63 characters*");
    }

    [Theory]
    [InlineData("-acme")]
    [InlineData("acme-")]
    [InlineData("-acme-")]
    public void Create_StartsOrEndsWithHyphen_ThrowsException(string value)
    {
        // Act
        var act = () => Subdomain.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must start and end with a letter or number*");
    }

    [Theory]
    [InlineData("acme!")]
    [InlineData("my_company")]
    [InlineData("test.broker")]
    [InlineData("company@123")]
    [InlineData("my company")]
    public void Create_InvalidCharacters_ThrowsException(string value)
    {
        // Act
        var act = () => Subdomain.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*lowercase letters, numbers, and hyphens*");
    }

    [Theory]
    [InlineData("www")]
    [InlineData("api")]
    [InlineData("app")]
    [InlineData("admin")]
    [InlineData("mail")]
    [InlineData("dev")]
    [InlineData("staging")]
    [InlineData("test")]
    [InlineData("support")]
    [InlineData("blog")]
    public void Create_ReservedSubdomain_ThrowsException(string value)
    {
        // Act
        var act = () => Subdomain.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*'{value}' is reserved*");
    }

    [Theory]
    [InlineData("acme", true)]
    [InlineData("my-company", true)]
    [InlineData("ab", false)]
    [InlineData("www", false)]
    [InlineData("-invalid", false)]
    [InlineData("invalid-", false)]
    [InlineData("", false)]
    public void IsValid_ReturnsCorrectResult(string value, bool expected)
    {
        // Act
        var result = Subdomain.IsValid(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        var subdomain1 = Subdomain.Create("acme");
        var subdomain2 = Subdomain.Create("acme");

        // Assert
        subdomain1.Should().Be(subdomain2);
    }

    [Fact]
    public void Equality_SameValueDifferentCase_AreEqual()
    {
        // Arrange
        var subdomain1 = Subdomain.Create("acme");
        var subdomain2 = Subdomain.Create("ACME");

        // Assert
        subdomain1.Should().Be(subdomain2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var subdomain1 = Subdomain.Create("acme");
        var subdomain2 = Subdomain.Create("beta");

        // Assert
        subdomain1.Should().NotBe(subdomain2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var subdomain = Subdomain.Create("acme");

        // Act
        var result = subdomain.ToString();

        // Assert
        result.Should().Be("acme");
    }
}
