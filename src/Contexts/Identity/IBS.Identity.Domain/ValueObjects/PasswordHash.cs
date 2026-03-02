using IBS.BuildingBlocks.Domain;

namespace IBS.Identity.Domain.ValueObjects;

/// <summary>
/// Represents a hashed password.
/// </summary>
public sealed class PasswordHash : ValueObject
{
    /// <summary>
    /// Gets the hashed password value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private PasswordHash()
    {
        Value = string.Empty;
    }

    private PasswordHash(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new password hash from an already hashed value.
    /// </summary>
    /// <param name="hashedPassword">The hashed password.</param>
    /// <returns>A new PasswordHash instance.</returns>
    /// <exception cref="ArgumentException">Thrown when hash is empty.</exception>
    public static PasswordHash FromHash(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Password hash is required.", nameof(hashedPassword));

        return new PasswordHash(hashedPassword);
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => "***";

    /// <summary>
    /// Implicit conversion to string.
    /// </summary>
    public static implicit operator string(PasswordHash hash) => hash.Value;
}
