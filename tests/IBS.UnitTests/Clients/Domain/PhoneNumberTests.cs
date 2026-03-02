using FluentAssertions;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.UnitTests.Clients.Domain;

/// <summary>
/// Unit tests for the PhoneNumber value object.
/// </summary>
public class PhoneNumberTests
{
    [Theory]
    [InlineData("555-123-4567", "5551234567")]
    [InlineData("(555) 123-4567", "5551234567")]
    [InlineData("555.123.4567", "5551234567")]
    [InlineData("5551234567", "5551234567")]
    [InlineData("+1 555 123 4567", "15551234567")]
    public void Create_ValidPhoneNumber_NormalizesToDigitsOnly(string input, string expected)
    {
        // Act
        var phoneNumber = PhoneNumber.Create(input);

        // Assert
        phoneNumber.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    public void Create_InvalidPhoneNumber_ThrowsException(string phone)
    {
        // Act
        var act = () => PhoneNumber.Create(phone);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*phone*");
    }

    [Fact]
    public void Create_WithType_SetsType()
    {
        // Act
        var phone = PhoneNumber.Create("555-123-4567", PhoneNumberType.Work);

        // Assert
        phone.Type.Should().Be(PhoneNumberType.Work);
    }

    [Fact]
    public void Create_WithExtension_SetsExtension()
    {
        // Act
        var phone = PhoneNumber.Create("555-123-4567", PhoneNumberType.Work, "123");

        // Assert
        phone.Extension.Should().Be("123");
    }

    [Fact]
    public void Equality_SamePhoneNumber_AreEqual()
    {
        // Arrange
        var phone1 = PhoneNumber.Create("555-123-4567");
        var phone2 = PhoneNumber.Create("(555) 123-4567");

        // Assert
        phone1.Should().Be(phone2);
    }

    [Fact]
    public void Equality_DifferentPhoneNumber_AreNotEqual()
    {
        // Arrange
        var phone1 = PhoneNumber.Create("555-123-4567");
        var phone2 = PhoneNumber.Create("555-987-6543");

        // Assert
        phone1.Should().NotBe(phone2);
    }

    [Fact]
    public void Equality_SameDigitsDifferentType_AreNotEqual()
    {
        // Arrange
        var phone1 = PhoneNumber.Create("555-123-4567", PhoneNumberType.Mobile);
        var phone2 = PhoneNumber.Create("555-123-4567", PhoneNumberType.Work);

        // Assert
        phone1.Should().NotBe(phone2);
    }

    [Fact]
    public void Format_10DigitNumber_ReturnsUSFormat()
    {
        // Arrange
        var phone = PhoneNumber.Create("5551234567");

        // Act
        var result = phone.Format();

        // Assert
        result.Should().Be("(555) 123-4567");
    }

    [Fact]
    public void Format_11DigitNumberStartingWith1_ReturnsUSFormatWithCountryCode()
    {
        // Arrange
        var phone = PhoneNumber.Create("15551234567");

        // Act
        var result = phone.Format();

        // Assert
        result.Should().Be("+1 (555) 123-4567");
    }

    [Fact]
    public void Format_WithExtension_IncludesExtension()
    {
        // Arrange
        var phone = PhoneNumber.Create("5551234567", PhoneNumberType.Work, "456");

        // Act
        var result = phone.Format();

        // Assert
        result.Should().Be("(555) 123-4567 x456");
    }

    [Fact]
    public void ToString_ReturnsFormattedPhone()
    {
        // Arrange
        var phone = PhoneNumber.Create("555-123-4567");

        // Act
        var result = phone.ToString();

        // Assert
        result.Should().Be("(555) 123-4567");
    }
}
