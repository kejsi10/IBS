using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Repositories;

namespace IBS.Commissions.Application.Commands.DeactivateSchedule;

/// <summary>
/// Handler for the DeactivateScheduleCommand.
/// </summary>
public sealed class DeactivateScheduleCommandHandler(
    ICommissionScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeactivateScheduleCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DeactivateScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);
        if (schedule is null)
            return Error.NotFound("Commission schedule not found.");

        try
        {
            schedule.Deactivate();
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
