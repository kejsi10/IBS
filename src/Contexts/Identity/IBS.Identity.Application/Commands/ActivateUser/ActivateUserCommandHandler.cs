using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IBS.Identity.Application.Commands.ActivateUser;

/// <summary>
/// Handler for the ActivateUserCommand.
/// </summary>
public sealed class ActivateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<ActivateUserCommandHandler> logger) : ICommandHandler<ActivateUserCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        // Load user
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User", request.UserId);
        }

        // Activate (domain handles idempotency)
        user.Activate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[Audit] User {UserId} activated", request.UserId);
        return Result.Success();
    }
}
