using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Repositories;

namespace IBS.Commissions.Application.Commands.UpdateSchedule;

/// <summary>
/// Handler for the UpdateScheduleCommand.
/// </summary>
public sealed class UpdateScheduleCommandHandler(
    ICommissionScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateScheduleCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);
        if (schedule is null)
            return Error.NotFound("Commission schedule not found.");

        try
        {
            schedule.Update(
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

        await scheduleRepository.UpdateAsync(schedule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
