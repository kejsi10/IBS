using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Commands.Login;
using IBS.Identity.Application.Services;
using IBS.Identity.Domain.Repositories;
using IBS.Identity.Domain.ValueObjects;

namespace IBS.Identity.Application.Commands.RefreshToken;

/// <summary>
/// Handler for the RefreshTokenCommand.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IUnitOfWork unitOfWork) : ICommandHandler<RefreshTokenCommand, LoginResult>
{
    /// <inheritdoc />
    public async Task<Result<LoginResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Get user by refresh token
        var user = await userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user is null)
        {
            return Error.Unauthorized("Invalid refresh token.");
        }

        // Find the refresh token
        var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
        if (refreshToken is null || !refreshToken.IsActive)
        {
            return Error.Unauthorized("Invalid or expired refresh token.");
        }

        // Check if account is active
        if (user.Status != UserStatus.Active)
        {
            return Error.Unauthorized("Account is not active.");
        }

        // Get user roles
        var roles = user.UserRoles
            .Where(ur => ur.Role is not null)
            .Select(ur => ur.Role!.Name)
            .ToList();

        // Generate new tokens
        var (accessToken, expiresIn) = jwtService.GenerateAccessToken(
            user.Id,
            user.TenantId,
            user.Email.Value,
            roles);

        var newRefreshToken = jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = jwtService.GetRefreshTokenExpiration();

        // Revoke old token and add new one
        user.RevokeRefreshToken(request.RefreshToken, newRefreshToken);
        user.AddRefreshToken(newRefreshToken, refreshTokenExpiry, request.IpAddress, request.UserAgent);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            accessToken,
            newRefreshToken,
            expiresIn,
            user.Id,
            user.TenantId,
            user.Email.Value,
            user.FullName,
            roles);
    }
}
