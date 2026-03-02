using FluentAssertions;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.UnitTests.Clients.Domain;

/// <summary>
/// Unit tests for the PersonName value object.
/// </summary>
public class PersonNameTests
{
    [Fact]
    public void Create_ValidInputs_CreatesPersonName()
    {
        // Act
        var name = PersonName.Create("John", "Doe");

        // Assert
        name.FirstName.Should().Be("John");
        name.LastName.Should().Be("Doe");
        name.MiddleName.Should().BeNull();
        name.Suffix.Should().BeNull();
    }

    [Fact]
    public void Create_WithMiddleNameAndSuffix_SetsAllFields()
    {
        // Act
        var name = PersonName.Create("John", "Doe", "William", "Jr.");

        // Assert
        name.FirstName.Should().Be("John");
        name.LastName.Should().Be("Doe");
        name.MiddleName.Should().Be("William");
        name.Suffix.Should().Be("Jr.");
    }

    [Fact]
    public void Create_EmptyFirstName_ThrowsException()
    {
        // Act
        var act = () => PersonName.Create("", "Doe");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*first name*");
    }

    [Fact]
    public void Create_EmptyLastName_ThrowsException()
    {
        // Act
        var act = () => PersonName.Create("John", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*last name*");
    }

    [Fact]
    public void Create_WhitespaceFirstName_ThrowsException()
    {
        // Act
        var act = () => PersonName.Create("   ", "Doe");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*first name*");
    }

    [Fact]
    public void FullName_NoMiddleName_ReturnsFirstAndLast()
    {
        // Arrange
        var name = PersonName.Create("John", "Doe");

        // Act
        var fullName = name.FullName;

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void FullName_WithMiddleName_IncludesMiddleName()
    {
        // Arrange
        var name = PersonName.Create("John", "Doe", "William");

        // Act
        var fullName = name.FullName;

        // Assert
        fullName.Should().Be("John William Doe");
    }

    [Fact]
    public void FullName_WithSuffix_IncludesSuffix()
    {
        // Arrange
        var name = PersonName.Create("John", "Doe", suffix: "Jr.");

        // Act
        var fullName = name.FullName;

        // Assert
        fullName.Should().Be("John Doe Jr.");
    }

    [Fact]
    public void FullName_WithAllFields_FormatsCorrectly()
    {
        // Arrange
        var name = PersonName.Create("John", "Doe", "William", "III");

        // Act
        var fullName = name.FullName;

        // Assert
        fullName.Should().Be("John William Doe III");
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var name1 = PersonName.Create("John", "Doe", "William");
        var name2 = PersonName.Create("John", "Doe", "William");

        // Assert
        name1.Should().Be(name2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var name1 = PersonName.Create("John", "Doe");
        var name2 = PersonName.Create("Jane", "Doe");

        // Assert
        name1.Should().NotBe(name2);
    }
}
