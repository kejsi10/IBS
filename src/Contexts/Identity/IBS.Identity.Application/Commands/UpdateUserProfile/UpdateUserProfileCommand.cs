using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.UpdateUserProfile;

/// <summary>
/// Command to update a user's profile information.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Title">The title (optional).</param>
/// <param name="PhoneNumber">The phone number (optional).</param>
public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? Title = null,
    string? PhoneNumber = null) : ICommand;
