using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Clients.Application.Queries.SearchClients;

/// <summary>
/// Handler for the SearchClientsQuery.
/// </summary>
public sealed class SearchClientsQueryHandler(
    IClientQueries clientQueries) : IQueryHandler<SearchClientsQuery, PagedResult<ClientListItemDto>>
{
    /// <inheritdoc />
    public async Task<Result<PagedResult<ClientListItemDto>>> Handle(SearchClientsQuery request, CancellationToken cancellationToken)
    {
        var result = await clientQueries.SearchAsync(
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        return result;
    }
}
