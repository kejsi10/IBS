using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Queries.GetPolicyById;

/// <summary>
/// Handler for the GetPolicyByIdQuery.
/// </summary>
public sealed class GetPolicyByIdQueryHandler(
    IPolicyRepository policyRepository) : IQueryHandler<GetPolicyByIdQuery, PolicyDto?>
{
    /// <inheritdoc />
    public async Task<Result<PolicyDto?>> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
    {
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
        {
            return (PolicyDto?)null;
        }

        var dto = new PolicyDto(
            policy.Id,
            policy.PolicyNumber.Value,
            policy.ClientId,
            policy.CarrierId,
            policy.LineOfBusiness.GetDisplayName(),
            policy.PolicyType,
            policy.Status.GetDisplayName(),
            policy.EffectivePeriod.EffectiveDate,
            policy.EffectivePeriod.ExpirationDate,
            policy.TotalPremium.Amount,
            policy.TotalPremium.Currency,
            policy.BillingType.ToString(),
            policy.PaymentPlan.ToString(),
            policy.CarrierPolicyNumber,
            policy.Notes,
            policy.BoundAt,
            policy.CancellationDate,
            policy.CancellationReason,
            policy.Coverages.Select(c => new CoverageDto(
                c.Id,
                c.Code,
                c.Name,
                c.Description,
                c.LimitAmount?.Amount,
                c.DeductibleAmount?.Amount,
                c.PremiumAmount.Amount,
                c.IsOptional,
                c.IsActive
            )).ToList(),
            policy.Endorsements.Select(e => new EndorsementDto(
                e.Id,
                e.EndorsementNumber,
                e.EffectiveDate,
                e.Type,
                e.Description,
                e.PremiumChange.Amount,
                e.Status.ToString(),
                e.ProcessedAt,
                e.Notes
            )).ToList(),
            policy.CreatedAt,
            policy.UpdatedAt,
            Convert.ToBase64String(policy.RowVersion)
        );

        return dto;
    }
}
