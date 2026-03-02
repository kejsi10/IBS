namespace IBS.Identity.Application.Queries;

/// <summary>
/// Read-only query interface for User data.
/// All queries use no-tracking for optimal performance.
/// </summary>
public interface IUserQueries
{
    /// <summary>
    /// Gets a user by identifier for display purposes.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user details if found; otherwise, null.</returns>
    Task<UserDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches users within a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="searchTerm">Optional search term for name or email.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of users.</returns>
    Task<PagedResult<UserListItemDto>> SearchAsync(
        Guid tenantId,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already used within a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="email">The email to check.</param>
    /// <param name="excludeUserId">User ID to exclude from check (for updates).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the email is already in use.</returns>
    Task<bool> EmailExistsAsync(
        Guid tenantId,
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// User list item DTO for search results.
/// </summary>
public sealed record UserListItemDto
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Gets whether the user is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets the last login timestamp.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; init; }

    /// <summary>
    /// Gets the role names assigned to the user.
    /// </summary>
    public IReadOnlyList<string> Roles { get; init; } = [];
}

/// <summary>
/// User details DTO with full information.
/// </summary>
public sealed record UserDetailsDto
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Gets the title.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the phone number.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Gets whether the user is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets the last login timestamp.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; init; }

    /// <summary>
    /// Gets the roles assigned to the user.
    /// </summary>
    public IReadOnlyList<UserRoleDto> Roles { get; init; } = [];

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
/// User role DTO.
/// </summary>
public sealed record UserRoleDto(Guid Id, string Name);

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
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Gets whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
