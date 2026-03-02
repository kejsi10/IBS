using FluentAssertions;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.UnitTests.Policies.Domain;

/// <summary>
/// Unit tests for the PolicyNumber value object.
/// </summary>
public class PolicyNumberTests
{
    [Fact]
    public void Create_ValidPolicyNumber_CreatesPolicyNumber()
    {
        // Arrange
        var value = "POL-2024-001";

        // Act
        var policyNumber = PolicyNumber.Create(value);

        // Assert
        policyNumber.Value.Should().Be("POL-2024-001");
    }

    [Fact]
    public void Create_ValidPolicyNumberWithMixedCase_ConvertsToUpperCase()
    {
        // Arrange
        var value = "pol-2024-001";

        // Act
        var policyNumber = PolicyNumber.Create(value);

        // Assert
        policyNumber.Value.Should().Be("POL-2024-001");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrNullValue_ThrowsArgumentException(string? value)
    {
        // Act
        var act = () => PolicyNumber.Create(value!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*required*");
    }

    [Fact]
    public void Create_TooShort_ThrowsArgumentException()
    {
        // Arrange
        var value = "AB12";

        // Act
        var act = () => PolicyNumber.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*5 and 30 characters*");
    }

    [Fact]
    public void Create_TooLong_ThrowsArgumentException()
    {
        // Arrange
        var value = new string('A', 31);

        // Act
        var act = () => PolicyNumber.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*5 and 30 characters*");
    }

    [Theory]
    [InlineData("POL 2024")]
    [InlineData("POL@2024")]
    [InlineData("POL#2024")]
    public void Create_InvalidCharacters_ThrowsArgumentException(string value)
    {
        // Act
        var act = () => PolicyNumber.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*letters, numbers, and hyphens*");
    }

    [Fact]
    public void Generate_WithPrefix_GeneratesValidPolicyNumber()
    {
        // Arrange
        var prefix = "AUTO";

        // Act
        var policyNumber = PolicyNumber.Generate(prefix);

        // Assert
        policyNumber.Value.Should().StartWith("AUTO-");
        policyNumber.Value.Length.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void Generate_WithoutPrefix_GeneratesValidPolicyNumber()
    {
        // Act
        var policyNumber = PolicyNumber.Generate();

        // Assert
        policyNumber.Value.Should().StartWith("POL-");
        policyNumber.Value.Length.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var policyNumber1 = PolicyNumber.Create("POL-2024-001");
        var policyNumber2 = PolicyNumber.Create("POL-2024-001");

        // Act & Assert
        policyNumber1.Should().Be(policyNumber2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var policyNumber1 = PolicyNumber.Create("POL-2024-001");
        var policyNumber2 = PolicyNumber.Create("POL-2024-002");

        // Act & Assert
        policyNumber1.Should().NotBe(policyNumber2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var policyNumber = PolicyNumber.Create("POL-2024-001");

        // Act
        var result = policyNumber.ToString();

        // Assert
        result.Should().Be("POL-2024-001");
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        // Arrange
        var policyNumber = PolicyNumber.Create("POL-2024-001");

        // Act
        string result = policyNumber;

        // Assert
        result.Should().Be("POL-2024-001");
    }
}
