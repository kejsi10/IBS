using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.SubmitQuote;

/// <summary>
/// Command to submit a quote to carriers.
/// </summary>
public sealed record SubmitQuoteCommand(
    Guid TenantId,
    Guid QuoteId,
    string? ExpectedRowVersion = null
) : ICommand;
