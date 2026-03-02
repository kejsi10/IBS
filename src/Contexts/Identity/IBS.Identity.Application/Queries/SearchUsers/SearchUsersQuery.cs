using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.SearchUsers;

/// <summary>
/// Query to search users within a tenant.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="SearchTerm">Optional search term for name or email.</param>
/// <param name="Page">The page number (1-based).</param>
/// <param name="PageSize">The page size.</param>
public sealed record SearchUsersQuery(
    Guid TenantId,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<UserListItemDto>>;
