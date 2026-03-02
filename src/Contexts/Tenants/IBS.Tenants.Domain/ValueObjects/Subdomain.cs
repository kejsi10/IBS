using System.Text.RegularExpressions;
using IBS.BuildingBlocks.Domain;

namespace IBS.Tenants.Domain.ValueObjects;

/// <summary>
/// Value object representing a tenant subdomain.
/// </summary>
public sealed partial class Subdomain : SingleValueObject<string>
{
    private static readonly Regex SubdomainPattern = GenerateSubdomainRegex();

    /// <summary>
    /// Gets the minimum length for a subdomain.
    /// </summary>
    public const int MinLength = 3;

    /// <summary>
    /// Gets the maximum length for a subdomain.
    /// </summary>
    public const int MaxLength = 63;

    /// <summary>
    /// Reserved subdomains that cannot be used.
    /// </summary>
    private static readonly HashSet<string> ReservedSubdomains =
    [
        "www", "api", "app", "admin", "mail", "email", "ftp", "smtp", "imap",
        "pop", "ns", "ns1", "ns2", "dns", "dev", "staging", "test", "demo",
        "support", "help", "docs", "blog", "status", "cdn", "static", "assets"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="Subdomain"/> class.
    /// </summary>
    /// <param name="value">The subdomain value.</param>
    private Subdomain(string value) : base(value.ToLowerInvariant())
    {
    }

    /// <summary>
    /// Creates a new subdomain.
    /// </summary>
    /// <param name="value">The subdomain value.</param>
    /// <returns>A new subdomain instance.</returns>
    public static Subdomain Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Subdomain cannot be empty.", nameof(value));

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (normalizedValue.Length < MinLength)
            throw new ArgumentException($"Subdomain must be at least {MinLength} characters.", nameof(value));

        if (normalizedValue.Length > MaxLength)
            throw new ArgumentException($"Subdomain cannot exceed {MaxLength} characters.", nameof(value));

        if (!SubdomainPattern.IsMatch(normalizedValue))
            throw new ArgumentException(
                "Subdomain can only contain lowercase letters, numbers, and hyphens. " +
                "It must start and end with a letter or number.", nameof(value));

        if (ReservedSubdomains.Contains(normalizedValue))
            throw new ArgumentException($"Subdomain '{normalizedValue}' is reserved and cannot be used.", nameof(value));

        return new Subdomain(normalizedValue);
    }

    /// <summary>
    /// Checks if a subdomain value is valid.
    /// </summary>
    /// <param name="value">The subdomain to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (normalizedValue.Length < MinLength || normalizedValue.Length > MaxLength)
            return false;

        if (!SubdomainPattern.IsMatch(normalizedValue))
            return false;

        if (ReservedSubdomains.Contains(normalizedValue))
            return false;

        return true;
    }

    [GeneratedRegex(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$", RegexOptions.Compiled)]
    private static partial Regex GenerateSubdomainRegex();
}
