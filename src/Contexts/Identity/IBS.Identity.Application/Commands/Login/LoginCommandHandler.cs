using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Services;
using IBS.Identity.Domain.Repositories;
using IBS.Identity.Domain.ValueObjects;

namespace IBS.Identity.Application.Commands.Login;

/// <summary>
/// Handler for the LoginCommand.
/// </summary>
public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IUnitOfWork unitOfWork) : ICommandHandler<LoginCommand, LoginResult>
{
    /// <inheritdoc />
    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await userRepository.GetByEmailAsync(request.TenantId, request.Email, cancellationToken);
        if (user is null)
        {
            return Error.Unauthorized("Invalid email or password.");
        }

        // Check if account is locked — auto-unlock when the lockout period has expired
        if (user.Status == UserStatus.Locked)
        {
            if (user.LockedUntil.HasValue && user.LockedUntil <= DateTimeOffset.UtcNow)
            {
                user.Unlock();
            }
            else
            {
                return Error.Unauthorized("Account is locked. Please try again later.");
            }
        }

        // Check if account is inactive
        if (user.Status == UserStatus.Inactive)
        {
            return Error.Unauthorized("Account is deactivated. Please contact support.");
        }

        // Check if account is pending activation
        if (user.Status == UserStatus.Pending)
        {
            return Error.Unauthorized("Account is not activated. Please check your email.");
        }

        // Verify password
        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Value))
        {
            user.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Error.Unauthorized("Invalid email or password.");
        }

        // Get user roles
        var roles = user.UserRoles
            .Where(ur => ur.Role is not null)
            .Select(ur => ur.Role!.Name)
            .ToList();

        // Generate tokens
        var (accessToken, expiresIn) = jwtService.GenerateAccessToken(
            user.Id,
            user.TenantId,
            user.Email.Value,
            roles);

        var refreshToken = jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = jwtService.GetRefreshTokenExpiration();

        // Record successful login and add refresh token
        user.RecordSuccessfulLogin();
        user.AddRefreshToken(refreshToken, refreshTokenExpiry, request.IpAddress, request.UserAgent);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            accessToken,
            refreshToken,
            expiresIn,
            user.Id,
            user.TenantId,
            user.Email.Value,
            user.FullName,
            roles);
    }
}
