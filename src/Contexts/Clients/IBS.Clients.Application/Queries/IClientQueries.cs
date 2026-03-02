namespace IBS.Clients.Application.Queries;

/// <summary>
/// Read-only query interface for Client data.
/// All queries use no-tracking for optimal performance.
/// </summary>
public interface IClientQueries
{
    /// <summary>
    /// Gets a client by identifier for display purposes.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The client details if found; otherwise, null.</returns>
    Task<ClientDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches clients by name or email.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of clients.</returns>
    Task<PagedResult<ClientListItemDto>> SearchAsync(
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets clients by email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of clients with the specified email.</returns>
    Task<IReadOnlyList<ClientListItemDto>> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already used by another client.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <param name="excludeClientId">Client ID to exclude from check (for updates).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the email is already in use.</returns>
    Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeClientId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Client list item DTO for search results.
/// </summary>
public sealed record ClientListItemDto
{
    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the client type.
    /// </summary>
    public string ClientType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the phone number.
    /// </summary>
    public string? Phone { get; init; }

    /// <summary>
    /// Gets the status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets the creation date.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Client details DTO with full information.
/// </summary>
public sealed record ClientDetailsDto
{
    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the client type.
    /// </summary>
    public string ClientType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the person name (for individual clients).
    /// </summary>
    public PersonNameDto? PersonName { get; init; }

    /// <summary>
    /// Gets the date of birth (for individual clients).
    /// </summary>
    public DateOnly? DateOfBirth { get; init; }

    /// <summary>
    /// Gets the business info (for business clients).
    /// </summary>
    public BusinessInfoDto? BusinessInfo { get; init; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the phone number.
    /// </summary>
    public string? Phone { get; init; }

    /// <summary>
    /// Gets the contacts.
    /// </summary>
    public IReadOnlyList<ContactDto> Contacts { get; init; } = [];

    /// <summary>
    /// Gets the addresses.
    /// </summary>
    public IReadOnlyList<AddressDto> Addresses { get; init; } = [];

    /// <summary>
    /// Gets the creation date.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the last update date.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Person name DTO.
/// </summary>
public sealed record PersonNameDto(string FirstName, string? MiddleName, string LastName, string? Suffix);

/// <summary>
/// Business info DTO.
/// </summary>
public sealed record BusinessInfoDto(
    string Name,
    string? DbaName,
    string BusinessType,
    string? Industry,
    int? YearEstablished,
    int? NumberOfEmployees,
    decimal? AnnualRevenue,
    string? Website);

/// <summary>
/// Contact DTO.
/// </summary>
public sealed record ContactDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Title,
    string? Email,
    string? Phone,
    bool IsPrimary);

/// <summary>
/// Address DTO.
/// </summary>
public sealed record AddressDto(
    Guid Id,
    string AddressType,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsPrimary);

/// <summary>
/// Paginated result.
/// </summary>
/// <typeparam name="T">The type of items.</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>
    /// Gets the items.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// Gets the current page.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total count.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the total pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Gets whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
