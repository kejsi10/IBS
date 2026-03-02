using FluentAssertions;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.UnitTests.Carriers.Domain;

/// <summary>
/// Unit tests for the CarrierCode value object.
/// </summary>
public class CarrierCodeTests
{
    [Theory]
    [InlineData("TRAV")]
    [InlineData("CNA")]
    [InlineData("HART1234")]
    [InlineData("ab")]
    public void Create_ValidCode_CreatesCarrierCode(string code)
    {
        // Act
        var carrierCode = CarrierCode.Create(code);

        // Assert
        carrierCode.Should().NotBeNull();
        carrierCode.Value.Should().Be(code.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyOrNullCode_ThrowsException(string? code)
    {
        // Act
        var act = () => CarrierCode.Create(code!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public void Create_TooShortCode_ThrowsException()
    {
        // Act
        var act = () => CarrierCode.Create("A");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*2*10*");
    }

    [Fact]
    public void Create_TooLongCode_ThrowsException()
    {
        // Act
        var act = () => CarrierCode.Create("VERYLONGCODE");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*2*10*");
    }

    [Theory]
    [InlineData("TRAV-1")]
    [InlineData("TRAV_1")]
    [InlineData("TRAV.1")]
    [InlineData("TRAV 1")]
    public void Create_InvalidCharacters_ThrowsException(string code)
    {
        // Act
        var act = () => CarrierCode.Create(code);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*letters and numbers*");
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        var code1 = CarrierCode.Create("TRAV");
        var code2 = CarrierCode.Create("trav");

        // Assert
        code1.Should().Be(code2);
    }

    [Fact]
    public void ImplicitConversion_ReturnsValue()
    {
        // Arrange
        var carrierCode = CarrierCode.Create("TRAV");

        // Act
        string value = carrierCode;

        // Assert
        value.Should().Be("TRAV");
    }
}
