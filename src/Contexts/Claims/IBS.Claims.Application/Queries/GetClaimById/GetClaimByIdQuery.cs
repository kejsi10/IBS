using IBS.BuildingBlocks.Application.Queries;
using IBS.Claims.Application.DTOs;

namespace IBS.Claims.Application.Queries.GetClaimById;

/// <summary>
/// Query to get a claim by its identifier.
/// </summary>
public sealed record GetClaimByIdQuery(
    Guid TenantId,
    Guid ClaimId
) : IQuery<ClaimDto>;
