using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.SearchUsers;

/// <summary>
/// Handler for the SearchUsersQuery.
/// </summary>
public sealed class SearchUsersQueryHandler(
    IUserQueries userQueries) : IQueryHandler<SearchUsersQuery, PagedResult<UserListItemDto>>
{
    /// <inheritdoc />
    public async Task<Result<PagedResult<UserListItemDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var result = await userQueries.SearchAsync(
            request.TenantId,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        return result;
    }
}
