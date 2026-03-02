using System.Text.RegularExpressions;
using IBS.BuildingBlocks.Domain;

namespace IBS.Policies.Domain.ValueObjects;

/// <summary>
/// Represents a unique policy number.
/// </summary>
public sealed partial class PolicyNumber : ValueObject
{
    private static readonly Regex PolicyNumberRegex = CreatePolicyNumberRegex();

    /// <summary>
    /// Gets the policy number value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private PolicyNumber()
    {
        Value = string.Empty;
    }

    private PolicyNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new policy number from an existing value.
    /// </summary>
    /// <param name="value">The policy number string.</param>
    /// <returns>A new PolicyNumber instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the policy number is invalid.</exception>
    public static PolicyNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Policy number is required.", nameof(value));

        var trimmed = value.Trim().ToUpperInvariant();

        if (trimmed.Length < 5 || trimmed.Length > 30)
            throw new ArgumentException("Policy number must be between 5 and 30 characters.", nameof(value));

        if (!PolicyNumberRegex.IsMatch(trimmed))
            throw new ArgumentException("Policy number can only contain letters, numbers, and hyphens.", nameof(value));

        return new PolicyNumber(trimmed);
    }

    /// <summary>
    /// Generates a new policy number with a prefix and timestamp.
    /// </summary>
    /// <param name="prefix">The prefix (e.g., carrier code, line of business).</param>
    /// <returns>A new PolicyNumber instance.</returns>
    public static PolicyNumber Generate(string prefix = "POL")
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        var value = $"{prefix.ToUpperInvariant()}-{timestamp}-{random}";
        return new PolicyNumber(value);
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
    public static implicit operator string(PolicyNumber policyNumber) => policyNumber.Value;

    [GeneratedRegex(@"^[A-Z0-9\-]+$")]
    private static partial Regex CreatePolicyNumberRegex();
}
