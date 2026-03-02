using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.DTOs;

/// <summary>
/// Summary data transfer object for Carrier (used in lists).
/// </summary>
public sealed record CarrierSummaryDto
{
    /// <summary>
    /// Gets the carrier identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the carrier name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the carrier code.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Gets the A.M. Best rating.
    /// </summary>
    public string? AmBestRating { get; init; }

    /// <summary>
    /// Gets the carrier status.
    /// </summary>
    public CarrierStatus Status { get; init; }

    /// <summary>
    /// Gets the number of active products.
    /// </summary>
    public int ActiveProductCount { get; init; }

    /// <summary>
    /// Gets the active lines of business.
    /// </summary>
    public IReadOnlyList<string> ActiveLinesOfBusiness { get; init; } = [];
}
