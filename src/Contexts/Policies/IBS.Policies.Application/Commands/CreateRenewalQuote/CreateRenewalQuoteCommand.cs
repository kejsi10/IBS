using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.CreateRenewalQuote;

/// <summary>
/// Command to create a renewal quote linked to an existing policy.
/// </summary>
public sealed record CreateRenewalQuoteCommand(
    Guid TenantId,
    Guid UserId,
    Guid PolicyId
) : ICommand<Guid>;
