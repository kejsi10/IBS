using FluentAssertions;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.UnitTests.Policies.Domain;

/// <summary>
/// Unit tests for the Money value object.
/// </summary>
public class MoneyTests
{
    [Fact]
    public void Create_ValidPositiveAmount_CreatesMoney()
    {
        // Arrange
        var amount = 1500.50m;

        // Act
        var money = Money.Create(amount);

        // Assert
        money.Amount.Should().Be(1500.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_ZeroAmount_CreatesMoney()
    {
        // Arrange
        var amount = 0m;

        // Act
        var money = Money.Create(amount);

        // Assert
        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Create_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var amount = -100m;

        // Act
        var act = () => Money.Create(amount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Create_NegativeAmountWithAllowNegative_CreatesMoney()
    {
        // Arrange
        var amount = -100m;

        // Act
        var money = Money.Create(amount, "USD", allowNegative: true);

        // Assert
        money.Amount.Should().Be(-100m);
    }

    [Fact]
    public void CreateWithSign_PositiveAmount_CreatesMoney()
    {
        // Arrange
        var amount = 500m;

        // Act
        var money = Money.CreateWithSign(amount);

        // Assert
        money.Amount.Should().Be(500m);
    }

    [Fact]
    public void CreateWithSign_NegativeAmount_CreatesMoney()
    {
        // Arrange
        var amount = -250m;

        // Act
        var money = Money.CreateWithSign(amount);

        // Assert
        money.Amount.Should().Be(-250m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyCurrency_ThrowsArgumentException(string currency)
    {
        // Act
        var act = () => Money.Create(100m, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Currency*");
    }

    [Theory]
    [InlineData("USDA")]
    [InlineData("US")]
    public void Create_InvalidCurrencyLength_ThrowsArgumentException(string currency)
    {
        // Act
        var act = () => Money.Create(100m, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*3-letter ISO*");
    }

    [Fact]
    public void Zero_CreatesZeroUsdMoney()
    {
        // Act
        var money = Money.Zero();

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Zero_WithCurrency_CreatesZeroMoneyWithCurrency()
    {
        // Act
        var money = Money.Zero("EUR");

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(50m);

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "EUR");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(30m);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Multiply_ByFactor_ReturnsProduct()
    {
        // Arrange
        var money = Money.Create(100m);

        // Act
        var result = money.Multiply(1.5m);

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Amount_PositiveValue_IsGreaterThanZero()
    {
        // Arrange
        var money = Money.Create(100m);
        var zero = Money.Zero();

        // Act & Assert
        money.IsGreaterThan(zero).Should().BeTrue();
    }

    [Fact]
    public void Amount_ZeroValue_IsNotGreaterThanZero()
    {
        // Arrange
        var money = Money.Zero();
        var zero = Money.Zero();

        // Act & Assert
        money.IsGreaterThan(zero).Should().BeFalse();
    }

    [Fact]
    public void Amount_ZeroValue_HasZeroAmount()
    {
        // Arrange
        var money = Money.Zero();

        // Act & Assert
        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Amount_NegativeValue_HasNegativeAmount()
    {
        // Arrange
        var money = Money.CreateWithSign(-100m);

        // Act & Assert
        money.Amount.Should().BeLessThan(0m);
    }

    [Fact]
    public void Equals_SameAmountAndCurrency_ReturnsTrue()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(100m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
    }

    [Fact]
    public void Equals_DifferentAmount_ReturnsFalse()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(200m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void Equals_DifferentCurrency_ReturnsFalse()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(100m, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var money = Money.Create(1500.50m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Contain("1,500.50");
        result.Should().Contain("USD");
    }
}
