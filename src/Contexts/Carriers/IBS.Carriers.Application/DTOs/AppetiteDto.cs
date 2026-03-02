using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.DTOs;

/// <summary>
/// Data transfer object for Appetite.
/// </summary>
public sealed record AppetiteDto
{
    /// <summary>
    /// Gets the appetite identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the carrier identifier.
    /// </summary>
    public Guid CarrierId { get; init; }

    /// <summary>
    /// Gets the line of business.
    /// </summary>
    public LineOfBusiness LineOfBusiness { get; init; }

    /// <summary>
    /// Gets the display name for the line of business.
    /// </summary>
    public string LineOfBusinessDisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the states covered.
    /// </summary>
    public string States { get; init; } = string.Empty;

    /// <summary>
    /// Gets the minimum years in business required.
    /// </summary>
    public int? MinYearsInBusiness { get; init; }

    /// <summary>
    /// Gets the maximum years in business.
    /// </summary>
    public int? MaxYearsInBusiness { get; init; }

    /// <summary>
    /// Gets the minimum annual revenue.
    /// </summary>
    public decimal? MinAnnualRevenue { get; init; }

    /// <summary>
    /// Gets the maximum annual revenue.
    /// </summary>
    public decimal? MaxAnnualRevenue { get; init; }

    /// <summary>
    /// Gets the minimum number of employees.
    /// </summary>
    public int? MinEmployees { get; init; }

    /// <summary>
    /// Gets the maximum number of employees.
    /// </summary>
    public int? MaxEmployees { get; init; }

    /// <summary>
    /// Gets the accepted industries.
    /// </summary>
    public string? AcceptedIndustries { get; init; }

    /// <summary>
    /// Gets the excluded industries.
    /// </summary>
    public string? ExcludedIndustries { get; init; }

    /// <summary>
    /// Gets additional notes.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Gets whether this appetite rule is active.
    /// </summary>
    public bool IsActive { get; init; }
}
