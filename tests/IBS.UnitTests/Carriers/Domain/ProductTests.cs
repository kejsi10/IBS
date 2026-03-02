using FluentAssertions;
using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.UnitTests.Carriers.Domain;

/// <summary>
/// Unit tests for the Product entity.
/// </summary>
public class ProductTests
{
    [Fact]
    public void SetMinimumPremium_ValidAmount_SetsValue()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var product = carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);

        // Act
        product.SetMinimumPremium(500m);

        // Assert
        product.MinimumPremium.Should().Be(500m);
    }

    [Fact]
    public void SetMinimumPremium_NegativeAmount_ThrowsException()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var product = carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);

        // Act
        var act = () => product.SetMinimumPremium(-100m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void SetEffectivePeriod_ValidDates_SetsDates()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var product = carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);
        var effectiveDate = new DateOnly(2024, 1, 1);
        var expirationDate = new DateOnly(2024, 12, 31);

        // Act
        product.SetEffectivePeriod(effectiveDate, expirationDate);

        // Assert
        product.EffectiveDate.Should().Be(effectiveDate);
        product.ExpirationDate.Should().Be(expirationDate);
    }

    [Fact]
    public void SetEffectivePeriod_EffectiveAfterExpiration_ThrowsException()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var product = carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);
        var effectiveDate = new DateOnly(2024, 12, 31);
        var expirationDate = new DateOnly(2024, 1, 1);

        // Act
        var act = () => product.SetEffectivePeriod(effectiveDate, expirationDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*before*");
    }

    [Fact]
    public void IsAvailableOn_ActiveProductWithinPeriod_ReturnsTrue()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var product = carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);
        product.SetEffectivePeriod(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));

        // Act
        var result = product.IsAvailableOn(new DateOnly(2024, 6, 15));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAvailableOn_BeforeEffectiveDate_ReturnsFalse()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var product = carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);
        product.SetEffectivePeriod(new DateOnly(2024, 6, 1), null);

        // Act
        var result = product.IsAvailableOn(new DateOnly(2024, 1, 1));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAvailableOn_AfterExpirationDate_ReturnsFalse()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var product = carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);
        product.SetEffectivePeriod(null, new DateOnly(2024, 6, 30));

        // Act
        var result = product.IsAvailableOn(new DateOnly(2024, 12, 1));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAvailableOn_InactiveProduct_ReturnsFalse()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var product = carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);
        product.Deactivate();

        // Act
        var result = product.IsAvailableOn(new DateOnly(2024, 6, 15));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Activate_InactiveProduct_ActivatesProduct()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var product = carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);
        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_ValidInputs_UpdatesProduct()
    {
        // Arrange
        var carrier = Carrier.Create("Test", CarrierCode.Create("TEST"));
        var product = carrier.AddProduct("GL Policy", "GL01", LineOfBusiness.GeneralLiability);

        // Act
        product.Update("Updated GL Policy", "Updated description");

        // Assert
        product.Name.Should().Be("Updated GL Policy");
        product.Description.Should().Be("Updated description");
    }
}
