using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IBS.Identity.Application.Commands.DeactivateUser;

/// <summary>
/// Handler for the DeactivateUserCommand.
/// </summary>
public sealed class DeactivateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeactivateUserCommandHandler> logger) : ICommandHandler<DeactivateUserCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        // Load user
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User", request.UserId);
        }

        // Deactivate (domain handles idempotency)
        user.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[Audit] User {UserId} deactivated", request.UserId);
        return Result.Success();
    }
}
