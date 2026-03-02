using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.DTOs;

/// <summary>
/// Data transfer object for Carrier.
/// </summary>
public sealed record CarrierDto : IConcurrencyAware
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
    /// Gets the legal name.
    /// </summary>
    public string? LegalName { get; init; }

    /// <summary>
    /// Gets the A.M. Best rating.
    /// </summary>
    public string? AmBestRating { get; init; }

    /// <summary>
    /// Gets the NAIC code.
    /// </summary>
    public string? NaicCode { get; init; }

    /// <summary>
    /// Gets the website URL.
    /// </summary>
    public string? WebsiteUrl { get; init; }

    /// <summary>
    /// Gets the API endpoint.
    /// </summary>
    public string? ApiEndpoint { get; init; }

    /// <summary>
    /// Gets the carrier status.
    /// </summary>
    public CarrierStatus Status { get; init; }

    /// <summary>
    /// Gets the notes.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Gets the products offered by this carrier.
    /// </summary>
    public IReadOnlyList<ProductDto> Products { get; init; } = [];

    /// <summary>
    /// Gets the appetite rules for this carrier.
    /// </summary>
    public IReadOnlyList<AppetiteDto> Appetites { get; init; } = [];

    /// <summary>
    /// Gets the created date.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the last updated date.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// Gets the Base64-encoded row version for concurrency control.
    /// </summary>
    public string RowVersion { get; init; } = string.Empty;
}
