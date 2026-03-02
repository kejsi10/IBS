using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.GetCurrentUser;

/// <summary>
/// Query to get the current user's details.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<UserDetailsDto>;
