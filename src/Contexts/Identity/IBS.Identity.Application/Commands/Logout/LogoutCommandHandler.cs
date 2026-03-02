using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Repositories;

namespace IBS.Identity.Application.Commands.Logout;

/// <summary>
/// Handler for the LogoutCommand.
/// </summary>
public sealed class LogoutCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<LogoutCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound($"User {request.UserId} not found.");
        }

        user.RevokeAllRefreshTokens();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
