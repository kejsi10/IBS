using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Carriers.Application.Commands.DeactivateCarrier;

/// <summary>
/// Command to deactivate a carrier.
/// </summary>
/// <param name="CarrierId">The carrier identifier.</param>
/// <param name="Reason">The reason for deactivation (optional).</param>
public sealed record DeactivateCarrierCommand(
    Guid CarrierId,
    string? Reason = null,
    string? ExpectedRowVersion = null) : ICommand;
