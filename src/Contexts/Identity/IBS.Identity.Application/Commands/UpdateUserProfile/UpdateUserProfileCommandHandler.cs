using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Repositories;

namespace IBS.Identity.Application.Commands.UpdateUserProfile;

/// <summary>
/// Handler for the UpdateUserProfileCommand.
/// </summary>
public sealed class UpdateUserProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserProfileCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        // Load user
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User", request.UserId);
        }

        // Update profile
        user.UpdateProfile(request.FirstName, request.LastName, request.Title, request.PhoneNumber);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
