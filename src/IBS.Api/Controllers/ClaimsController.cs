using IBS.Claims.Application.Commands.AddClaimNote;
using IBS.Claims.Application.Commands.AuthorizePayment;
using IBS.Claims.Application.Commands.CreateClaim;
using IBS.Claims.Application.Commands.IssuePayment;
using IBS.Claims.Application.Commands.SetReserve;
using IBS.Claims.Application.Commands.UpdateClaimStatus;
using IBS.Claims.Application.Commands.VoidPayment;
using IBS.Claims.Application.Queries.GetClaimById;
using IBS.Claims.Application.Queries.GetClaims;
using IBS.Claims.Application.Queries.GetClaimStatistics;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for managing insurance claims.
/// </summary>
[Authorize]
public sealed class ClaimsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClaimsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ClaimsController class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public ClaimsController(IMediator mediator, ILogger<ClaimsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of claims with optional filtering.
    /// </summary>
    /// <param name="request">The filter/pagination parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of claims.</returns>
    /// <response code="200">Returns the list of claims.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetClaims(
        [FromQuery] GetClaimsRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting claims for tenant {TenantId}", CurrentTenantId);

        var query = new GetClaimsQuery(
            CurrentTenantId,
            request.Search,
            request.Status,
            request.PolicyId,
            request.ClientId,
            request.LossType,
            request.LossDateFrom,
            request.LossDateTo,
            request.Page,
            request.PageSize,
            request.SortBy ?? "CreatedAt",
            request.SortDirection ?? "desc");

        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a claim by its identifier.
    /// </summary>
    /// <param name="id">The claim identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The claim details.</returns>
    /// <response code="200">Returns the claim.</response>
    /// <response code="404">If the claim is not found.</response>
    [HttpGet("{id:guid}", Name = "GetClaimById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaimById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetClaimByIdQuery(CurrentTenantId, id);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets claim statistics for the dashboard.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Claim statistics.</returns>
    /// <response code="200">Returns the claim statistics.</response>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var query = new GetClaimStatisticsQuery(CurrentTenantId);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new claim (FNOL — First Notice of Loss).
    /// </summary>
    /// <param name="request">The claim creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created claim identifier.</returns>
    /// <response code="201">Claim created successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClaim(
        [FromBody] CreateClaimRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating claim for policy {PolicyId}", request.PolicyId);

        var command = new CreateClaimCommand(
            CurrentTenantId,
            CurrentUserId,
            request.PolicyId,
            request.ClientId,
            request.LossDate,
            request.ReportedDate,
            request.LossType,
            request.LossDescription,
            request.EstimatedLossAmount,
            request.EstimatedLossCurrency);

        var result = await _mediator.Send(command, cancellationToken);
        return ToCreatedResult(result, "GetClaimById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Updates the status of a claim.
    /// </summary>
    /// <param name="id">The claim identifier.</param>
    /// <param name="request">The status update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Status updated successfully.</response>
    /// <response code="400">If the transition is invalid.</response>
    /// <response code="404">If the claim is not found.</response>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateClaimStatusRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating claim {ClaimId} status to {Status}", id, request.NewStatus);

        var command = new UpdateClaimStatusCommand(
            CurrentTenantId,
            CurrentUserId,
            id,
            request.NewStatus,
            request.ClaimAmount,
            request.ClaimAmountCurrency,
            request.DenialReason,
            request.ClosureReason,
            request.AdjusterId,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Adds a note to a claim.
    /// </summary>
    /// <param name="id">The claim identifier.</param>
    /// <param name="request">The note request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Note added successfully.</response>
    /// <response code="404">If the claim is not found.</response>
    [HttpPost("{id:guid}/notes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNote(
        Guid id,
        [FromBody] AddClaimNoteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddClaimNoteCommand(
            CurrentTenantId,
            CurrentUserId,
            id,
            request.Content,
            request.IsInternal,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sets a reserve on a claim.
    /// </summary>
    /// <param name="id">The claim identifier.</param>
    /// <param name="request">The reserve request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Reserve set successfully.</response>
    /// <response code="404">If the claim is not found.</response>
    [HttpPost("{id:guid}/reserves")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetReserve(
        Guid id,
        [FromBody] SetReserveRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetReserveCommand(
            CurrentTenantId,
            CurrentUserId,
            id,
            request.ReserveType,
            request.Amount,
            request.Currency,
            request.Notes,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Authorizes a payment on a claim.
    /// </summary>
    /// <param name="id">The claim identifier.</param>
    /// <param name="request">The payment request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created payment identifier.</returns>
    /// <response code="201">Payment authorized successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the claim is not found.</response>
    [HttpPost("{id:guid}/payments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AuthorizePayment(
        Guid id,
        [FromBody] AuthorizePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AuthorizePaymentCommand(
            CurrentTenantId,
            CurrentUserId,
            id,
            request.PaymentType,
            request.Amount,
            request.Currency,
            request.PayeeName,
            request.PaymentDate,
            request.CheckNumber,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(new { id = result.Value });

        return ToActionResult(result);
    }

    /// <summary>
    /// Issues an authorized payment.
    /// </summary>
    /// <param name="id">The claim identifier.</param>
    /// <param name="paymentId">The payment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Payment issued successfully.</response>
    /// <response code="400">If the payment cannot be issued.</response>
    /// <response code="404">If the claim or payment is not found.</response>
    [HttpPut("{id:guid}/payments/{paymentId:guid}/issue")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IssuePayment(
        Guid id,
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        var command = new IssuePaymentCommand(CurrentTenantId, CurrentUserId, id, paymentId, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Voids a payment.
    /// </summary>
    /// <param name="id">The claim identifier.</param>
    /// <param name="paymentId">The payment identifier.</param>
    /// <param name="request">The void request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Payment voided successfully.</response>
    /// <response code="400">If the payment cannot be voided.</response>
    /// <response code="404">If the claim or payment is not found.</response>
    [HttpPut("{id:guid}/payments/{paymentId:guid}/void")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VoidPayment(
        Guid id,
        Guid paymentId,
        [FromBody] VoidPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new VoidPaymentCommand(CurrentTenantId, CurrentUserId, id, paymentId, request.Reason, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }
}

// Request/Response records

/// <summary>
/// Request for getting claims with filtering.
/// </summary>
public sealed record GetClaimsRequest(
    string? Search = null,
    ClaimStatus? Status = null,
    Guid? PolicyId = null,
    Guid? ClientId = null,
    LossType? LossType = null,
    DateTimeOffset? LossDateFrom = null,
    DateTimeOffset? LossDateTo = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDirection = null
);

/// <summary>
/// Request for creating a new claim.
/// </summary>
public sealed record CreateClaimRequest(
    Guid PolicyId,
    Guid ClientId,
    DateTimeOffset LossDate,
    DateTimeOffset ReportedDate,
    LossType LossType,
    string LossDescription,
    decimal? EstimatedLossAmount = null,
    string? EstimatedLossCurrency = "USD"
);

/// <summary>
/// Request for updating claim status.
/// </summary>
public sealed record UpdateClaimStatusRequest(
    ClaimStatus NewStatus,
    decimal? ClaimAmount = null,
    string? ClaimAmountCurrency = "USD",
    string? DenialReason = null,
    string? ClosureReason = null,
    string? AdjusterId = null
);

/// <summary>
/// Request for adding a note to a claim.
/// </summary>
public sealed record AddClaimNoteRequest(
    string Content,
    bool IsInternal = false
);

/// <summary>
/// Request for setting a reserve.
/// </summary>
public sealed record SetReserveRequest(
    string ReserveType,
    decimal Amount,
    string Currency = "USD",
    string? Notes = null
);

/// <summary>
/// Request for authorizing a payment.
/// </summary>
public sealed record AuthorizePaymentRequest(
    string PaymentType,
    decimal Amount,
    string Currency,
    string PayeeName,
    DateOnly PaymentDate,
    string? CheckNumber = null
);

/// <summary>
/// Request for voiding a payment.
/// </summary>
public sealed record VoidPaymentRequest(
    string Reason
);
