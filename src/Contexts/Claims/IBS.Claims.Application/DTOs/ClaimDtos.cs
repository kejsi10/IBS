using IBS.BuildingBlocks.Domain;

namespace IBS.Claims.Application.DTOs;

/// <summary>
/// Full claim DTO with all details.
/// </summary>
public sealed record ClaimDto(
    Guid Id,
    string ClaimNumber,
    Guid PolicyId,
    Guid ClientId,
    string Status,
    DateTimeOffset LossDate,
    DateTimeOffset ReportedDate,
    string LossType,
    string LossDescription,
    decimal? LossAmount,
    string? LossAmountCurrency,
    decimal? ClaimAmount,
    string? ClaimAmountCurrency,
    string? AssignedAdjusterId,
    string? DenialReason,
    DateTimeOffset? ClosedAt,
    string? ClosureReason,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ClaimNoteDto> Notes,
    IReadOnlyList<ReserveDto> Reserves,
    IReadOnlyList<ClaimPaymentDto> Payments,
    string RowVersion
) : IConcurrencyAware;

/// <summary>
/// Claim list item DTO for list views.
/// </summary>
public sealed record ClaimListItemDto(
    Guid Id,
    string ClaimNumber,
    Guid PolicyId,
    Guid ClientId,
    string Status,
    DateTimeOffset LossDate,
    DateTimeOffset ReportedDate,
    string LossType,
    decimal? LossAmount,
    string? LossAmountCurrency,
    decimal? ClaimAmount,
    string? ClaimAmountCurrency,
    string? AssignedAdjusterId,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Claim note DTO.
/// </summary>
public sealed record ClaimNoteDto(
    Guid Id,
    string Content,
    string AuthorBy,
    bool IsInternal,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Reserve DTO.
/// </summary>
public sealed record ReserveDto(
    Guid Id,
    string ReserveType,
    decimal Amount,
    string Currency,
    string SetBy,
    DateTimeOffset SetAt,
    string? Notes
);

/// <summary>
/// Claim payment DTO.
/// </summary>
public sealed record ClaimPaymentDto(
    Guid Id,
    string PaymentType,
    decimal Amount,
    string Currency,
    string PayeeName,
    DateOnly PaymentDate,
    string? CheckNumber,
    string Status,
    string AuthorizedBy,
    DateTimeOffset AuthorizedAt,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? VoidedAt,
    string? VoidReason
);

/// <summary>
/// Claim statistics DTO for dashboard.
/// </summary>
public sealed record ClaimStatisticsDto(
    int TotalClaims,
    int OpenClaims,
    int ClosedClaims,
    int DeniedClaims,
    decimal TotalClaimAmount,
    decimal TotalPaidAmount,
    Dictionary<string, int> ClaimsByStatus,
    Dictionary<string, int> ClaimsByLossType
);
