using IBS.BuildingBlocks.Domain;

namespace IBS.Clients.Domain.ValueObjects;

/// <summary>
/// Represents a person's name.
/// </summary>
public sealed class PersonName : ValueObject
{
    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; }

    /// <summary>
    /// Gets the middle name.
    /// </summary>
    public string? MiddleName { get; }

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; }

    /// <summary>
    /// Gets the name suffix (Jr., Sr., III, etc.).
    /// </summary>
    public string? Suffix { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private PersonName()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    private PersonName(string firstName, string? middleName, string lastName, string? suffix)
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Suffix = suffix;
    }

    /// <summary>
    /// Creates a new person name.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="middleName">The middle name (optional).</param>
    /// <param name="suffix">The suffix (optional).</param>
    /// <returns>A new PersonName instance.</returns>
    /// <exception cref="ArgumentException">Thrown when first or last name is empty.</exception>
    public static PersonName Create(string firstName, string lastName, string? middleName = null, string? suffix = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        return new PersonName(
            firstName.Trim(),
            middleName?.Trim(),
            lastName.Trim(),
            suffix?.Trim());
    }

    /// <summary>
    /// Gets the full name formatted as "FirstName MiddleName LastName Suffix".
    /// </summary>
    public string FullName
    {
        get
        {
            var parts = new List<string> { FirstName };
            if (!string.IsNullOrWhiteSpace(MiddleName))
                parts.Add(MiddleName);
            parts.Add(LastName);
            if (!string.IsNullOrWhiteSpace(Suffix))
                parts.Add(Suffix);
            return string.Join(" ", parts);
        }
    }

    /// <summary>
    /// Gets the formal name formatted as "LastName, FirstName MiddleName Suffix".
    /// </summary>
    public string FormalName
    {
        get
        {
            var parts = new List<string> { FirstName };
            if (!string.IsNullOrWhiteSpace(MiddleName))
                parts.Add(MiddleName);
            if (!string.IsNullOrWhiteSpace(Suffix))
                parts.Add(Suffix);
            return $"{LastName}, {string.Join(" ", parts)}";
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName.ToUpperInvariant();
        yield return MiddleName?.ToUpperInvariant();
        yield return LastName.ToUpperInvariant();
        yield return Suffix?.ToUpperInvariant();
    }
}
