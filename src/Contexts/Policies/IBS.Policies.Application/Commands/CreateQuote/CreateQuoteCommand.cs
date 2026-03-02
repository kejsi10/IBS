using IBS.BuildingBlocks.Application.Commands;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Policies.Application.Commands.CreateQuote;

/// <summary>
/// Command to create a new quote.
/// </summary>
public sealed record CreateQuoteCommand(
    Guid TenantId,
    Guid UserId,
    Guid ClientId,
    LineOfBusiness LineOfBusiness,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    string? Notes = null,
    DateOnly? QuoteExpiresAt = null
) : ICommand<Guid>;
