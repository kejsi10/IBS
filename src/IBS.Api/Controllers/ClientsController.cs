using IBS.Clients.Application.Commands.AddAddress;
using IBS.Clients.Application.Commands.AddContact;
using IBS.Clients.Application.Commands.CreateBusinessClient;
using IBS.Clients.Application.Commands.CreateIndividualClient;
using IBS.Clients.Application.Commands.DeactivateClient;
using IBS.Clients.Application.Commands.LogCommunication;
using IBS.Clients.Application.Commands.RemoveAddress;
using IBS.Clients.Application.Commands.RemoveContact;
using IBS.Clients.Application.Commands.SetPrimaryAddress;
using IBS.Clients.Application.Commands.SetPrimaryContact;
using IBS.Clients.Application.Commands.UpdateAddress;
using IBS.Clients.Application.Commands.UpdateClient;
using IBS.Clients.Application.Commands.UpdateContact;
using IBS.Clients.Application.Queries.GetClientById;
using IBS.Clients.Application.Queries.SearchClients;
using IBS.Clients.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for managing clients.
/// </summary>
[Authorize]
public sealed class ClientsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClientsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ClientsController class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public ClientsController(IMediator mediator, ILogger<ClientsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets a list of clients.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of clients.</returns>
    /// <response code="200">Returns the list of clients.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetClients(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting clients for tenant {TenantId}", CurrentTenantId);

        var query = new SearchClientsQuery(search, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a client by identifier.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The client details.</returns>
    /// <response code="200">Returns the client.</response>
    /// <response code="404">If the client is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("{id:guid}", Name = "GetClientById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetClient(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting client {ClientId} for tenant {TenantId}", id, CurrentTenantId);

        var query = new GetClientByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new individual client.
    /// </summary>
    /// <param name="request">The client creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created client.</returns>
    /// <response code="201">Returns the newly created client ID.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("individual")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateIndividualClient(
        [FromBody] CreateIndividualClientRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating individual client for tenant {TenantId}", CurrentTenantId);

        var command = new CreateIndividualClientCommand(
            CurrentTenantId,
            CurrentUserId,
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.Suffix,
            request.DateOfBirth,
            request.Email,
            request.Phone);

        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetClientById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Creates a new business client.
    /// </summary>
    /// <param name="request">The client creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created client.</returns>
    /// <response code="201">Returns the newly created client ID.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("business")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBusinessClient(
        [FromBody] CreateBusinessClientRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating business client for tenant {TenantId}", CurrentTenantId);

        var command = new CreateBusinessClientCommand(
            CurrentTenantId,
            CurrentUserId,
            request.BusinessName,
            request.BusinessType,
            request.DbaName,
            request.Industry,
            request.YearEstablished,
            request.NumberOfEmployees,
            request.AnnualRevenue,
            request.Website,
            request.Email,
            request.Phone);

        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetClientById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Updates a client.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="request">The client update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the client was updated successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the client is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateClient(
        Guid id,
        [FromBody] UpdateClientRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating client {ClientId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new UpdateClientCommand(
            id,
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.Suffix,
            request.BusinessName,
            request.BusinessType,
            request.DbaName,
            request.Industry,
            request.YearEstablished,
            request.NumberOfEmployees,
            request.AnnualRevenue,
            request.Website,
            request.Email,
            request.Phone);

        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Deactivates a client.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the client was deactivated successfully.</response>
    /// <response code="404">If the client is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateClient(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating client {ClientId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new DeactivateClientCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    // ==========================================
    // Contact Operations
    // ==========================================

    /// <summary>
    /// Adds a contact to a client.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="request">The add contact request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created contact's identifier.</returns>
    [HttpPost("{id:guid}/contacts")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddContact(
        Guid id,
        [FromBody] AddContactRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding contact to client {ClientId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new AddContactCommand(
            id,
            CurrentTenantId,
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.Suffix,
            request.Title,
            request.Email,
            request.Phone,
            request.IsPrimary);

        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetClientById", new { id });
    }

    /// <summary>
    /// Updates a contact on a client.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="contactId">The contact identifier.</param>
    /// <param name="request">The update contact request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id:guid}/contacts/{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateContact(
        Guid id,
        Guid contactId,
        [FromBody] UpdateContactRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating contact {ContactId} on client {ClientId}", contactId, id);

        var command = new UpdateContactCommand(
            id,
            contactId,
            CurrentTenantId,
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.Suffix,
            request.Title,
            request.Email,
            request.Phone);

        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Removes a contact from a client.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="contactId">The contact identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:guid}/contacts/{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveContact(
        Guid id,
        Guid contactId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing contact {ContactId} from client {ClientId}", contactId, id);

        var command = new RemoveContactCommand(id, contactId, CurrentTenantId);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Patches a contact on a client (e.g., set as primary).
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="contactId">The contact identifier.</param>
    /// <param name="request">The patch request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPatch("{id:guid}/contacts/{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PatchContact(
        Guid id,
        Guid contactId,
        [FromBody] PatchContactRequest request,
        CancellationToken cancellationToken)
    {
        if (request.IsPrimary == true)
        {
            _logger.LogInformation("Setting primary contact {ContactId} on client {ClientId}", contactId, id);

            var command = new SetPrimaryContactCommand(id, contactId, CurrentTenantId);
            var result = await _mediator.Send(command, cancellationToken);

            return ToActionResult(result);
        }

        return BadRequest("No valid patch operations specified.");
    }

    // ==========================================
    // Address Operations
    // ==========================================

    /// <summary>
    /// Adds an address to a client.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="request">The add address request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created address's identifier.</returns>
    [HttpPost("{id:guid}/addresses")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddAddress(
        Guid id,
        [FromBody] AddAddressRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding address to client {ClientId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new AddAddressCommand(
            id,
            CurrentTenantId,
            request.AddressType,
            request.StreetLine1,
            request.StreetLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.IsPrimary);

        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetClientById", new { id });
    }

    /// <summary>
    /// Updates an address on a client.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="addressId">The address identifier.</param>
    /// <param name="request">The update address request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id:guid}/addresses/{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateAddress(
        Guid id,
        Guid addressId,
        [FromBody] UpdateAddressRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating address {AddressId} on client {ClientId}", addressId, id);

        var command = new UpdateAddressCommand(
            id,
            addressId,
            CurrentTenantId,
            request.StreetLine1,
            request.StreetLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Removes an address from a client.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="addressId">The address identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:guid}/addresses/{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveAddress(
        Guid id,
        Guid addressId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing address {AddressId} from client {ClientId}", addressId, id);

        var command = new RemoveAddressCommand(id, addressId, CurrentTenantId);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Patches an address on a client (e.g., set as primary).
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="addressId">The address identifier.</param>
    /// <param name="request">The patch request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPatch("{id:guid}/addresses/{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PatchAddress(
        Guid id,
        Guid addressId,
        [FromBody] PatchAddressRequest request,
        CancellationToken cancellationToken)
    {
        if (request.IsPrimary == true)
        {
            _logger.LogInformation("Setting primary address {AddressId} on client {ClientId}", addressId, id);

            var command = new SetPrimaryAddressCommand(id, addressId, CurrentTenantId);
            var result = await _mediator.Send(command, cancellationToken);

            return ToActionResult(result);
        }

        return BadRequest("No valid patch operations specified.");
    }

    // ==========================================
    // Communication Operations
    // ==========================================

    /// <summary>
    /// Logs a communication for a client.
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="request">The log communication request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created communication's identifier.</returns>
    [HttpPost("{id:guid}/communications")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogCommunication(
        Guid id,
        [FromBody] LogCommunicationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logging communication for client {ClientId}", id);

        var command = new LogCommunicationCommand(
            id,
            CurrentTenantId,
            CurrentUserId,
            request.CommunicationType,
            request.Subject,
            request.Notes);

        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetClientById", new { id });
    }
}

/// <summary>
/// Request model for creating an individual client.
/// </summary>
public sealed record CreateIndividualClientRequest
{
    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the middle name (optional).
    /// </summary>
    public string? MiddleName { get; init; }

    /// <summary>
    /// Gets the suffix (optional).
    /// </summary>
    public string? Suffix { get; init; }

    /// <summary>
    /// Gets the date of birth (optional).
    /// </summary>
    public DateOnly? DateOfBirth { get; init; }

    /// <summary>
    /// Gets the email address (optional).
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the phone number (optional).
    /// </summary>
    public string? Phone { get; init; }
}

/// <summary>
/// Request model for creating a business client.
/// </summary>
public sealed record CreateBusinessClientRequest
{
    /// <summary>
    /// Gets the business name.
    /// </summary>
    public string BusinessName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the business type.
    /// </summary>
    public string BusinessType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the DBA name (optional).
    /// </summary>
    public string? DbaName { get; init; }

    /// <summary>
    /// Gets the industry (optional).
    /// </summary>
    public string? Industry { get; init; }

    /// <summary>
    /// Gets the year established (optional).
    /// </summary>
    public int? YearEstablished { get; init; }

    /// <summary>
    /// Gets the number of employees (optional).
    /// </summary>
    public int? NumberOfEmployees { get; init; }

    /// <summary>
    /// Gets the annual revenue (optional).
    /// </summary>
    public decimal? AnnualRevenue { get; init; }

    /// <summary>
    /// Gets the website URL (optional).
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// Gets the email address (optional).
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the phone number (optional).
    /// </summary>
    public string? Phone { get; init; }
}

/// <summary>
/// Request model for updating a client.
/// </summary>
public sealed record UpdateClientRequest
{
    /// <summary>
    /// Gets the first name (for individual clients).
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Gets the last name (for individual clients).
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Gets the middle name (for individual clients).
    /// </summary>
    public string? MiddleName { get; init; }

    /// <summary>
    /// Gets the suffix (for individual clients).
    /// </summary>
    public string? Suffix { get; init; }

    /// <summary>
    /// Gets the business name (for business clients).
    /// </summary>
    public string? BusinessName { get; init; }

    /// <summary>
    /// Gets the business type (for business clients).
    /// </summary>
    public string? BusinessType { get; init; }

    /// <summary>
    /// Gets the DBA name (for business clients).
    /// </summary>
    public string? DbaName { get; init; }

    /// <summary>
    /// Gets the industry (for business clients).
    /// </summary>
    public string? Industry { get; init; }

    /// <summary>
    /// Gets the year established (for business clients).
    /// </summary>
    public int? YearEstablished { get; init; }

    /// <summary>
    /// Gets the number of employees (for business clients).
    /// </summary>
    public int? NumberOfEmployees { get; init; }

    /// <summary>
    /// Gets the annual revenue (for business clients).
    /// </summary>
    public decimal? AnnualRevenue { get; init; }

    /// <summary>
    /// Gets the website (for business clients).
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the phone number.
    /// </summary>
    public string? Phone { get; init; }
}

/// <summary>
/// Request model for adding a contact to a client.
/// </summary>
public sealed record AddContactRequest
{
    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the middle name (optional).
    /// </summary>
    public string? MiddleName { get; init; }

    /// <summary>
    /// Gets the suffix (optional).
    /// </summary>
    public string? Suffix { get; init; }

    /// <summary>
    /// Gets the title (optional).
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the email address (optional).
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the phone number (optional).
    /// </summary>
    public string? Phone { get; init; }

    /// <summary>
    /// Gets whether this is the primary contact.
    /// </summary>
    public bool IsPrimary { get; init; }
}

/// <summary>
/// Request model for updating a contact.
/// </summary>
public sealed record UpdateContactRequest
{
    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the middle name (optional).
    /// </summary>
    public string? MiddleName { get; init; }

    /// <summary>
    /// Gets the suffix (optional).
    /// </summary>
    public string? Suffix { get; init; }

    /// <summary>
    /// Gets the title (optional).
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the email address (optional).
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the phone number (optional).
    /// </summary>
    public string? Phone { get; init; }
}

/// <summary>
/// Request model for adding an address to a client.
/// </summary>
public sealed record AddAddressRequest
{
    /// <summary>
    /// Gets the address type.
    /// </summary>
    public AddressType AddressType { get; init; }

    /// <summary>
    /// Gets the street line 1.
    /// </summary>
    public string StreetLine1 { get; init; } = string.Empty;

    /// <summary>
    /// Gets the street line 2 (optional).
    /// </summary>
    public string? StreetLine2 { get; init; }

    /// <summary>
    /// Gets the city.
    /// </summary>
    public string City { get; init; } = string.Empty;

    /// <summary>
    /// Gets the state.
    /// </summary>
    public string State { get; init; } = string.Empty;

    /// <summary>
    /// Gets the postal code.
    /// </summary>
    public string PostalCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the country.
    /// </summary>
    public string Country { get; init; } = "USA";

    /// <summary>
    /// Gets whether this is the primary address.
    /// </summary>
    public bool IsPrimary { get; init; }
}

/// <summary>
/// Request model for updating an address.
/// </summary>
public sealed record UpdateAddressRequest
{
    /// <summary>
    /// Gets the street line 1.
    /// </summary>
    public string StreetLine1 { get; init; } = string.Empty;

    /// <summary>
    /// Gets the street line 2 (optional).
    /// </summary>
    public string? StreetLine2 { get; init; }

    /// <summary>
    /// Gets the city.
    /// </summary>
    public string City { get; init; } = string.Empty;

    /// <summary>
    /// Gets the state.
    /// </summary>
    public string State { get; init; } = string.Empty;

    /// <summary>
    /// Gets the postal code.
    /// </summary>
    public string PostalCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the country.
    /// </summary>
    public string Country { get; init; } = "USA";
}

/// <summary>
/// Request model for logging a communication.
/// </summary>
public sealed record LogCommunicationRequest
{
    /// <summary>
    /// Gets the communication type.
    /// </summary>
    public CommunicationType CommunicationType { get; init; }

    /// <summary>
    /// Gets the subject.
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Gets the notes (optional).
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Request model for patching a contact.
/// </summary>
public sealed record PatchContactRequest
{
    /// <summary>
    /// Gets whether to set the contact as primary (optional).
    /// </summary>
    public bool? IsPrimary { get; init; }
}

/// <summary>
/// Request model for patching an address.
/// </summary>
public sealed record PatchAddressRequest
{
    /// <summary>
    /// Gets whether to set the address as primary (optional).
    /// </summary>
    public bool? IsPrimary { get; init; }
}
