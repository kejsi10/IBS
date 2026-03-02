using System.Text.RegularExpressions;
using IBS.BuildingBlocks.Domain;

namespace IBS.Clients.Domain.ValueObjects;

/// <summary>
/// Represents a phone number.
/// </summary>
public sealed partial class PhoneNumber : ValueObject
{
    private static readonly Regex DigitsOnlyRegex = CreateDigitsOnlyRegex();

    /// <summary>
    /// Gets the phone number value (digits only, normalized).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the phone number type.
    /// </summary>
    public PhoneNumberType Type { get; }

    /// <summary>
    /// Gets the extension (if any).
    /// </summary>
    public string? Extension { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private PhoneNumber()
    {
        Value = string.Empty;
        Type = PhoneNumberType.Mobile;
    }

    private PhoneNumber(string value, PhoneNumberType type, string? extension)
    {
        Value = value;
        Type = type;
        Extension = extension;
    }

    /// <summary>
    /// Creates a new phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number string.</param>
    /// <param name="type">The phone number type.</param>
    /// <param name="extension">The extension (optional).</param>
    /// <returns>A new PhoneNumber instance.</returns>
    /// <exception cref="ArgumentException">Thrown when phone number is invalid.</exception>
    public static PhoneNumber Create(string phoneNumber, PhoneNumberType type = PhoneNumberType.Mobile, string? extension = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        var digitsOnly = DigitsOnlyRegex.Replace(phoneNumber, "");

        if (digitsOnly.Length < 10 || digitsOnly.Length > 15)
            throw new ArgumentException("Phone number must be between 10 and 15 digits.", nameof(phoneNumber));

        return new PhoneNumber(digitsOnly, type, extension?.Trim());
    }

    /// <summary>
    /// Tries to create a new phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number string.</param>
    /// <param name="type">The phone number type.</param>
    /// <param name="result">The resulting PhoneNumber if valid.</param>
    /// <returns>True if the phone number was valid and created; otherwise, false.</returns>
    public static bool TryCreate(string phoneNumber, PhoneNumberType type, out PhoneNumber? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var digitsOnly = DigitsOnlyRegex.Replace(phoneNumber, "");

        if (digitsOnly.Length < 10 || digitsOnly.Length > 15)
            return false;

        result = new PhoneNumber(digitsOnly, type, null);
        return true;
    }

    /// <summary>
    /// Formats the phone number for display (US format).
    /// </summary>
    /// <returns>Formatted phone number string.</returns>
    public string Format()
    {
        if (Value.Length == 10)
        {
            var formatted = $"({Value[..3]}) {Value[3..6]}-{Value[6..]}";
            return Extension != null ? $"{formatted} x{Extension}" : formatted;
        }

        if (Value.Length == 11 && Value[0] == '1')
        {
            var formatted = $"+1 ({Value[1..4]}) {Value[4..7]}-{Value[7..]}";
            return Extension != null ? $"{formatted} x{Extension}" : formatted;
        }

        return Extension != null ? $"+{Value} x{Extension}" : $"+{Value}";
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
        yield return Extension;
    }

    /// <inheritdoc />
    public override string ToString() => Format();

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex CreateDigitsOnlyRegex();
}

/// <summary>
/// Represents the type of phone number.
/// </summary>
public enum PhoneNumberType
{
    /// <summary>
    /// Mobile/cell phone.
    /// </summary>
    Mobile = 1,

    /// <summary>
    /// Home phone.
    /// </summary>
    Home = 2,

    /// <summary>
    /// Work phone.
    /// </summary>
    Work = 3,

    /// <summary>
    /// Fax number.
    /// </summary>
    Fax = 4,

    /// <summary>
    /// Other phone type.
    /// </summary>
    Other = 99
}
