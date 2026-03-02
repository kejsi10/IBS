using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Claims.Application.Commands.VoidPayment;

/// <summary>
/// Command to void a payment.
/// </summary>
public sealed record VoidPaymentCommand(
    Guid TenantId,
    Guid UserId,
    Guid ClaimId,
    Guid PaymentId,
    string Reason,
    string? ExpectedRowVersion = null
) : ICommand;
