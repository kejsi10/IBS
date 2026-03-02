using IBS.BuildingBlocks.Domain;

namespace IBS.Clients.Domain.ValueObjects;

/// <summary>
/// Represents business information for a business client.
/// </summary>
public sealed class BusinessInfo : ValueObject
{
    /// <summary>
    /// Gets the legal business name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the DBA (Doing Business As) name.
    /// </summary>
    public string? DbaName { get; }

    /// <summary>
    /// Gets the business type (LLC, Corporation, Partnership, etc.).
    /// </summary>
    public string BusinessType { get; }

    /// <summary>
    /// Gets the industry classification.
    /// </summary>
    public string? Industry { get; }

    /// <summary>
    /// Gets the year the business was established.
    /// </summary>
    public int? YearEstablished { get; }

    /// <summary>
    /// Gets the number of employees.
    /// </summary>
    public int? NumberOfEmployees { get; }

    /// <summary>
    /// Gets the annual revenue.
    /// </summary>
    public decimal? AnnualRevenue { get; }

    /// <summary>
    /// Gets the website URL.
    /// </summary>
    public string? Website { get; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private BusinessInfo()
    {
        Name = string.Empty;
        BusinessType = string.Empty;
    }

    private BusinessInfo(
        string name,
        string? dbaName,
        string businessType,
        string? industry,
        int? yearEstablished,
        int? numberOfEmployees,
        decimal? annualRevenue,
        string? website)
    {
        Name = name;
        DbaName = dbaName;
        BusinessType = businessType;
        Industry = industry;
        YearEstablished = yearEstablished;
        NumberOfEmployees = numberOfEmployees;
        AnnualRevenue = annualRevenue;
        Website = website;
    }

    /// <summary>
    /// Creates a new business info.
    /// </summary>
    /// <param name="name">The legal business name.</param>
    /// <param name="businessType">The business type.</param>
    /// <param name="dbaName">The DBA name (optional).</param>
    /// <param name="industry">The industry classification (optional).</param>
    /// <param name="yearEstablished">The year established (optional).</param>
    /// <param name="numberOfEmployees">The number of employees (optional).</param>
    /// <param name="annualRevenue">The annual revenue (optional).</param>
    /// <param name="website">The website URL (optional).</param>
    /// <returns>A new BusinessInfo instance.</returns>
    /// <exception cref="ArgumentException">Thrown when name or business type is empty.</exception>
    public static BusinessInfo Create(
        string name,
        string businessType,
        string? dbaName = null,
        string? industry = null,
        int? yearEstablished = null,
        int? numberOfEmployees = null,
        decimal? annualRevenue = null,
        string? website = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Business name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(businessType))
            throw new ArgumentException("Business type is required.", nameof(businessType));
        if (yearEstablished.HasValue && (yearEstablished < 1800 || yearEstablished > DateTime.UtcNow.Year))
            throw new ArgumentException("Year established must be between 1800 and current year.", nameof(yearEstablished));
        if (numberOfEmployees.HasValue && numberOfEmployees < 0)
            throw new ArgumentException("Number of employees cannot be negative.", nameof(numberOfEmployees));
        if (annualRevenue.HasValue && annualRevenue < 0)
            throw new ArgumentException("Annual revenue cannot be negative.", nameof(annualRevenue));

        return new BusinessInfo(
            name.Trim(),
            dbaName?.Trim(),
            businessType.Trim(),
            industry?.Trim(),
            yearEstablished,
            numberOfEmployees,
            annualRevenue,
            website?.Trim());
    }

    /// <summary>
    /// Gets the display name (DBA name if available, otherwise legal name).
    /// </summary>
    public string DisplayName => DbaName ?? Name;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name.ToUpperInvariant();
        yield return DbaName?.ToUpperInvariant();
        yield return BusinessType.ToUpperInvariant();
        yield return Industry?.ToUpperInvariant();
        yield return YearEstablished;
        yield return NumberOfEmployees;
        yield return AnnualRevenue;
        yield return Website?.ToUpperInvariant();
    }
}
