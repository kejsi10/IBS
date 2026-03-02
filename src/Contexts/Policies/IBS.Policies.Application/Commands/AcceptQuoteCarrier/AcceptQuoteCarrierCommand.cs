using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.AcceptQuoteCarrier;

/// <summary>
/// Command to accept a carrier's quote, creating a policy draft.
/// </summary>
public sealed record AcceptQuoteCarrierCommand(
    Guid TenantId,
    Guid UserId,
    Guid QuoteId,
    Guid QuoteCarrierId,
    string? ExpectedRowVersion = null
) : ICommand<Guid>;
