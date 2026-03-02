using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Application.Commands.AcceptQuoteCarrier;
using IBS.Policies.Application.Commands.AddCarrierToQuote;
using IBS.Policies.Application.Commands.CancelQuote;
using IBS.Policies.Application.Commands.CreateQuote;
using IBS.Policies.Application.Commands.RecordQuoteResponse;
using IBS.Policies.Application.Commands.RemoveCarrierFromQuote;
using IBS.Policies.Application.Commands.SubmitQuote;
using IBS.Policies.Application.Queries.GetQuoteById;
using IBS.Policies.Application.Queries.GetQuotes;
using IBS.Policies.Application.Queries.GetQuotesByClient;
using IBS.Policies.Application.Queries.GetQuotesSummary;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for managing insurance quotes.
/// </summary>
[Authorize]
public sealed class QuotesController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<QuotesController> _logger;

    /// <summary>
    /// Initializes a new instance of the QuotesController class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public QuotesController(IMediator mediator, ILogger<QuotesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets a list of quotes with optional filtering.
    /// </summary>
    /// <param name="request">The filter/pagination parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of quotes.</returns>
    /// <response code="200">Returns the list of quotes.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuotes(
        [FromQuery] GetQuotesRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting quotes for tenant {TenantId}", CurrentTenantId);

        var query = new GetQuotesQuery(
            CurrentTenantId,
            request.Search,
            request.ClientId,
            request.Status,
            request.LineOfBusiness,
            request.Page,
            request.PageSize,
            request.SortBy ?? "CreatedAt",
            request.SortDirection ?? "desc");

        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a quote by identifier.
    /// </summary>
    /// <param name="id">The quote identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The quote details.</returns>
    /// <response code="200">Returns the quote.</response>
    /// <response code="404">If the quote is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("{id:guid}", Name = "GetQuoteById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuote(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting quote {QuoteId} for tenant {TenantId}", id, CurrentTenantId);

        var query = new GetQuoteByIdQuery(CurrentTenantId, id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess && result.Value is null)
        {
            return NotFound(new { error = $"Quote with ID '{id}' not found." });
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets quote summary statistics for the dashboard.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Quote summary statistics.</returns>
    /// <response code="200">Returns the summary statistics.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuotesSummary(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting quotes summary for tenant {TenantId}", CurrentTenantId);

        var query = new GetQuotesSummaryQuery(CurrentTenantId);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets quotes for a specific client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="request">The pagination parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of quotes for the client.</returns>
    /// <response code="200">Returns the list of client quotes.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("~/api/v1/clients/{clientId:guid}/quotes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuotesByClient(
        Guid clientId,
        [FromQuery] GetQuotesByClientRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting quotes for client {ClientId} for tenant {TenantId}", clientId, CurrentTenantId);

        var query = new GetQuotesByClientQuery(
            CurrentTenantId,
            clientId,
            request.Page,
            request.PageSize);

        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new quote.
    /// </summary>
    /// <param name="request">The quote creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created quote ID.</returns>
    /// <response code="201">Returns the newly created quote ID.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateQuote(
        [FromBody] CreateQuoteRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating quote for tenant {TenantId}, client {ClientId}", CurrentTenantId, request.ClientId);

        var command = new CreateQuoteCommand(
            CurrentTenantId,
            CurrentUserId,
            request.ClientId,
            request.LineOfBusiness,
            request.EffectiveDate,
            request.ExpirationDate,
            request.Notes,
            request.ExpiresAt);

        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetQuoteById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Adds a carrier to a quote.
    /// </summary>
    /// <param name="id">The quote identifier.</param>
    /// <param name="request">The carrier to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created quote carrier ID.</returns>
    /// <response code="201">Returns the newly created quote carrier ID.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the quote is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/carriers")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddCarrier(
        Guid id,
        [FromBody] AddCarrierToQuoteRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding carrier {CarrierId} to quote {QuoteId} for tenant {TenantId}", request.CarrierId, id, CurrentTenantId);

        var command = new AddCarrierToQuoteCommand(CurrentTenantId, id, request.CarrierId, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, new { id = result.Value });
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Removes a carrier from a quote.
    /// </summary>
    /// <param name="id">The quote identifier.</param>
    /// <param name="carrierQuoteId">The quote carrier identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the carrier was removed successfully.</response>
    /// <response code="400">If the carrier cannot be removed.</response>
    /// <response code="404">If the quote or carrier is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpDelete("{id:guid}/carriers/{carrierQuoteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveCarrier(
        Guid id,
        Guid carrierQuoteId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing carrier {CarrierQuoteId} from quote {QuoteId} for tenant {TenantId}", carrierQuoteId, id, CurrentTenantId);

        var command = new RemoveCarrierFromQuoteCommand(CurrentTenantId, id, carrierQuoteId, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Submits a quote to carriers.
    /// </summary>
    /// <param name="id">The quote identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the quote was submitted successfully.</response>
    /// <response code="400">If the quote cannot be submitted.</response>
    /// <response code="404">If the quote is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitQuote(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Submitting quote {QuoteId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new SubmitQuoteCommand(CurrentTenantId, id, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Records a carrier's response to a quote request.
    /// </summary>
    /// <param name="id">The quote identifier.</param>
    /// <param name="carrierQuoteId">The quote carrier identifier.</param>
    /// <param name="request">The response details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the response was recorded successfully.</response>
    /// <response code="400">If the response is invalid.</response>
    /// <response code="404">If the quote or carrier is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/carriers/{carrierQuoteId:guid}/respond")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RecordResponse(
        Guid id,
        Guid carrierQuoteId,
        [FromBody] RecordQuoteResponseRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording response for carrier {CarrierQuoteId} on quote {QuoteId} for tenant {TenantId}", carrierQuoteId, id, CurrentTenantId);

        var command = new RecordQuoteResponseCommand(
            CurrentTenantId,
            id,
            carrierQuoteId,
            request.IsQuoted,
            request.PremiumAmount,
            request.PremiumCurrency,
            request.Conditions,
            request.ProposedCoverages,
            request.CarrierExpiresAt,
            request.DeclinationReason,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Accepts a carrier's quote, creating a draft policy.
    /// </summary>
    /// <param name="id">The quote identifier.</param>
    /// <param name="carrierQuoteId">The quote carrier identifier to accept.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created policy ID.</returns>
    /// <response code="201">Returns the newly created policy ID.</response>
    /// <response code="400">If the carrier quote cannot be accepted.</response>
    /// <response code="404">If the quote or carrier is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/carriers/{carrierQuoteId:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AcceptCarrier(
        Guid id,
        Guid carrierQuoteId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Accepting carrier {CarrierQuoteId} on quote {QuoteId} for tenant {TenantId}", carrierQuoteId, id, CurrentTenantId);

        var command = new AcceptQuoteCarrierCommand(CurrentTenantId, CurrentUserId, id, carrierQuoteId, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, new { policyId = result.Value });
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Cancels a quote.
    /// </summary>
    /// <param name="id">The quote identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the quote was cancelled successfully.</response>
    /// <response code="400">If the quote cannot be cancelled.</response>
    /// <response code="404">If the quote is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelQuote(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling quote {QuoteId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new CancelQuoteCommand(CurrentTenantId, id, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }
}

/// <summary>
/// Request model for listing quotes with filtering.
/// </summary>
public sealed record GetQuotesRequest
{
    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    /// <summary>
    /// Gets or sets the client ID filter.
    /// </summary>
    [FromQuery(Name = "clientId")]
    public Guid? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the status filter.
    /// </summary>
    [FromQuery(Name = "status")]
    public QuoteStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the line of business filter.
    /// </summary>
    [FromQuery(Name = "lineOfBusiness")]
    public LineOfBusiness? LineOfBusiness { get; set; }

    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets the sort direction (asc or desc).
    /// </summary>
    [FromQuery(Name = "sortDirection")]
    public string? SortDirection { get; set; }
}

/// <summary>
/// Request model for creating a quote.
/// </summary>
public sealed record CreateQuoteRequest
{
    /// <summary>
    /// Gets or sets the client ID.
    /// </summary>
    public Guid ClientId { get; init; }

    /// <summary>
    /// Gets or sets the line of business.
    /// </summary>
    public LineOfBusiness LineOfBusiness { get; init; }

    /// <summary>
    /// Gets or sets the effective date.
    /// </summary>
    public DateOnly EffectiveDate { get; init; }

    /// <summary>
    /// Gets or sets the expiration date.
    /// </summary>
    public DateOnly ExpirationDate { get; init; }

    /// <summary>
    /// Gets or sets the notes (optional).
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Gets or sets the quote expiration date (optional, defaults to 30 days).
    /// </summary>
    public DateOnly? ExpiresAt { get; init; }
}

/// <summary>
/// Request model for adding a carrier to a quote.
/// </summary>
public sealed record AddCarrierToQuoteRequest
{
    /// <summary>
    /// Gets or sets the carrier ID.
    /// </summary>
    public Guid CarrierId { get; init; }
}

/// <summary>
/// Request model for recording a carrier's response.
/// </summary>
public sealed record RecordQuoteResponseRequest
{
    /// <summary>
    /// Gets or sets whether the carrier provided a quote (true) or declined (false).
    /// </summary>
    public bool IsQuoted { get; init; }

    /// <summary>
    /// Gets or sets the quoted premium amount (required if IsQuoted is true).
    /// </summary>
    public decimal? PremiumAmount { get; init; }

    /// <summary>
    /// Gets or sets the premium currency (defaults to USD).
    /// </summary>
    public string? PremiumCurrency { get; init; } = "USD";

    /// <summary>
    /// Gets or sets any conditions attached to the quote.
    /// </summary>
    public string? Conditions { get; init; }

    /// <summary>
    /// Gets or sets the proposed coverages as JSON.
    /// </summary>
    public string? ProposedCoverages { get; init; }

    /// <summary>
    /// Gets or sets the carrier's quote expiration date.
    /// </summary>
    public DateOnly? CarrierExpiresAt { get; init; }

    /// <summary>
    /// Gets or sets the declination reason (if carrier declined).
    /// </summary>
    public string? DeclinationReason { get; init; }
}

/// <summary>
/// Request model for getting quotes by client.
/// </summary>
public sealed record GetQuotesByClientRequest
{
    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 20;
}
