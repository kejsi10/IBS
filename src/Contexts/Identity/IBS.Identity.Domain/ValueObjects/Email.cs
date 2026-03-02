using System.Text.RegularExpressions;
using IBS.BuildingBlocks.Domain;

namespace IBS.Identity.Domain.ValueObjects;

/// <summary>
/// Represents an email address.
/// </summary>
public sealed partial class Email : ValueObject
{
    private static readonly Regex EmailRegex = CreateEmailRegex();

    /// <summary>
    /// Gets the email address value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the normalized (lowercase) email address.
    /// </summary>
    public string NormalizedValue { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Email()
    {
        Value = string.Empty;
        NormalizedValue = string.Empty;
    }

    private Email(string value, string normalizedValue)
    {
        Value = value;
        NormalizedValue = normalizedValue;
    }

    /// <summary>
    /// Creates a new email address.
    /// </summary>
    /// <param name="email">The email address string.</param>
    /// <returns>A new Email instance.</returns>
    /// <exception cref="ArgumentException">Thrown when email is invalid.</exception>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email address is required.", nameof(email));

        var trimmedEmail = email.Trim();
        var normalizedEmail = trimmedEmail.ToUpperInvariant();

        if (!IsValidEmail(trimmedEmail))
            throw new ArgumentException("Email address is not valid.", nameof(email));

        return new Email(trimmedEmail, normalizedEmail);
    }

    /// <summary>
    /// Tries to create a new email address.
    /// </summary>
    /// <param name="email">The email address string.</param>
    /// <param name="result">The resulting Email if valid.</param>
    /// <returns>True if the email was valid and created; otherwise, false.</returns>
    public static bool TryCreate(string email, out Email? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(email))
            return false;

        var trimmedEmail = email.Trim();

        if (!IsValidEmail(trimmedEmail))
            return false;

        result = new Email(trimmedEmail, trimmedEmail.ToUpperInvariant());
        return true;
    }

    private static bool IsValidEmail(string email)
    {
        if (email.Length > 254)
            return false;

        return EmailRegex.IsMatch(email);
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return NormalizedValue;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Implicit conversion to string.
    /// </summary>
    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", RegexOptions.Compiled)]
    private static partial Regex CreateEmailRegex();
}
