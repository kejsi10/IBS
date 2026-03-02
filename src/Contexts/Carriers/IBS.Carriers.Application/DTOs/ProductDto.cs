using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.DTOs;

/// <summary>
/// Data transfer object for Product.
/// </summary>
public sealed record ProductDto
{
    /// <summary>
    /// Gets the product identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the carrier identifier.
    /// </summary>
    public Guid CarrierId { get; init; }

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the product code.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Gets the line of business.
    /// </summary>
    public LineOfBusiness LineOfBusiness { get; init; }

    /// <summary>
    /// Gets the display name for the line of business.
    /// </summary>
    public string LineOfBusinessDisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the product description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets whether the product is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets the minimum premium.
    /// </summary>
    public decimal? MinimumPremium { get; init; }

    /// <summary>
    /// Gets the effective date.
    /// </summary>
    public DateOnly? EffectiveDate { get; init; }

    /// <summary>
    /// Gets the expiration date.
    /// </summary>
    public DateOnly? ExpirationDate { get; init; }
}
