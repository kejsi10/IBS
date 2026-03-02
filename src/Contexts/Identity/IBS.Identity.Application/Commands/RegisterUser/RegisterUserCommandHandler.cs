using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Queries;
using IBS.Identity.Application.Services;
using IBS.Identity.Domain.Aggregates.User;
using IBS.Identity.Domain.Repositories;
using IBS.Identity.Domain.ValueObjects;

namespace IBS.Identity.Application.Commands.RegisterUser;

/// <summary>
/// Handler for the RegisterUserCommand.
/// </summary>
public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUserQueries userQueries,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterUserCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await userQueries.EmailExistsAsync(request.TenantId, request.Email, cancellationToken: cancellationToken))
        {
            return Error.Conflict($"A user with email '{request.Email}' already exists.");
        }

        // Create value objects
        var email = Email.Create(request.Email);
        var passwordHash = PasswordHash.FromHash(passwordHasher.HashPassword(request.Password));

        // Create the user
        var user = User.Create(
            request.TenantId,
            email,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.Title,
            request.PhoneNumber);

        // Persist the user
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
