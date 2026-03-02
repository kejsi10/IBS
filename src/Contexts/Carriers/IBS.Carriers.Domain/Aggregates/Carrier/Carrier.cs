using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.Aggregates.Carrier.Events;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Domain.Aggregates.Carrier;

/// <summary>
/// Represents an insurance carrier (insurance company) in the system.
/// This is the aggregate root for the Carrier aggregate.
/// </summary>
public sealed class Carrier : AggregateRoot
{
    private readonly List<Product> _products = [];
    private readonly List<Appetite> _appetites = [];

    /// <summary>
    /// Gets the carrier name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique carrier code.
    /// </summary>
    public CarrierCode Code { get; private set; } = null!;

    /// <summary>
    /// Gets the carrier's legal name (for official documents).
    /// </summary>
    public string? LegalName { get; private set; }

    /// <summary>
    /// Gets the carrier's A.M. Best rating.
    /// </summary>
    public AmBestRating? AmBestRating { get; private set; }

    /// <summary>
    /// Gets the NAIC (National Association of Insurance Commissioners) code.
    /// </summary>
    public string? NaicCode { get; private set; }

    /// <summary>
    /// Gets the carrier's main website URL.
    /// </summary>
    public string? WebsiteUrl { get; private set; }

    /// <summary>
    /// Gets the carrier's API endpoint for integrations.
    /// </summary>
    public string? ApiEndpoint { get; private set; }

    /// <summary>
    /// Gets the carrier status.
    /// </summary>
    public CarrierStatus Status { get; private set; }

    /// <summary>
    /// Gets additional notes about the carrier.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets the products offered by this carrier.
    /// </summary>
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    /// <summary>
    /// Gets the appetite rules for this carrier.
    /// </summary>
    public IReadOnlyCollection<Appetite> Appetites => _appetites.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Carrier() { }

    /// <summary>
    /// Creates a new carrier.
    /// </summary>
    /// <param name="name">The carrier name.</param>
    /// <param name="code">The carrier code.</param>
    /// <param name="legalName">The legal name (optional).</param>
    /// <returns>A new Carrier instance.</returns>
    public static Carrier Create(string name, CarrierCode code, string? legalName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Carrier name cannot be empty.", nameof(name));

        var carrier = new Carrier
        {
            Name = name.Trim(),
            Code = code,
            LegalName = legalName?.Trim(),
            Status = CarrierStatus.Active
        };

        carrier.RaiseDomainEvent(new CarrierCreatedEvent(carrier.Id, carrier.Name, carrier.Code.Value));

        return carrier;
    }

