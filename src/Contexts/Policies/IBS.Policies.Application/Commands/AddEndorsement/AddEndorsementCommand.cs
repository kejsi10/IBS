using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.AddEndorsement;

/// <summary>
/// Command to add an endorsement to a policy.
/// </summary>
public sealed record AddEndorsementCommand(
    Guid TenantId,
    Guid PolicyId,
    DateOnly EffectiveDate,
    string Type,
    string Description,
    decimal PremiumChange,
    string? Notes = null,
    string? ExpectedRowVersion = null
) : ICommand<Guid>;
