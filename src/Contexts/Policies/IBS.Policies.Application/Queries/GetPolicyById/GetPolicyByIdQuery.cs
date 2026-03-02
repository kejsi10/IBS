using IBS.BuildingBlocks.Application.Queries;
using IBS.BuildingBlocks.Domain;

namespace IBS.Policies.Application.Queries.GetPolicyById;

/// <summary>
/// Query to get a policy by its identifier.
/// </summary>
public sealed record GetPolicyByIdQuery(
    Guid TenantId,
    Guid PolicyId
) : IQuery<PolicyDto?>;

/// <summary>
/// DTO for policy data.
/// </summary>
public sealed record PolicyDto(
    Guid Id,
    string PolicyNumber,
    Guid ClientId,
    Guid CarrierId,
    string LineOfBusiness,
    string PolicyType,
    string Status,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    decimal TotalPremium,
    string Currency,
    string BillingType,
    string PaymentPlan,
    string? CarrierPolicyNumber,
    string? Notes,
    DateTimeOffset? BoundAt,
    DateOnly? CancellationDate,
    string? CancellationReason,
    IReadOnlyList<CoverageDto> Coverages,
    IReadOnlyList<EndorsementDto> Endorsements,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string RowVersion
) : IConcurrencyAware;

/// <summary>
/// DTO for coverage data.
/// </summary>
public sealed record CoverageDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    decimal? LimitAmount,
    decimal? DeductibleAmount,
    decimal PremiumAmount,
    bool IsOptional,
    bool IsActive
);

/// <summary>
/// DTO for endorsement data.
/// </summary>
public sealed record EndorsementDto(
    Guid Id,
    string EndorsementNumber,
    DateOnly EffectiveDate,
    string Type,
    string Description,
    decimal PremiumChange,
    string Status,
    DateTimeOffset? ProcessedAt,
    string? Notes
);