    /// <summary>
    /// Updates the carrier's basic information.
    /// </summary>
    /// <param name="name">The new name.</param>
    /// <param name="legalName">The new legal name.</param>
    public void UpdateBasicInfo(string name, string? legalName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Carrier name cannot be empty.", nameof(name));

        Name = name.Trim();
        LegalName = legalName?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets the carrier's A.M. Best rating.
    /// </summary>
    /// <param name="rating">The A.M. Best rating.</param>
    public void SetAmBestRating(AmBestRating? rating)
    {
        AmBestRating = rating;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets the NAIC code.
    /// </summary>
    /// <param name="naicCode">The NAIC code.</param>
    public void SetNaicCode(string? naicCode)
    {
        if (naicCode is not null)
        {
            naicCode = naicCode.Trim();
            if (naicCode.Length > 0 && (naicCode.Length != 5 || !naicCode.All(char.IsDigit)))
                throw new ArgumentException("NAIC code must be a 5-digit number.", nameof(naicCode));
        }

        NaicCode = string.IsNullOrWhiteSpace(naicCode) ? null : naicCode;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets the carrier's website URL.
    /// </summary>
    /// <param name="websiteUrl">The website URL.</param>
    public void SetWebsiteUrl(string? websiteUrl)
    {
        if (websiteUrl is not null && !string.IsNullOrWhiteSpace(websiteUrl))
        {
            if (!Uri.TryCreate(websiteUrl.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Invalid website URL.", nameof(websiteUrl));
            }
            WebsiteUrl = uri.ToString();
        }
        else
        {
            WebsiteUrl = null;
        }
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets the carrier's API endpoint.
    /// </summary>
    /// <param name="apiEndpoint">The API endpoint URL.</param>
    public void SetApiEndpoint(string? apiEndpoint)
    {
        if (apiEndpoint is not null && !string.IsNullOrWhiteSpace(apiEndpoint))
        {
            if (!Uri.TryCreate(apiEndpoint.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Invalid API endpoint URL.", nameof(apiEndpoint));
            }
            ApiEndpoint = uri.ToString();
        }
        else
        {
            ApiEndpoint = null;
        }
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets notes for the carrier.
    /// </summary>
    /// <param name="notes">The notes.</param>
    public void SetNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the carrier.
    /// </summary>
    public void Activate()
    {
        if (Status == CarrierStatus.Active)
            return;

        Status = CarrierStatus.Active;
        MarkAsUpdated();
    }

    /// <summary>
    /// Suspends the carrier (temporarily not accepting new business).
    /// </summary>
    public void Suspend()
    {
        if (Status == CarrierStatus.Suspended)
            return;

        Status = CarrierStatus.Suspended;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the carrier.
    /// </summary>
    /// <param name="reason">The reason for deactivation.</param>
    public void Deactivate(string? reason = null)
    {
        if (Status == CarrierStatus.Inactive)
            return;

        Status = CarrierStatus.Inactive;
        MarkAsUpdated();

        RaiseDomainEvent(new CarrierDeactivatedEvent(Id, reason));
    }

    /// <summary>
    /// Adds a product to this carrier.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <param name="code">The product code.</param>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="description">The product description (optional).</param>
    /// <returns>The created product.</returns>
    public Product AddProduct(string name, string code, LineOfBusiness lineOfBusiness, string? description = null)
    {
        // Check for duplicate product code
        var normalizedCode = code.Trim().ToUpperInvariant();
        if (_products.Any(p => p.Code == normalizedCode))
            throw new InvalidOperationException($"A product with code '{normalizedCode}' already exists for this carrier.");

        var product = Product.Create(Id, name, code, lineOfBusiness, description);
        _products.Add(product);
        MarkAsUpdated();

        RaiseDomainEvent(new ProductAddedEvent(Id, product.Id, product.Name, product.Code, product.LineOfBusiness));

        return product;
    }

    /// <summary>
    /// Gets a product by its identifier.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <returns>The product if found; otherwise, null.</returns>
    public Product? GetProduct(Guid productId)
    {
        return _products.FirstOrDefault(p => p.Id == productId);
    }

    /// <summary>
    /// Gets a product by its code.
    /// </summary>
    /// <param name="code">The product code.</param>
    /// <returns>The product if found; otherwise, null.</returns>
    public Product? GetProductByCode(string code)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return _products.FirstOrDefault(p => p.Code == normalizedCode);
    }

    /// <summary>
    /// Gets active products for a specific line of business.
    /// </summary>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <returns>Active products for the specified line of business.</returns>
    public IEnumerable<Product> GetActiveProducts(LineOfBusiness lineOfBusiness)
    {
        return _products.Where(p => p.IsActive && p.LineOfBusiness == lineOfBusiness);
    }

    /// <summary>
    /// Removes a product from this carrier.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    public void RemoveProduct(Guid productId)
    {
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product is null)
            throw new InvalidOperationException($"Product with ID '{productId}' not found.");

        _products.Remove(product);
        MarkAsUpdated();
    }

    /// <summary>
    /// Adds an appetite rule to this carrier.
    /// </summary>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="states">The states covered (comma-separated, or "ALL").</param>
    /// <returns>The created appetite.</returns>
    public Appetite AddAppetite(LineOfBusiness lineOfBusiness, string states = "ALL")
    {
        // Check for duplicate appetite for same line of business
        if (_appetites.Any(a => a.LineOfBusiness == lineOfBusiness && a.IsActive))
            throw new InvalidOperationException($"An active appetite rule for {lineOfBusiness.GetDisplayName()} already exists for this carrier.");

        var appetite = Appetite.Create(Id, lineOfBusiness, states);
        _appetites.Add(appetite);
        MarkAsUpdated();

        RaiseDomainEvent(new AppetiteAddedEvent(Id, appetite.Id, appetite.LineOfBusiness, appetite.States));

        return appetite;
    }

    /// <summary>
    /// Gets an appetite rule by its identifier.
    /// </summary>
    /// <param name="appetiteId">The appetite identifier.</param>
    /// <returns>The appetite if found; otherwise, null.</returns>
    public Appetite? GetAppetite(Guid appetiteId)
    {
        return _appetites.FirstOrDefault(a => a.Id == appetiteId);
    }

    /// <summary>
    /// Gets the appetite rule for a specific line of business.
    /// </summary>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <returns>The appetite if found; otherwise, null.</returns>
    public Appetite? GetAppetiteForLineOfBusiness(LineOfBusiness lineOfBusiness)
    {
        return _appetites.FirstOrDefault(a => a.LineOfBusiness == lineOfBusiness && a.IsActive);
    }

    /// <summary>
    /// Checks if this carrier covers a specific state for a line of business.
    /// </summary>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="state">The state code (e.g., "CA", "TX").</param>
    /// <returns>True if the carrier covers the state; otherwise, false.</returns>
    public bool CoversState(LineOfBusiness lineOfBusiness, string state)
    {
        if (Status != CarrierStatus.Active)
            return false;

        var appetite = GetAppetiteForLineOfBusiness(lineOfBusiness);
        return appetite?.CoversState(state) ?? false;
    }

    /// <summary>
    /// Removes an appetite rule from this carrier.
    /// </summary>
    /// <param name="appetiteId">The appetite identifier.</param>
    public void RemoveAppetite(Guid appetiteId)
    {
        var appetite = _appetites.FirstOrDefault(a => a.Id == appetiteId);
        if (appetite is null)
            throw new InvalidOperationException($"Appetite with ID '{appetiteId}' not found.");

        _appetites.Remove(appetite);
        MarkAsUpdated();
    }

    /// <summary>
    /// Gets all active lines of business for this carrier.
    /// </summary>
    /// <returns>Active lines of business.</returns>
    public IEnumerable<LineOfBusiness> GetActiveLinesOfBusiness()
    {
        return _appetites
            .Where(a => a.IsActive)
            .Select(a => a.LineOfBusiness)
            .Distinct();
    }
}
