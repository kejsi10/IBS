using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Clients.Application.Queries.GetClientById;

/// <summary>
/// Query to get a client by identifier.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
public sealed record GetClientByIdQuery(Guid ClientId) : IQuery<ClientDetailsDto>;
