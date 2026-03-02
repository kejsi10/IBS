using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.CreateIndividualClient;

/// <summary>
/// Command to create a new individual client.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The creating user identifier.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="MiddleName">The middle name (optional).</param>
/// <param name="Suffix">The name suffix (optional).</param>
/// <param name="DateOfBirth">The date of birth (optional).</param>
/// <param name="Email">The email address (optional).</param>
/// <param name="Phone">The phone number (optional).</param>
public sealed record CreateIndividualClientCommand(
    Guid TenantId,
    Guid UserId,
    string FirstName,
    string LastName,
    string? MiddleName = null,
    string? Suffix = null,
    DateOnly? DateOfBirth = null,
    string? Email = null,
    string? Phone = null) : ICommand<Guid>;
