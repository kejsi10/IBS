using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.GetCurrentUser;

/// <summary>
/// Handler for the GetCurrentUserQuery.
/// </summary>
public sealed class GetCurrentUserQueryHandler(
    IUserQueries userQueries) : IQueryHandler<GetCurrentUserQuery, UserDetailsDto>
{
    /// <inheritdoc />
    public async Task<Result<UserDetailsDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userQueries.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Error.NotFound($"User {request.UserId} not found.");
        }

        return user;
    }
}
