using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Claims.Application.Commands.IssuePayment;

/// <summary>
/// Command to issue an authorized payment.
/// </summary>
public sealed record IssuePaymentCommand(
    Guid TenantId,
    Guid UserId,
    Guid ClaimId,
    Guid PaymentId,
    string? ExpectedRowVersion = null
) : ICommand;
