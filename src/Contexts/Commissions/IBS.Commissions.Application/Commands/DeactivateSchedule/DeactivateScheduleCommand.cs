using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Commissions.Application.Commands.DeactivateSchedule;

/// <summary>
/// Command to deactivate a commission schedule.
/// </summary>
public sealed record DeactivateScheduleCommand(
    Guid TenantId,
    Guid ScheduleId
) : ICommand;
