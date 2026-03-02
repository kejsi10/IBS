namespace IBS.BuildingBlocks.Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary amount with currency.
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>
{
    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency code (ISO 4217).
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Money()
    {
        Amount = 0;
        Currency = "USD";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency code (defaults to USD).</param>
    public Money(decimal amount, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        if (currency.Trim().Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Creates a new Money instance with validation.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency code (defaults to USD).</param>
    /// <param name="allowNegative">Whether to allow negative amounts (for return premium, etc.).</param>
    /// <returns>A new Money instance.</returns>
    public static Money Create(decimal amount, string currency = "USD", bool allowNegative = false)
    {
        if (amount < 0 && !allowNegative)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        return new Money(Math.Round(amount, 2), currency);
    }

    /// <summary>
    /// Creates a Money instance that allows negative values (for endorsement premium changes).
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency code (defaults to USD).</param>
    /// <returns>A new Money instance.</returns>
    public static Money CreateWithSign(decimal amount, string currency = "USD")
    {
        return Create(amount, currency, allowNegative: true);
    }

    /// <summary>
    /// Creates a zero amount in the specified currency.
    /// </summary>
    /// <param name="currency">The currency code (defaults to USD).</param>
    /// <returns>A zero Money instance.</returns>
    public static Money Zero(string currency = "USD") => Create(0, currency);

    /// <summary>
    /// Creates a money value in USD.
    /// </summary>
    /// <param name="amount">The amount.</param>
    /// <returns>A new Money instance.</returns>
    public static Money Usd(decimal amount) => new(amount, "USD");

    /// <summary>
    /// Adds two monetary amounts.
    /// </summary>
    /// <param name="other">The money to add.</param>
    /// <returns>A new Money with the summed amount.</returns>
    public Money Add(Money other)
    {
        EnsureSameCurrency(this, other);
        return Create(Amount + other.Amount, Currency, allowNegative: true);
    }

    /// <summary>
    /// Subtracts a monetary amount from this one.
    /// </summary>
    /// <param name="other">The money to subtract.</param>
    /// <returns>A new Money with the resulting amount.</returns>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(this, other);
        return Create(Amount - other.Amount, Currency, allowNegative: true);
    }

    /// <summary>
    /// Multiplies the amount by a factor.
    /// </summary>
    /// <param name="factor">The multiplication factor.</param>
    /// <returns>A new Money with the multiplied amount.</returns>
    public Money Multiply(decimal factor)
    {
        return Create(Amount * factor, Currency, allowNegative: true);
    }

    /// <summary>
    /// Determines if this amount is greater than another.
    /// </summary>
    /// <param name="other">The other money.</param>
    /// <returns>True if greater.</returns>
    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount > other.Amount;
    }

    /// <summary>
    /// Determines if this amount is less than another.
    /// </summary>
    /// <param name="other">The other money.</param>
    /// <returns>True if less.</returns>
    public bool IsLessThan(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount < other.Amount;
    }

    /// <summary>
    /// Checks if this amount is positive.
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Checks if this amount is negative.
    /// </summary>
    public bool IsNegative => Amount < 0;

    /// <summary>
    /// Checks if this amount is zero.
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <summary>
    /// Adds two monetary amounts.
    /// </summary>
    public static Money operator +(Money left, Money right) => left.Add(right);

    /// <summary>
    /// Subtracts two monetary amounts.
    /// </summary>
    public static Money operator -(Money left, Money right) => left.Subtract(right);

    /// <summary>
    /// Multiplies a monetary amount by a factor.
    /// </summary>
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);

    /// <summary>
    /// Multiplies a monetary amount by a factor.
    /// </summary>
    public static Money operator *(decimal factor, Money money) => money.Multiply(factor);

    /// <summary>
    /// Determines if one money is greater than another.
    /// </summary>
    public static bool operator >(Money left, Money right) => left.IsGreaterThan(right);

    /// <summary>
    /// Determines if one money is less than another.
    /// </summary>
    public static bool operator <(Money left, Money right) => left.IsLessThan(right);

    /// <summary>
    /// Determines if one money is greater than or equal to another.
    /// </summary>
    public static bool operator >=(Money left, Money right) => !left.IsLessThan(right);

    /// <summary>
    /// Determines if one money is less than or equal to another.
    /// </summary>
    public static bool operator <=(Money left, Money right) => !left.IsGreaterThan(right);

    /// <inheritdoc />
    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Amount:N2} {Currency}";

    /// <summary>
    /// Formats the amount as a currency string with symbol.
    /// </summary>
    /// <returns>A formatted currency string.</returns>
    public string Format() => Currency switch
    {
        "USD" => $"${Amount:N2}",
        "EUR" => $"\u20ac{Amount:N2}",
        "GBP" => $"\u00a3{Amount:N2}",
        "PLN" => $"{Amount:N2} z\u0142",
        _ => $"{Amount:N2} {Currency}"
    };

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on money with different currencies: {left.Currency} and {right.Currency}.");
    }
}
