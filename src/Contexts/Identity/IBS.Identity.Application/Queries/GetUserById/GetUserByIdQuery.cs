using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.GetUserById;

/// <summary>
/// Query to get a user by their identifier.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDetailsDto>;
