using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Domain.Aggregates.Client;

/// <summary>
/// Represents an address for a client.
/// </summary>
public sealed class Address : TenantEntity
{
    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the address type.
    /// </summary>
    public AddressType AddressType { get; private set; }

    /// <summary>
    /// Gets the first line of the street address.
    /// </summary>
    public string StreetLine1 { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the second line of the street address.
    /// </summary>
    public string? StreetLine2 { get; private set; }

    /// <summary>
    /// Gets the city.
    /// </summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the state/province.
    /// </summary>
    public string State { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the postal/zip code.
    /// </summary>
    public string PostalCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the country.
    /// </summary>
    public string Country { get; private set; } = "USA";

    /// <summary>
    /// Gets a value indicating whether this is the primary address for its type.
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Address() { }

    /// <summary>
    /// Creates a new address.
    /// </summary>
    internal static Address Create(
        Guid clientId,
        Guid tenantId,
        AddressType type,
        string streetLine1,
        string? streetLine2,
        string city,
        string state,
        string postalCode,
        string country,
        bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(streetLine1))
            throw new ArgumentException("Street address is required.", nameof(streetLine1));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State is required.", nameof(state));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code is required.", nameof(postalCode));

        return new Address
        {
            ClientId = clientId,
            TenantId = tenantId,
            AddressType = type,
            StreetLine1 = streetLine1.Trim(),
            StreetLine2 = streetLine2?.Trim(),
            City = city.Trim(),
            State = state.Trim(),
            PostalCode = postalCode.Trim(),
            Country = country.Trim(),
            IsPrimary = isPrimary
        };
    }

    /// <summary>
    /// Updates the address.
    /// </summary>
    public void Update(
        string streetLine1,
        string? streetLine2,
        string city,
        string state,
        string postalCode,
        string country)
    {
        if (string.IsNullOrWhiteSpace(streetLine1))
            throw new ArgumentException("Street address is required.", nameof(streetLine1));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State is required.", nameof(state));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code is required.", nameof(postalCode));

        StreetLine1 = streetLine1.Trim();
        StreetLine2 = streetLine2?.Trim();
        City = city.Trim();
        State = state.Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets whether this address is the primary address for its type.
    /// </summary>
    internal void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        MarkAsUpdated();
    }

    /// <summary>
    /// Gets the full address as a single string.
    /// </summary>
    public string GetFullAddress()
    {
        var parts = new List<string> { StreetLine1 };
        if (!string.IsNullOrWhiteSpace(StreetLine2))
            parts.Add(StreetLine2);
        parts.Add($"{City}, {State} {PostalCode}");
        if (Country != "USA")
            parts.Add(Country);
        return string.Join(", ", parts);
    }
}
