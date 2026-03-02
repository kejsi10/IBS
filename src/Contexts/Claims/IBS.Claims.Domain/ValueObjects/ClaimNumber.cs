using System.Text.RegularExpressions;
using IBS.BuildingBlocks.Domain;

namespace IBS.Claims.Domain.ValueObjects;

/// <summary>
/// Represents a unique claim number with format CLM-YYYYMMDD-XXXX.
/// </summary>
public sealed partial class ClaimNumber : ValueObject
{
    private static readonly Regex ClaimNumberRegex = CreateClaimNumberRegex();

    /// <summary>
    /// Gets the claim number value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private ClaimNumber()
    {
        Value = string.Empty;
    }

    private ClaimNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new ClaimNumber from an existing value.
    /// </summary>
    /// <param name="value">The claim number string.</param>
    /// <returns>A new ClaimNumber instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the claim number is invalid.</exception>
    public static ClaimNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Claim number is required.", nameof(value));

        var trimmed = value.Trim().ToUpperInvariant();

        if (!ClaimNumberRegex.IsMatch(trimmed))
            throw new ArgumentException("Claim number must match format CLM-YYYYMMDD-XXXX.", nameof(value));

        return new ClaimNumber(trimmed);
    }

    /// <summary>
    /// Generates a new claim number with timestamp and random suffix.
    /// </summary>
    /// <returns>A new ClaimNumber instance.</returns>
    public static ClaimNumber Generate()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        var value = $"CLM-{datePart}-{random}";
        return new ClaimNumber(value);
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Implicit conversion to string.
    /// </summary>
    public static implicit operator string(ClaimNumber claimNumber) => claimNumber.Value;

    [GeneratedRegex(@"^CLM-\d{8}-\d{4}$")]
    private static partial Regex CreateClaimNumberRegex();
}
