using FluentAssertions;
using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.UnitTests.Carriers.Domain;

/// <summary>
/// Unit tests for the Carrier aggregate root.
/// </summary>
public class CarrierTests
{
    [Fact]
    public void Create_ValidInputs_CreatesCarrier()
    {
        // Arrange
        var name = "Travelers Insurance";
        var code = CarrierCode.Create("TRAV");

        // Act
        var carrier = Carrier.Create(name, code);

        // Assert
        carrier.Should().NotBeNull();
        carrier.Name.Should().Be(name);
        carrier.Code.Value.Should().Be("TRAV");
        carrier.Status.Should().Be(CarrierStatus.Active);
        carrier.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithLegalName_SetsLegalName()
    {
        // Arrange
        var name = "Travelers Insurance";
        var code = CarrierCode.Create("TRAV");
        var legalName = "The Travelers Indemnity Company";

        // Act
        var carrier = Carrier.Create(name, code, legalName);

        // Assert
        carrier.LegalName.Should().Be(legalName);
    }

    [Fact]
    public void Create_EmptyName_ThrowsException()
    {
        // Arrange
        var code = CarrierCode.Create("TRAV");

        // Act
        var act = () => Carrier.Create("", code);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void UpdateBasicInfo_ValidInputs_UpdatesCarrier()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        var newName = "Updated Name";
        var newLegalName = "Updated Legal Name";

        // Act
        carrier.UpdateBasicInfo(newName, newLegalName);

        // Assert
        carrier.Name.Should().Be(newName);
        carrier.LegalName.Should().Be(newLegalName);
    }

    [Fact]
    public void SetAmBestRating_ValidRating_SetsRating()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        var rating = AmBestRating.Create("A+");

        // Act
        carrier.SetAmBestRating(rating);

        // Assert
        carrier.AmBestRating.Should().NotBeNull();
        carrier.AmBestRating!.Value.Should().Be("A+");
    }

    [Fact]
    public void SetNaicCode_ValidCode_SetsCode()
    {
        // Arrange
        var carrier = CreateTestCarrier();

        // Act
        carrier.SetNaicCode("12345");

        // Assert
        carrier.NaicCode.Should().Be("12345");
    }

    [Fact]
    public void SetNaicCode_InvalidCode_ThrowsException()
    {
        // Arrange
        var carrier = CreateTestCarrier();

        // Act
        var act = () => carrier.SetNaicCode("123");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*5-digit*");
    }

    [Fact]
    public void Deactivate_ActiveCarrier_DeactivatesAndRaisesEvent()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        carrier.ClearDomainEvents();

        // Act
        carrier.Deactivate("No longer in business");

        // Assert
        carrier.Status.Should().Be(CarrierStatus.Inactive);
        carrier.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddProduct_ValidInputs_AddsProduct()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        carrier.ClearDomainEvents();

        // Act
        var product = carrier.AddProduct("General Liability", "GL01", LineOfBusiness.GeneralLiability);

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be("General Liability");
        product.Code.Should().Be("GL01");
        carrier.Products.Should().HaveCount(1);
        carrier.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddProduct_DuplicateCode_ThrowsException()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        carrier.AddProduct("General Liability", "GL01", LineOfBusiness.GeneralLiability);

        // Act
        var act = () => carrier.AddProduct("Another GL", "GL01", LineOfBusiness.GeneralLiability);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void AddAppetite_ValidInputs_AddsAppetite()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        carrier.ClearDomainEvents();

        // Act
        var appetite = carrier.AddAppetite(LineOfBusiness.GeneralLiability, "CA,TX,NY");

        // Assert
        appetite.Should().NotBeNull();
        appetite.LineOfBusiness.Should().Be(LineOfBusiness.GeneralLiability);
        appetite.States.Should().Be("CA,TX,NY");
        carrier.Appetites.Should().HaveCount(1);
        carrier.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddAppetite_DuplicateLineOfBusiness_ThrowsException()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");

        // Act
        var act = () => carrier.AddAppetite(LineOfBusiness.GeneralLiability, "CA");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void CoversState_WithAllStates_ReturnsTrue()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");

        // Act
        var result = carrier.CoversState(LineOfBusiness.GeneralLiability, "CA");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CoversState_WithSpecificStates_ReturnsTrueForMatchingState()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        carrier.AddAppetite(LineOfBusiness.GeneralLiability, "CA,TX,NY");

        // Act
        var result = carrier.CoversState(LineOfBusiness.GeneralLiability, "TX");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CoversState_WithSpecificStates_ReturnsFalseForNonMatchingState()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        carrier.AddAppetite(LineOfBusiness.GeneralLiability, "CA,TX,NY");

        // Act
        var result = carrier.CoversState(LineOfBusiness.GeneralLiability, "FL");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CoversState_InactiveCarrier_ReturnsFalse()
    {
        // Arrange
        var carrier = CreateTestCarrier();
        carrier.AddAppetite(LineOfBusiness.GeneralLiability, "ALL");
        carrier.Deactivate();

        // Act
        var result = carrier.CoversState(LineOfBusiness.GeneralLiability, "CA");

        // Assert
        result.Should().BeFalse();
    }

    private static Carrier CreateTestCarrier()
    {
        return Carrier.Create("Test Carrier", CarrierCode.Create("TEST"));
    }
}
