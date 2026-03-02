using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Commissions.Application.Commands.UpdateSchedule;

/// <summary>
/// Command to update an existing commission schedule.
/// </summary>
public sealed record UpdateScheduleCommand(
    Guid TenantId,
    Guid ScheduleId,
    string CarrierName,
    string LineOfBusiness,
    decimal NewBusinessRate,
    decimal RenewalRate,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo = null
) : ICommand;
