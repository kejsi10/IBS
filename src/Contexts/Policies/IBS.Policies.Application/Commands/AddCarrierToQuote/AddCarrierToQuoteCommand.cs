using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.AddCarrierToQuote;

/// <summary>
/// Command to add a carrier to a quote.
/// </summary>
public sealed record AddCarrierToQuoteCommand(
    Guid TenantId,
    Guid QuoteId,
    Guid CarrierId,
    string? ExpectedRowVersion = null
) : ICommand<Guid>;
