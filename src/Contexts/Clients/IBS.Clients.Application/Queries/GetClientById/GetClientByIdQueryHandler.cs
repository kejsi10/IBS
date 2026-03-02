using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Clients.Application.Queries.GetClientById;

/// <summary>
/// Handler for the GetClientByIdQuery.
/// </summary>
public sealed class GetClientByIdQueryHandler(
    IClientQueries clientQueries) : IQueryHandler<GetClientByIdQuery, ClientDetailsDto>
{
    /// <inheritdoc />
    public async Task<Result<ClientDetailsDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await clientQueries.GetByIdAsync(request.ClientId, cancellationToken);

        if (client is null)
        {
            return Error.NotFound($"Client {request.ClientId} not found.");
        }

        return client;
    }
}
