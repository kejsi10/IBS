using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.CancelQuote;

/// <summary>
/// Command to cancel a quote.
/// </summary>
public sealed record CancelQuoteCommand(
    Guid TenantId,
    Guid QuoteId,
    string? ExpectedRowVersion = null
) : ICommand;
