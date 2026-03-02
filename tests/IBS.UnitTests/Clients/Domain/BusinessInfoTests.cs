using FluentAssertions;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.UnitTests.Clients.Domain;

/// <summary>
/// Unit tests for the BusinessInfo value object.
/// </summary>
public class BusinessInfoTests
{
    [Fact]
    public void Create_RequiredFieldsOnly_CreatesBusinessInfo()
    {
        // Act
        var info = BusinessInfo.Create("Acme Corp", "LLC");

        // Assert
        info.Name.Should().Be("Acme Corp");
        info.BusinessType.Should().Be("LLC");
        info.DbaName.Should().BeNull();
        info.Industry.Should().BeNull();
        info.YearEstablished.Should().BeNull();
        info.NumberOfEmployees.Should().BeNull();
        info.AnnualRevenue.Should().BeNull();
        info.Website.Should().BeNull();
    }

    [Fact]
    public void Create_AllFields_SetsAllValues()
    {
        // Act
        var info = BusinessInfo.Create(
            "Acme Corporation",
            "Corporation",
            "Acme Co",
            "Technology",
            2010,
            500,
            10000000m,
            "https://acme.com");

        // Assert
        info.Name.Should().Be("Acme Corporation");
        info.BusinessType.Should().Be("Corporation");
        info.DbaName.Should().Be("Acme Co");
        info.Industry.Should().Be("Technology");
        info.YearEstablished.Should().Be(2010);
        info.NumberOfEmployees.Should().Be(500);
        info.AnnualRevenue.Should().Be(10000000m);
        info.Website.Should().Be("https://acme.com");
    }

    [Fact]
    public void Create_EmptyName_ThrowsException()
    {
        // Act
        var act = () => BusinessInfo.Create("", "LLC");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void Create_EmptyBusinessType_ThrowsException()
    {
        // Act
        var act = () => BusinessInfo.Create("Acme Corp", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*business type*");
    }

    [Fact]
    public void Create_FutureYearEstablished_ThrowsException()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 1;

        // Act
        var act = () => BusinessInfo.Create("Acme Corp", "LLC", yearEstablished: futureYear);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Year established*");
    }

    [Fact]
    public void Create_NegativeEmployees_ThrowsException()
    {
        // Act
        var act = () => BusinessInfo.Create("Acme Corp", "LLC", numberOfEmployees: -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void Create_NegativeRevenue_ThrowsException()
    {
        // Act
        var act = () => BusinessInfo.Create("Acme Corp", "LLC", annualRevenue: -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var info1 = BusinessInfo.Create("Acme Corp", "LLC", industry: "Tech");
        var info2 = BusinessInfo.Create("Acme Corp", "LLC", industry: "Tech");

        // Assert
        info1.Should().Be(info2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var info1 = BusinessInfo.Create("Acme Corp", "LLC");
        var info2 = BusinessInfo.Create("Beta Inc", "Corporation");

        // Assert
        info1.Should().NotBe(info2);
    }
}
