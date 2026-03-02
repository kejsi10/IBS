using IBS.BuildingBlocks.Domain.ValueObjects;

namespace IBS.BuildingBlocks.Application;

/// <summary>
/// Service for currency exchange rate operations.
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Gets the exchange rate from one currency to another.
    /// </summary>
    /// <param name="fromCurrency">The source currency code.</param>
    /// <param name="toCurrency">The target currency code.</param>
    /// <returns>The exchange rate.</returns>
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);

    /// <summary>
    /// Converts a monetary amount to a different currency.
    /// </summary>
    /// <param name="money">The money to convert.</param>
    /// <param name="targetCurrency">The target currency code.</param>
    /// <returns>The converted money in the target currency.</returns>
    Task<Money> ConvertAsync(Money money, string targetCurrency);
}
