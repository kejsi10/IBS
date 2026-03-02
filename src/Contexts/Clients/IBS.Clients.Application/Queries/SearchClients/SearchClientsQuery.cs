using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Clients.Application.Queries.SearchClients;

/// <summary>
/// Query to search clients.
/// </summary>
/// <param name="SearchTerm">The search term (optional).</param>
/// <param name="Page">The page number (1-based).</param>
/// <param name="PageSize">The page size.</param>
public sealed record SearchClientsQuery(
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<ClientListItemDto>>;
