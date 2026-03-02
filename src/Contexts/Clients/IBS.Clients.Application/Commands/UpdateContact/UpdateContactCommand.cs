using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.UpdateContact;

/// <summary>
/// Command to update a contact.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ContactId">The contact identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="FirstName">The contact's first name.</param>
/// <param name="LastName">The contact's last name.</param>
/// <param name="MiddleName">The contact's middle name.</param>
/// <param name="Suffix">The contact's suffix.</param>
/// <param name="Title">The contact's title/role.</param>
/// <param name="Email">The contact's email address.</param>
/// <param name="Phone">The contact's phone number.</param>
public sealed record UpdateContactCommand(
    Guid ClientId,
    Guid ContactId,
    Guid TenantId,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? Suffix,
    string? Title,
    string? Email,
    string? Phone) : ICommand;
