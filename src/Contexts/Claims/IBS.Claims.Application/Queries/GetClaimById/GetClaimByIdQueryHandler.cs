using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Claims.Application.DTOs;
using IBS.Claims.Domain.Queries;

namespace IBS.Claims.Application.Queries.GetClaimById;

/// <summary>
/// Handler for the GetClaimByIdQuery.
/// </summary>
public sealed class GetClaimByIdQueryHandler(
    IClaimQueries claimQueries) : IQueryHandler<GetClaimByIdQuery, ClaimDto>
{
    /// <inheritdoc />
    public async Task<Result<ClaimDto>> Handle(GetClaimByIdQuery request, CancellationToken cancellationToken)
    {
        var claim = await claimQueries.GetByIdAsync(request.ClaimId, cancellationToken);
        if (claim is null)
            return Error.NotFound("Claim", request.ClaimId);

        return new ClaimDto(
            claim.Id,
            claim.ClaimNumber,
            claim.PolicyId,
            claim.ClientId,
            claim.Status,
            claim.LossDate,
            claim.ReportedDate,
            claim.LossType,
            claim.LossDescription,
            claim.LossAmount,
            claim.LossAmountCurrency,
            claim.ClaimAmount,
            claim.ClaimAmountCurrency,
            claim.AssignedAdjusterId,
            claim.DenialReason,
            claim.ClosedAt,
            claim.ClosureReason,
            claim.CreatedBy,
            claim.CreatedAt,
            claim.UpdatedAt,
            claim.Notes.Select(n => new ClaimNoteDto(n.Id, n.Content, n.AuthorBy, n.IsInternal, n.CreatedAt)).ToList(),
            claim.Reserves.Select(r => new ReserveDto(r.Id, r.ReserveType, r.Amount, r.Currency, r.SetBy, r.SetAt, r.Notes)).ToList(),
            claim.Payments.Select(p => new ClaimPaymentDto(
                p.Id, p.PaymentType, p.Amount, p.Currency, p.PayeeName, p.PaymentDate,
                p.CheckNumber, p.Status, p.AuthorizedBy, p.AuthorizedAt, p.IssuedAt, p.VoidedAt, p.VoidReason)).ToList(),
            claim.RowVersion
        );
    }
}
