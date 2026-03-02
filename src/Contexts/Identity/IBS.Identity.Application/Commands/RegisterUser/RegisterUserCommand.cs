using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.RegisterUser;

/// <summary>
/// Command to register a new user.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="Email">The email address.</param>
/// <param name="Password">The password.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Title">The title (optional).</param>
/// <param name="PhoneNumber">The phone number (optional).</param>
public sealed record RegisterUserCommand(
    Guid TenantId,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Title = null,
    string? PhoneNumber = null) : ICommand<Guid>;
