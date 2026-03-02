using FluentAssertions;
using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.UnitTests.Carriers.Domain;

/// <summary>
/// Unit tests for the Appetite entity.
/// </summary>
public class AppetiteTests
{
    [Fact]
    public void SetYearsInBusinessRequirement_ValidRange_SetsValues()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");

        // Act
        appetite.SetYearsInBusinessRequirement(2, 10);

        // Assert
        appetite.MinYearsInBusiness.Should().Be(2);
        appetite.MaxYearsInBusiness.Should().Be(10);
    }

    [Fact]
    public void SetYearsInBusinessRequirement_NegativeMin_ThrowsException()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");

        // Act
        var act = () => appetite.SetYearsInBusinessRequirement(-1, 10);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void SetYearsInBusinessRequirement_MinGreaterThanMax_ThrowsException()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");

        // Act
        var act = () => appetite.SetYearsInBusinessRequirement(10, 5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed*");
    }

    [Fact]
    public void SetRevenueRequirement_ValidRange_SetsValues()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");

        // Act
        appetite.SetRevenueRequirement(100000m, 10000000m);

        // Assert
        appetite.MinAnnualRevenue.Should().Be(100000m);
        appetite.MaxAnnualRevenue.Should().Be(10000000m);
    }

    [Fact]
    public void SetEmployeeRequirement_ValidRange_SetsValues()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");

        // Act
        appetite.SetEmployeeRequirement(5, 100);

        // Assert
        appetite.MinEmployees.Should().Be(5);
        appetite.MaxEmployees.Should().Be(100);
    }

    [Fact]
    public void SetIndustryRestrictions_ValidInputs_SetsValues()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");

        // Act
        appetite.SetIndustryRestrictions("5411,5412,5413", "4512,4513");

        // Assert
        appetite.AcceptedIndustries.Should().Be("5411,5412,5413");
        appetite.ExcludedIndustries.Should().Be("4512,4513");
    }

    [Fact]
    public void CoversState_AllStates_ReturnsTrue()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");

        // Act
        var result = appetite.CoversState("CA");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CoversState_MatchingState_ReturnsTrue()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "CA,TX,NY");

        // Act
        var result = appetite.CoversState("TX");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CoversState_NonMatchingState_ReturnsFalse()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "CA,TX,NY");

        // Act
        var result = appetite.CoversState("FL");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CoversState_InactiveAppetite_ReturnsFalse()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");
        appetite.Deactivate();

        // Act
        var result = appetite.CoversState("CA");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateStates_ValidStates_UpdatesStates()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "CA");

        // Act
        appetite.UpdateStates("CA, TX, NY");

        // Assert
        appetite.States.Should().Be("CA, TX, NY");
    }

    [Fact]
    public void UpdateStates_EmptyStates_ThrowsException()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "CA");

        // Act
        var act = () => appetite.UpdateStates("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*empty*");
    }
}
