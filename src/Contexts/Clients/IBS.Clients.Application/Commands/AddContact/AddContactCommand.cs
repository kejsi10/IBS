using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.AddContact;

/// <summary>
/// Command to add a contact to a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="FirstName">The contact's first name.</param>
/// <param name="LastName">The contact's last name.</param>
/// <param name="MiddleName">The contact's middle name.</param>
/// <param name="Suffix">The contact's suffix.</param>
/// <param name="Title">The contact's title/role.</param>
/// <param name="Email">The contact's email address.</param>
/// <param name="Phone">The contact's phone number.</param>
/// <param name="IsPrimary">Whether this is the primary contact.</param>
public sealed record AddContactCommand(
    Guid ClientId,
    Guid TenantId,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? Suffix,
    string? Title,
    string? Email,
    string? Phone,
    bool IsPrimary) : ICommand<Guid>;
