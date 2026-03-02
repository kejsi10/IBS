using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.GetUserById;

/// <summary>
/// Handler for the GetUserByIdQuery.
/// </summary>
public sealed class GetUserByIdQueryHandler(
    IUserQueries userQueries) : IQueryHandler<GetUserByIdQuery, UserDetailsDto>
{
    /// <inheritdoc />
    public async Task<Result<UserDetailsDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userQueries.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("User", request.UserId);
        }

        return user;
    }
}
