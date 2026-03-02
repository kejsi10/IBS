using IBS.Carriers.Application.Commands.AddAppetite;
using IBS.Carriers.Application.Commands.AddProduct;
using IBS.Carriers.Application.Commands.CreateCarrier;
using IBS.Carriers.Application.Commands.DeactivateCarrier;
using IBS.Carriers.Application.Commands.UpdateCarrier;
using IBS.Carriers.Application.DTOs;
using IBS.Carriers.Application.Queries.GetAllCarriers;
using IBS.Carriers.Application.Queries.GetCarrierById;
using IBS.Carriers.Application.Queries.GetCarriersByStatus;
using IBS.Carriers.Application.Queries.SearchCarriers;
using IBS.Carriers.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for managing insurance carriers.
/// </summary>
[Authorize]
public class CarriersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CarriersController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    public CarriersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets carriers, optionally filtered by search term and/or status.
    /// </summary>
    /// <param name="search">Optional search term to filter by name or code.</param>
    /// <param name="status">Optional carrier status filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of carrier summaries.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CarrierSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] CarrierStatus? status,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchQuery = new SearchCarriersQuery(search);
            var searchResult = await _mediator.Send(searchQuery, cancellationToken);
            return ToActionResult(searchResult);
        }

        if (status.HasValue)
        {
            var statusQuery = new GetCarriersByStatusQuery(status.Value);
            var statusResult = await _mediator.Send(statusQuery, cancellationToken);
            return ToActionResult(statusResult);
        }

        var query = new GetAllCarriersQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a carrier by its identifier.
    /// </summary>
    /// <param name="id">The carrier identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The carrier details.</returns>
    [HttpGet("{id:guid}", Name = "GetCarrierById")]
    [ProducesResponseType(typeof(CarrierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCarrierByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new carrier.
    /// </summary>
    /// <param name="request">The create carrier request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created carrier's identifier.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCarrierRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCarrierCommand(
            request.Name,
            request.Code,
            request.LegalName,
            request.AmBestRating,
            request.NaicCode,
            request.WebsiteUrl,
            request.Notes);

        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetCarrierById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Updates an existing carrier.
    /// </summary>
    /// <param name="id">The carrier identifier.</param>
    /// <param name="request">The update carrier request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCarrierRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCarrierCommand(
            id,
            request.Name,
            request.LegalName,
            request.AmBestRating,
            request.NaicCode,
            request.WebsiteUrl,
            request.ApiEndpoint,
            request.Notes,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Deactivates a carrier.
    /// </summary>
    /// <param name="id">The carrier identifier.</param>
    /// <param name="request">The deactivation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateCarrierRequest? request, CancellationToken cancellationToken)
    {
        var command = new DeactivateCarrierCommand(id, request?.Reason, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Adds a product to a carrier.
    /// </summary>
    /// <param name="id">The carrier identifier.</param>
    /// <param name="request">The add product request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created product's identifier.</returns>
    [HttpPost("{id:guid}/products")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddProduct(Guid id, [FromBody] AddProductRequest request, CancellationToken cancellationToken)
    {
        var command = new AddProductCommand(
            id,
            request.Name,
            request.Code,
            request.LineOfBusiness,
            request.Description,
            request.MinimumPremium,
            request.EffectiveDate,
            request.ExpirationDate,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtRoute("GetCarrierById", new { id }, new { productId = result.Value });
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Adds an appetite rule to a carrier.
    /// </summary>
    /// <param name="id">The carrier identifier.</param>
    /// <param name="request">The add appetite request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created appetite's identifier.</returns>
    [HttpPost("{id:guid}/appetites")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAppetite(Guid id, [FromBody] AddAppetiteRequest request, CancellationToken cancellationToken)
    {
        var command = new AddAppetiteCommand(
            id,
            request.LineOfBusiness,
            request.States,
            request.MinYearsInBusiness,
            request.MaxYearsInBusiness,
            request.MinAnnualRevenue,
            request.MaxAnnualRevenue,
            request.MinEmployees,
            request.MaxEmployees,
            request.AcceptedIndustries,
            request.ExcludedIndustries,
            request.Notes,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtRoute("GetCarrierById", new { id }, new { appetiteId = result.Value });
        }

        return ToActionResult(result);
    }
}

/// <summary>
/// Request to create a new carrier.
/// </summary>
public sealed record CreateCarrierRequest(
    string Name,
    string Code,
    string? LegalName = null,
    string? AmBestRating = null,
    string? NaicCode = null,
    string? WebsiteUrl = null,
    string? Notes = null);

/// <summary>
/// Request to update a carrier.
/// </summary>
public sealed record UpdateCarrierRequest(
    string Name,
    string? LegalName = null,
    string? AmBestRating = null,
    string? NaicCode = null,
    string? WebsiteUrl = null,
    string? ApiEndpoint = null,
    string? Notes = null);

/// <summary>
/// Request to deactivate a carrier.
/// </summary>
public sealed record DeactivateCarrierRequest(string? Reason = null);

/// <summary>
/// Request to add a product to a carrier.
/// </summary>
public sealed record AddProductRequest(
    string Name,
    string Code,
    LineOfBusiness LineOfBusiness,
    string? Description = null,
    decimal? MinimumPremium = null,
    DateOnly? EffectiveDate = null,
    DateOnly? ExpirationDate = null);

/// <summary>
/// Request to add an appetite rule to a carrier.
/// </summary>
public sealed record AddAppetiteRequest(
    LineOfBusiness LineOfBusiness,
    string States = "ALL",
    int? MinYearsInBusiness = null,
    int? MaxYearsInBusiness = null,
    decimal? MinAnnualRevenue = null,
    decimal? MaxAnnualRevenue = null,
    int? MinEmployees = null,
    int? MaxEmployees = null,
    string? AcceptedIndustries = null,
    string? ExcludedIndustries = null,
    string? Notes = null);
