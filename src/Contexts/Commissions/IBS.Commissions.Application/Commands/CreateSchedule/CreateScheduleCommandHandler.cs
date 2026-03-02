using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Aggregates.CommissionSchedule;
using IBS.Commissions.Domain.Repositories;

namespace IBS.Commissions.Application.Commands.CreateSchedule;

/// <summary>
/// Handler for the CreateScheduleCommand.
/// </summary>
public sealed class CreateScheduleCommandHandler(
    ICommissionScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateScheduleCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        CommissionSchedule schedule;
        try
        {
            schedule = CommissionSchedule.Create(
                request.TenantId,
                request.CarrierId,
                request.CarrierName,
                request.LineOfBusiness,
                request.NewBusinessRate,
                request.RenewalRate,
                request.EffectiveFrom,
                request.EffectiveTo);
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }

        await scheduleRepository.AddAsync(schedule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return schedule.Id;
    }
}
