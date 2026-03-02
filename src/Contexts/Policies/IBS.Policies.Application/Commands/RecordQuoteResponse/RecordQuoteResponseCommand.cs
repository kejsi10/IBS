using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.RecordQuoteResponse;

/// <summary>
/// Command to record a carrier's response to a quote request.
/// </summary>
public sealed record RecordQuoteResponseCommand(
    Guid TenantId,
    Guid QuoteId,
    Guid QuoteCarrierId,
    bool IsQuoted,
    decimal? PremiumAmount = null,
    string? PremiumCurrency = "USD",
    string? Conditions = null,
    string? ProposedCoverages = null,
    DateOnly? CarrierExpiresAt = null,
    string? DeclinationReason = null,
    string? ExpectedRowVersion = null
) : ICommand;
