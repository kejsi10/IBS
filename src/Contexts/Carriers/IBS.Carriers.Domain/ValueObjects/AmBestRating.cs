using IBS.BuildingBlocks.Domain;

namespace IBS.Carriers.Domain.ValueObjects;

/// <summary>
/// Represents an A.M. Best financial strength rating for an insurance carrier.
/// </summary>
public sealed class AmBestRating : ValueObject
{
    /// <summary>
    /// Valid A.M. Best ratings in order of strength (highest to lowest).
    /// </summary>
    private static readonly string[] ValidRatings =
    [
        "A++", "A+", "A", "A-",
        "B++", "B+", "B", "B-",
        "C++", "C+", "C", "C-",
        "D", "E", "F", "S",
        "NR" // Not Rated
    ];

    /// <summary>
    /// Gets the rating value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private AmBestRating() => Value = string.Empty;

    private AmBestRating(string value) => Value = value;

    /// <summary>
    /// Creates a new A.M. Best rating.
    /// </summary>
    /// <param name="rating">The rating value (e.g., "A+", "A", "B++").</param>
    /// <returns>A new AmBestRating instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the rating is invalid.</exception>
    public static AmBestRating Create(string rating)
    {
        if (string.IsNullOrWhiteSpace(rating))
            throw new ArgumentException("A.M. Best rating cannot be empty.", nameof(rating));

        var normalizedRating = rating.Trim().ToUpperInvariant();

        if (!ValidRatings.Contains(normalizedRating))
            throw new ArgumentException($"Invalid A.M. Best rating: {rating}. Valid ratings are: {string.Join(", ", ValidRatings)}", nameof(rating));

        return new AmBestRating(normalizedRating);
    }

    /// <summary>
    /// Creates a "Not Rated" rating.
    /// </summary>
    public static AmBestRating NotRated() => new("NR");

    /// <summary>
    /// Gets whether this is a secure rating (A- or better).
    /// </summary>
    public bool IsSecure => Value is "A++" or "A+" or "A" or "A-";

    /// <summary>
    /// Gets whether this carrier is rated.
    /// </summary>
    public bool IsRated => Value != "NR";

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
    public static implicit operator string(AmBestRating rating) => rating.Value;
}
