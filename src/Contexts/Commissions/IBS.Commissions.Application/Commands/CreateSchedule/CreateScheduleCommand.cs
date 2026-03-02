using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Commissions.Application.Commands.CreateSchedule;

/// <summary>
/// Command to create a new commission schedule.
/// </summary>
public sealed record CreateScheduleCommand(
    Guid TenantId,
    Guid CarrierId,
    string CarrierName,
    string LineOfBusiness,
    decimal NewBusinessRate,
    decimal RenewalRate,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo = null
) : ICommand<Guid>;
