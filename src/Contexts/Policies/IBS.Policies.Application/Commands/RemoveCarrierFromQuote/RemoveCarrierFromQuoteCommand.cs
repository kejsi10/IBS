using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.RemoveCarrierFromQuote;

/// <summary>
/// Command to remove a carrier from a quote.
/// </summary>
public sealed record RemoveCarrierFromQuoteCommand(
    Guid TenantId,
    Guid QuoteId,
    Guid QuoteCarrierId,
    string? ExpectedRowVersion = null
) : ICommand;
