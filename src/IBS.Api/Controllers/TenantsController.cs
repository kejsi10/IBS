using IBS.Tenants.Application.Commands.ActivateTenant;
using IBS.Tenants.Application.Commands.AddTenantCarrier;
using IBS.Tenants.Application.Commands.CancelTenant;
using IBS.Tenants.Application.Commands.CreateTenant;
using IBS.Tenants.Application.Commands.RemoveTenantCarrier;
using IBS.Tenants.Application.Commands.SuspendTenant;
using IBS.Tenants.Application.Commands.UpdateSubscriptionTier;
using IBS.Tenants.Application.Commands.UpdateTenant;
using IBS.Tenants.Application.Queries;
using IBS.Tenants.Application.Queries.GetTenantById;
using IBS.Tenants.Application.Queries.SearchTenants;
using IBS.Tenants.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for managing tenants.
/// </summary>
[Authorize]
public sealed class TenantsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public TenantsController(IMediator mediator, ILogger<TenantsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Searches tenants with pagination.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of tenants.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TenantListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching tenants");

        var query = new SearchTenantsQuery(search, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a tenant by identifier.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant details.</returns>
    [HttpGet("{id:guid}", Name = "GetTenantById")]
    [ProducesResponseType(typeof(TenantDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tenant {TenantId}", id);

        var query = new GetTenantByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="request">The create tenant request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created tenant's identifier.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating tenant {TenantName}", request.Name);

        var command = new CreateTenantCommand(
            request.Name,
            request.Subdomain,
            request.SubscriptionTier);

        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetTenantById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Updates an existing tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="request">The update tenant request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating tenant {TenantId}", id);

        var command = new UpdateTenantCommand(id, request.Name, request.Settings);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Suspends a tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Suspending tenant {TenantId}", id);

        var command = new SuspendTenantCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Activates a suspended tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating tenant {TenantId}", id);

        var command = new ActivateTenantCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Cancels a tenant's subscription.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling tenant {TenantId}", id);

        var command = new CancelTenantCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Updates a tenant's subscription tier.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="request">The update subscription request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id:guid}/subscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateSubscription(
        Guid id,
        [FromBody] UpdateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating subscription for tenant {TenantId}", id);

        var command = new UpdateSubscriptionTierCommand(id, request.SubscriptionTier);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Adds a carrier to a tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="request">The add carrier request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{id:guid}/carriers")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddCarrier(
        Guid id,
        [FromBody] AddTenantCarrierRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding carrier {CarrierId} to tenant {TenantId}", request.CarrierId, id);

        var command = new AddTenantCarrierCommand(id, request.CarrierId, request.AgencyCode, request.CommissionRate);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Removes a carrier from a tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="carrierId">The carrier identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:guid}/carriers/{carrierId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveCarrier(Guid id, Guid carrierId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing carrier {CarrierId} from tenant {TenantId}", carrierId, id);

        var command = new RemoveTenantCarrierCommand(id, carrierId);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }
}

/// <summary>
/// Request to create a new tenant.
/// </summary>
public sealed record CreateTenantRequest(
    string Name,
    string Subdomain,
    SubscriptionTier SubscriptionTier);

/// <summary>
/// Request to update a tenant.
/// </summary>
public sealed record UpdateTenantRequest(
    string Name,
    string? Settings = null);

/// <summary>
/// Request to update a tenant's subscription tier.
/// </summary>
public sealed record UpdateSubscriptionRequest(
    SubscriptionTier SubscriptionTier);

/// <summary>
/// Request to add a carrier to a tenant.
/// </summary>
public sealed record AddTenantCarrierRequest(
    Guid CarrierId,
    string? AgencyCode = null,
    decimal? CommissionRate = null);
