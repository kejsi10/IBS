using FluentAssertions;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.UnitTests.Policies.Domain;

/// <summary>
/// Unit tests for the EffectivePeriod value object.
/// </summary>
public class EffectivePeriodTests
{
    [Fact]
    public void Create_ValidDates_CreatesEffectivePeriod()
    {
        // Arrange
        var effectiveDate = new DateOnly(2024, 1, 1);
        var expirationDate = new DateOnly(2025, 1, 1);

        // Act
        var period = EffectivePeriod.Create(effectiveDate, expirationDate);

        // Assert
        period.EffectiveDate.Should().Be(effectiveDate);
        period.ExpirationDate.Should().Be(expirationDate);
    }

    [Fact]
    public void Create_ExpirationBeforeEffective_ThrowsArgumentException()
    {
        // Arrange
        var effectiveDate = new DateOnly(2025, 1, 1);
        var expirationDate = new DateOnly(2024, 1, 1);

        // Act
        var act = () => EffectivePeriod.Create(effectiveDate, expirationDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be after*");
    }

    [Fact]
    public void Create_SameDates_ThrowsArgumentException()
    {
        // Arrange
        var date = new DateOnly(2024, 1, 1);

        // Act
        var act = () => EffectivePeriod.Create(date, date);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be after*");
    }

    [Fact]
    public void Annual_FromEffectiveDate_CreatesOneYearPeriod()
    {
        // Arrange
        var effectiveDate = new DateOnly(2024, 3, 15);

        // Act
        var period = EffectivePeriod.Annual(effectiveDate);

        // Assert
        period.EffectiveDate.Should().Be(effectiveDate);
        period.ExpirationDate.Should().Be(new DateOnly(2025, 3, 15));
        period.MonthsInPeriod.Should().Be(12);
    }

    [Fact]
    public void SemiAnnual_FromEffectiveDate_CreatesSixMonthPeriod()
    {
        // Arrange
        var effectiveDate = new DateOnly(2024, 1, 1);

        // Act
        var period = EffectivePeriod.SemiAnnual(effectiveDate);

        // Assert
        period.EffectiveDate.Should().Be(effectiveDate);
        period.ExpirationDate.Should().Be(new DateOnly(2024, 7, 1));
        period.MonthsInPeriod.Should().Be(6);
    }

    [Fact]
    public void Quarterly_FromEffectiveDate_CreatesThreeMonthPeriod()
    {
        // Arrange
        var effectiveDate = new DateOnly(2024, 4, 1);

        // Act
        var period = EffectivePeriod.Quarterly(effectiveDate);

        // Assert
        period.EffectiveDate.Should().Be(effectiveDate);
        period.ExpirationDate.Should().Be(new DateOnly(2024, 7, 1));
        period.MonthsInPeriod.Should().Be(3);
    }

    [Fact]
    public void Monthly_FromEffectiveDate_CreatesOneMonthPeriod()
    {
        // Arrange
        var effectiveDate = new DateOnly(2024, 5, 15);

        // Act
        var period = EffectivePeriod.Monthly(effectiveDate);

        // Assert
        period.EffectiveDate.Should().Be(effectiveDate);
        period.ExpirationDate.Should().Be(new DateOnly(2024, 6, 15));
        period.MonthsInPeriod.Should().Be(1);
    }

    [Fact]
    public void Contains_DateWithinPeriod_ReturnsTrue()
    {
        // Arrange
        var period = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));
        var checkDate = new DateOnly(2024, 6, 15);

        // Act
        var result = period.Contains(checkDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_EffectiveDate_ReturnsTrue()
    {
        // Arrange
        var effectiveDate = new DateOnly(2024, 1, 1);
        var period = EffectivePeriod.Annual(effectiveDate);

        // Act
        var result = period.Contains(effectiveDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ExpirationDate_ReturnsFalse()
    {
        // Arrange
        var period = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));
        var expirationDate = new DateOnly(2025, 1, 1);

        // Act
        var result = period.Contains(expirationDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_DateBeforePeriod_ReturnsFalse()
    {
        // Arrange
        var period = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));
        var checkDate = new DateOnly(2023, 12, 31);

        // Act
        var result = period.Contains(checkDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_DateAfterPeriod_ReturnsFalse()
    {
        // Arrange
        var period = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));
        var checkDate = new DateOnly(2025, 6, 1);

        // Act
        var result = period.Contains(checkDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_PastExpiration_ReturnsTrue()
    {
        // Arrange
        var period = EffectivePeriod.Annual(new DateOnly(2020, 1, 1));

        // Act
        var result = period.HasExpired;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_BeforeExpiration_ReturnsFalse()
    {
        // Arrange
        var period = EffectivePeriod.Annual(new DateOnly(2030, 1, 1));

        // Act
        var result = period.HasExpired;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateRenewalPeriod_CreatesNextTermPeriod()
    {
        // Arrange
        var period = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));

        // Act
        var renewalPeriod = period.CreateRenewalPeriod();

        // Assert
        renewalPeriod.EffectiveDate.Should().Be(new DateOnly(2025, 1, 1));
        renewalPeriod.ExpirationDate.Should().Be(new DateOnly(2026, 1, 1));
    }

    [Fact]
    public void CalculateProRataFactor_FullTerm_ReturnsOne()
    {
        // Arrange
        var effectiveDate = new DateOnly(2024, 1, 1);
        var period = EffectivePeriod.Annual(effectiveDate);

        // Act
        var factor = period.CalculateProRataFactor(effectiveDate);

        // Assert
        factor.Should().BeApproximately(1.0m, 0.01m);
    }

    [Fact]
    public void CalculateProRataFactor_HalfTerm_ReturnsApproximatelyHalf()
    {
        // Arrange
        var effectiveDate = new DateOnly(2024, 1, 1);
        var period = EffectivePeriod.Annual(effectiveDate);
        var midTermDate = new DateOnly(2024, 7, 1);

        // Act
        var factor = period.CalculateProRataFactor(midTermDate);

        // Assert
        factor.Should().BeApproximately(0.5m, 0.02m);
    }

    [Fact]
    public void CalculateProRataFactor_EndOfTerm_ReturnsZero()
    {
        // Arrange
        var effectiveDate = new DateOnly(2024, 1, 1);
        var period = EffectivePeriod.Annual(effectiveDate);
        var endDate = new DateOnly(2025, 1, 1);

        // Act
        var factor = period.CalculateProRataFactor(endDate);

        // Assert
        factor.Should().Be(0m);
    }

    [Fact]
    public void Equals_SameDates_ReturnsTrue()
    {
        // Arrange
        var period1 = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));
        var period2 = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));

        // Act & Assert
        period1.Should().Be(period2);
    }

    [Fact]
    public void Equals_DifferentDates_ReturnsFalse()
    {
        // Arrange
        var period1 = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));
        var period2 = EffectivePeriod.Annual(new DateOnly(2024, 2, 1));

        // Act & Assert
        period1.Should().NotBe(period2);
    }
}
