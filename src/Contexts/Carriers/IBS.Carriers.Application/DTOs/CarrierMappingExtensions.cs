using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.DTOs;

/// <summary>
/// Extension methods for mapping Carrier domain entities to DTOs.
/// </summary>
public static class CarrierMappingExtensions
{
    /// <summary>
    /// Maps a Carrier aggregate to a CarrierDto.
    /// </summary>
    /// <param name="carrier">The carrier.</param>
    /// <returns>The CarrierDto.</returns>
    public static CarrierDto ToDto(this Carrier carrier)
    {
        return new CarrierDto
        {
            Id = carrier.Id,
            Name = carrier.Name,
            Code = carrier.Code.Value,
            LegalName = carrier.LegalName,
            AmBestRating = carrier.AmBestRating?.Value,
            NaicCode = carrier.NaicCode,
            WebsiteUrl = carrier.WebsiteUrl,
            ApiEndpoint = carrier.ApiEndpoint,
            Status = carrier.Status,
            Notes = carrier.Notes,
            Products = carrier.Products.Select(p => p.ToDto()).ToList(),
            Appetites = carrier.Appetites.Select(a => a.ToDto()).ToList(),
            CreatedAt = carrier.CreatedAt,
            UpdatedAt = carrier.UpdatedAt,
            RowVersion = Convert.ToBase64String(carrier.RowVersion)
        };
    }

    /// <summary>
    /// Maps a Carrier aggregate to a CarrierSummaryDto.
    /// </summary>
    /// <param name="carrier">The carrier.</param>
    /// <returns>The CarrierSummaryDto.</returns>
    public static CarrierSummaryDto ToSummaryDto(this Carrier carrier)
    {
        return new CarrierSummaryDto
        {
            Id = carrier.Id,
            Name = carrier.Name,
            Code = carrier.Code.Value,
            AmBestRating = carrier.AmBestRating?.Value,
            Status = carrier.Status,
            ActiveProductCount = carrier.Products.Count(p => p.IsActive),
            ActiveLinesOfBusiness = carrier.GetActiveLinesOfBusiness()
                .Select(lob => lob.GetDisplayName())
                .ToList()
        };
    }

    /// <summary>
    /// Maps a Product entity to a ProductDto.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <returns>The ProductDto.</returns>
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            CarrierId = product.CarrierId,
            Name = product.Name,
            Code = product.Code,
            LineOfBusiness = product.LineOfBusiness,
            LineOfBusinessDisplayName = product.LineOfBusiness.GetDisplayName(),
            Description = product.Description,
            IsActive = product.IsActive,
            MinimumPremium = product.MinimumPremium,
            EffectiveDate = product.EffectiveDate,
            ExpirationDate = product.ExpirationDate
        };
    }

    /// <summary>
    /// Maps an Appetite entity to an AppetiteDto.
    /// </summary>
    /// <param name="appetite">The appetite.</param>
    /// <returns>The AppetiteDto.</returns>
    public static AppetiteDto ToDto(this Appetite appetite)
    {
        return new AppetiteDto
        {
            Id = appetite.Id,
            CarrierId = appetite.CarrierId,
            LineOfBusiness = appetite.LineOfBusiness,
            LineOfBusinessDisplayName = appetite.LineOfBusiness.GetDisplayName(),
            States = appetite.States,
            MinYearsInBusiness = appetite.MinYearsInBusiness,
            MaxYearsInBusiness = appetite.MaxYearsInBusiness,
            MinAnnualRevenue = appetite.MinAnnualRevenue,
            MaxAnnualRevenue = appetite.MaxAnnualRevenue,
            MinEmployees = appetite.MinEmployees,
            MaxEmployees = appetite.MaxEmployees,
            AcceptedIndustries = appetite.AcceptedIndustries,
            ExcludedIndustries = appetite.ExcludedIndustries,
            Notes = appetite.Notes,
            IsActive = appetite.IsActive
        };
    }
}
