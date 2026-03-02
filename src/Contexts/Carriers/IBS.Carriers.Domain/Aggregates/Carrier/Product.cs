using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Domain.Aggregates.Carrier;

/// <summary>
/// Represents an insurance product offered by a carrier.
/// </summary>
public sealed class Product : Entity
{
    /// <summary>
    /// Gets the carrier identifier.
    /// </summary>
    public Guid CarrierId { get; private set; }

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the product code (carrier-specific identifier).
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the line of business.
    /// </summary>
    public LineOfBusiness LineOfBusiness { get; private set; }

    /// <summary>
    /// Gets the product description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets whether this product is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the minimum premium for this product.
    /// </summary>
    public decimal? MinimumPremium { get; private set; }

    /// <summary>
    /// Gets the effective date when this product became available.
    /// </summary>
    public DateOnly? EffectiveDate { get; private set; }

    /// <summary>
    /// Gets the expiration date when this product is no longer available.
    /// </summary>
    public DateOnly? ExpirationDate { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Product() { }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="carrierId">The carrier identifier.</param>
    /// <param name="name">The product name.</param>
    /// <param name="code">The product code.</param>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="description">The product description (optional).</param>
    /// <returns>A new Product instance.</returns>
    internal static Product Create(
        Guid carrierId,
        string name,
        string code,
        LineOfBusiness lineOfBusiness,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Product code cannot be empty.", nameof(code));

        return new Product
        {
            CarrierId = carrierId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            LineOfBusiness = lineOfBusiness,
            Description = description?.Trim(),
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the product information.
    /// </summary>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets the minimum premium for this product.
    /// </summary>
    /// <param name="minimumPremium">The minimum premium amount.</param>
    public void SetMinimumPremium(decimal? minimumPremium)
    {
        if (minimumPremium.HasValue && minimumPremium.Value < 0)
            throw new ArgumentException("Minimum premium cannot be negative.", nameof(minimumPremium));

        MinimumPremium = minimumPremium;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets the effective period for this product.
    /// </summary>
    /// <param name="effectiveDate">The effective date.</param>
    /// <param name="expirationDate">The expiration date (optional).</param>
    public void SetEffectivePeriod(DateOnly? effectiveDate, DateOnly? expirationDate)
    {
        if (effectiveDate.HasValue && expirationDate.HasValue && effectiveDate > expirationDate)
            throw new ArgumentException("Effective date must be before expiration date.");

        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the product.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the product.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Checks if the product is available on a given date.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>True if the product is available; otherwise, false.</returns>
    public bool IsAvailableOn(DateOnly date)
    {
        if (!IsActive) return false;
        if (EffectiveDate.HasValue && date < EffectiveDate.Value) return false;
        if (ExpirationDate.HasValue && date > ExpirationDate.Value) return false;
        return true;
    }
}
