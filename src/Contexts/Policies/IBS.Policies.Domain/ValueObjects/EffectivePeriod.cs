using IBS.BuildingBlocks.Domain;

namespace IBS.Policies.Domain.ValueObjects;

/// <summary>
/// Represents a policy's effective period (effective date to expiration date).
/// </summary>
public sealed class EffectivePeriod : ValueObject
{
    /// <summary>
    /// Gets the effective date (when coverage begins).
    /// </summary>
    public DateOnly EffectiveDate { get; }

    /// <summary>
    /// Gets the expiration date (when coverage ends).
    /// </summary>
    public DateOnly ExpirationDate { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private EffectivePeriod()
    {
        EffectiveDate = DateOnly.MinValue;
        ExpirationDate = DateOnly.MinValue;
    }

    private EffectivePeriod(DateOnly effectiveDate, DateOnly expirationDate)
    {
        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
    }

    /// <summary>
    /// Creates a new effective period.
    /// </summary>
    /// <param name="effectiveDate">The effective date.</param>
    /// <param name="expirationDate">The expiration date.</param>
    /// <returns>A new EffectivePeriod instance.</returns>
    /// <exception cref="ArgumentException">Thrown when dates are invalid.</exception>
    public static EffectivePeriod Create(DateOnly effectiveDate, DateOnly expirationDate)
    {
        if (expirationDate <= effectiveDate)
            throw new ArgumentException("Expiration date must be after effective date.", nameof(expirationDate));

        return new EffectivePeriod(effectiveDate, expirationDate);
    }

    /// <summary>
    /// Creates an annual policy period.
    /// </summary>
    /// <param name="effectiveDate">The effective date.</param>
    /// <returns>A new EffectivePeriod instance with a one-year term.</returns>
    public static EffectivePeriod Annual(DateOnly effectiveDate)
    {
        return Create(effectiveDate, effectiveDate.AddYears(1));
    }

    /// <summary>
    /// Creates a semi-annual policy period.
    /// </summary>
    /// <param name="effectiveDate">The effective date.</param>
    /// <returns>A new EffectivePeriod instance with a six-month term.</returns>
    public static EffectivePeriod SemiAnnual(DateOnly effectiveDate)
    {
        return Create(effectiveDate, effectiveDate.AddMonths(6));
    }

    /// <summary>
    /// Creates a quarterly policy period.
    /// </summary>
    /// <param name="effectiveDate">The effective date.</param>
    /// <returns>A new EffectivePeriod instance with a three-month term.</returns>
    public static EffectivePeriod Quarterly(DateOnly effectiveDate)
    {
        return Create(effectiveDate, effectiveDate.AddMonths(3));
    }

    /// <summary>
    /// Creates a monthly policy period.
    /// </summary>
    /// <param name="effectiveDate">The effective date.</param>
    /// <returns>A new EffectivePeriod instance with a one-month term.</returns>
    public static EffectivePeriod Monthly(DateOnly effectiveDate)
    {
        return Create(effectiveDate, effectiveDate.AddMonths(1));
    }

    /// <summary>
    /// Gets the number of days in the policy period.
    /// </summary>
    public int DaysInPeriod => ExpirationDate.DayNumber - EffectiveDate.DayNumber;

    /// <summary>
    /// Gets the number of months in the policy period.
    /// </summary>
    public int MonthsInPeriod => ((ExpirationDate.Year - EffectiveDate.Year) * 12) + ExpirationDate.Month - EffectiveDate.Month;

    /// <summary>
    /// Determines if the policy is currently in force as of a given date.
    /// </summary>
    /// <param name="asOfDate">The date to check.</param>
    /// <returns>True if the policy is in force; otherwise, false.</returns>
    public bool IsInForce(DateOnly asOfDate)
    {
        return asOfDate >= EffectiveDate && asOfDate < ExpirationDate;
    }

    /// <summary>
    /// Determines if the policy is currently in force as of today.
    /// </summary>
    public bool IsCurrentlyInForce => IsInForce(DateOnly.FromDateTime(DateTime.UtcNow));

    /// <summary>
    /// Determines if the policy has expired.
    /// </summary>
    public bool HasExpired => DateOnly.FromDateTime(DateTime.UtcNow) >= ExpirationDate;

    /// <summary>
    /// Determines if the policy is not yet effective.
    /// </summary>
    public bool IsFuture => DateOnly.FromDateTime(DateTime.UtcNow) < EffectiveDate;

    /// <summary>
    /// Gets the number of days until expiration (0 if already expired).
    /// </summary>
    public int DaysUntilExpiration
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (today >= ExpirationDate) return 0;
            return ExpirationDate.DayNumber - today.DayNumber;
        }
    }

    /// <summary>
    /// Determines if a date falls within the policy period.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>True if the date is within the period; otherwise, false.</returns>
    public bool Contains(DateOnly date)
    {
        return date >= EffectiveDate && date < ExpirationDate;
    }

    /// <summary>
    /// Creates a renewal period based on this period.
    /// </summary>
    /// <returns>A new EffectivePeriod starting at this period's expiration.</returns>
    public EffectivePeriod CreateRenewalPeriod()
    {
        var termLength = MonthsInPeriod;
        return Create(ExpirationDate, ExpirationDate.AddMonths(termLength));
    }

    /// <summary>
    /// Calculates the pro-rata factor for a given date within the period.
    /// </summary>
    /// <param name="asOfDate">The date to calculate the factor for.</param>
    /// <returns>The pro-rata factor (0 to 1).</returns>
    public decimal CalculateProRataFactor(DateOnly asOfDate)
    {
        if (asOfDate <= EffectiveDate) return 1m;
        if (asOfDate >= ExpirationDate) return 0m;

        var elapsed = asOfDate.DayNumber - EffectiveDate.DayNumber;
        var remaining = DaysInPeriod - elapsed;
        return (decimal)remaining / DaysInPeriod;
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return EffectiveDate;
        yield return ExpirationDate;
    }

    /// <inheritdoc />
    public override string ToString() => $"{EffectiveDate:MM/dd/yyyy} - {ExpirationDate:MM/dd/yyyy}";
}
