using System.Text.RegularExpressions;
using IBS.BuildingBlocks.Domain;

namespace IBS.Clients.Domain.ValueObjects;

/// <summary>
/// Represents an email address.
/// </summary>
public sealed partial class EmailAddress : ValueObject
{
    private static readonly Regex EmailRegex = CreateEmailRegex();

    /// <summary>
    /// Gets the email address value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private EmailAddress()
    {
        Value = string.Empty;
    }

    private EmailAddress(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new email address.
    /// </summary>
    /// <param name="email">The email address string.</param>
    /// <returns>A new EmailAddress instance.</returns>
    /// <exception cref="ArgumentException">Thrown when email is invalid.</exception>
    public static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email address is required.", nameof(email));

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!IsValidEmail(normalizedEmail))
            throw new ArgumentException("Email address is not valid.", nameof(email));

        return new EmailAddress(normalizedEmail);
    }

    /// <summary>
    /// Tries to create a new email address.
    /// </summary>
    /// <param name="email">The email address string.</param>
    /// <param name="result">The resulting EmailAddress if valid.</param>
    /// <returns>True if the email was valid and created; otherwise, false.</returns>
    public static bool TryCreate(string email, out EmailAddress? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!IsValidEmail(normalizedEmail))
            return false;

        result = new EmailAddress(normalizedEmail);
        return true;
    }

    /// <summary>
    /// Gets the domain portion of the email address.
    /// </summary>
    public string Domain => Value.Split('@')[1];

    /// <summary>
    /// Gets the local portion of the email address (before @).
    /// </summary>
    public string LocalPart => Value.Split('@')[0];

    private static bool IsValidEmail(string email)
    {
        if (email.Length > 254)
            return false;

        return EmailRegex.IsMatch(email);
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
    public static implicit operator string(EmailAddress email) => email.Value;

    [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", RegexOptions.Compiled)]
    private static partial Regex CreateEmailRegex();
}
