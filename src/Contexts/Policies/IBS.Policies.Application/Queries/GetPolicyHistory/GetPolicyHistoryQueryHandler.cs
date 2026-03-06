using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Queries.GetPolicyHistory;

/// <summary>
/// Handler for GetPolicyHistoryQuery.
/// </summary>
public sealed class GetPolicyHistoryQueryHandler(
    IPolicyHistoryRepository historyRepository) : IQueryHandler<GetPolicyHistoryQuery, PolicyHistoryResult>
{
    /// <inheritdoc />
    public async Task<Result<PolicyHistoryResult>> Handle(GetPolicyHistoryQuery request, CancellationToken cancellationToken)
    {
        var page = await historyRepository.GetByPolicyIdAsync(
            request.TenantId,
            request.PolicyId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = page.Items.Select(h => new PolicyHistoryItemDto(
            h.Id,
            h.EventType,
            h.Description,
            h.ChangesJson,
            h.UserId,
            h.CreatedAt)).ToList();

        return new PolicyHistoryResult(
            items,
            page.TotalCount,
            page.PageNumber,
            page.PageSize,
            page.TotalPages);
    }
}
