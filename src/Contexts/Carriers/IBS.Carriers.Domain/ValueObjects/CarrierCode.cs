using System.Text.RegularExpressions;
using IBS.BuildingBlocks.Domain;

namespace IBS.Carriers.Domain.ValueObjects;

/// <summary>
/// Represents a unique carrier code (e.g., "TRAV", "HART", "CNA").
/// </summary>
public sealed partial class CarrierCode : ValueObject
{
    /// <summary>
    /// Gets the carrier code value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private CarrierCode() => Value = string.Empty;

    private CarrierCode(string value) => Value = value;

    /// <summary>
    /// Creates a new carrier code.
    /// </summary>
    /// <param name="code">The carrier code (2-10 alphanumeric characters).</param>
    /// <returns>A new CarrierCode instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the code is invalid.</exception>
    public static CarrierCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Carrier code cannot be empty.", nameof(code));

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length < 2 || normalizedCode.Length > 10)
            throw new ArgumentException("Carrier code must be between 2 and 10 characters.", nameof(code));

        if (!CarrierCodeRegex().IsMatch(normalizedCode))
            throw new ArgumentException("Carrier code must contain only letters and numbers.", nameof(code));

        return new CarrierCode(normalizedCode);
    }

    /// <inheritdoc />
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Implicit conversion to string.
    /// </summary>
    public static implicit operator string(CarrierCode code) => code.Value;

    [GeneratedRegex("^[A-Z0-9]+$")]
    private static partial Regex CarrierCodeRegex();
}
