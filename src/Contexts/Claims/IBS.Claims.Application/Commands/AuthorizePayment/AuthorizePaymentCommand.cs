using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Claims.Application.Commands.AuthorizePayment;

/// <summary>
/// Command to authorize a payment on a claim.
/// </summary>
public sealed record AuthorizePaymentCommand(
    Guid TenantId,
    Guid UserId,
    Guid ClaimId,
    string PaymentType,
    decimal Amount,
    string Currency,
    string PayeeName,
    DateOnly PaymentDate,
    string? CheckNumber = null,
    string? ExpectedRowVersion = null
) : ICommand<Guid>;
