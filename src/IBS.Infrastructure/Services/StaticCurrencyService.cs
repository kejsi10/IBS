using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Domain.ValueObjects;

namespace IBS.Infrastructure.Services;

/// <summary>
/// Static currency service with hardcoded exchange rates.
/// Replace with an external API (e.g., ECB, Open Exchange Rates) for production use.
/// </summary>
public sealed class StaticCurrencyService : ICurrencyService
{
    /// <summary>
    /// Hardcoded exchange rates relative to USD.
    /// </summary>
    private static readonly Dictionary<string, decimal> RatesToUsd = new()
    {
        ["USD"] = 1.0m,
        ["EUR"] = 0.92m,
        ["GBP"] = 0.79m,
        ["PLN"] = 4.02m,
    };

    /// <inheritdoc />
    public Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(1.0m);

        var from = fromCurrency.ToUpperInvariant();
        var to = toCurrency.ToUpperInvariant();

        if (!RatesToUsd.TryGetValue(from, out var fromRate))
            throw new ArgumentException($"Unsupported currency: {from}", nameof(fromCurrency));

        if (!RatesToUsd.TryGetValue(to, out var toRate))
            throw new ArgumentException($"Unsupported currency: {to}", nameof(toCurrency));

        // Convert through USD: from -> USD -> to
        var rate = toRate / fromRate;
        return Task.FromResult(Math.Round(rate, 6));
    }

    /// <inheritdoc />
    public async Task<Money> ConvertAsync(Money money, string targetCurrency)
    {
        if (string.Equals(money.Currency, targetCurrency, StringComparison.OrdinalIgnoreCase))
            return money;

        var rate = await GetExchangeRateAsync(money.Currency, targetCurrency);
        var convertedAmount = Math.Round(money.Amount * rate, 2);
        return Money.Create(convertedAmount, targetCurrency, allowNegative: money.IsNegative);
    }
}
