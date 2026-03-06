using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Application.Commands.ActivatePolicy;
using IBS.Policies.Application.Commands.AddCoverage;
using IBS.Policies.Application.Commands.AddEndorsement;
using IBS.Policies.Application.Commands.ApproveEndorsement;
using IBS.Policies.Application.Commands.BindPolicy;
using IBS.Policies.Application.Commands.CancelPolicy;
using IBS.Policies.Application.Commands.CreatePolicy;
using IBS.Policies.Application.Commands.ReinstatePolicy;
using IBS.Policies.Application.Commands.IssueEndorsement;
using IBS.Policies.Application.Commands.RejectEndorsement;
using IBS.Policies.Application.Commands.RemoveCoverage;
using IBS.Policies.Application.Commands.RenewPolicy;
using IBS.Policies.Application.Commands.UpdateCoverage;
using IBS.Policies.Application.Queries.GetExpiringPolicies;
using IBS.Policies.Application.Queries.GetPolicies;
using IBS.Policies.Application.Queries.GetPoliciesByClient;
using IBS.Policies.Application.Queries.GetPoliciesDueForRenewal;
using IBS.Policies.Application.Queries.GetPolicyById;
using IBS.Policies.Application.Commands.CreateRenewalQuote;
using IBS.Policies.Application.Queries.GetPolicyHistory;
using IBS.Policies.Application.Queries.GetRenewalComparison;
using IBS.Policies.Domain.Events;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for managing insurance policies.
/// </summary>
[Authorize]
public sealed class PoliciesController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PoliciesController> _logger;

    /// <summary>
    /// Initializes a new instance of the PoliciesController class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public PoliciesController(IMediator mediator, ILogger<PoliciesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets a list of policies with optional filtering.
    /// </summary>
    /// <param name="request">The filter/pagination parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of policies.</returns>
    /// <response code="200">Returns the list of policies.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPolicies(
        [FromQuery] GetPoliciesRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting policies for tenant {TenantId}", CurrentTenantId);

        var query = new GetPoliciesQuery(
            CurrentTenantId,
            request.Search,
            request.ClientId,
            request.CarrierId,
            request.Status,
            request.LineOfBusiness,
            request.EffectiveDateFrom,
            request.EffectiveDateTo,
            request.ExpirationDateFrom,
            request.ExpirationDateTo,
            request.Page,
            request.PageSize,
            request.SortBy ?? "CreatedAt",
            request.SortDirection ?? "desc");

        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a policy by identifier.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The policy details.</returns>
    /// <response code="200">Returns the policy.</response>
    /// <response code="404">If the policy is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("{id:guid}", Name = "GetPolicyById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPolicy(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting policy {PolicyId} for tenant {TenantId}", id, CurrentTenantId);

        var query = new GetPolicyByIdQuery(CurrentTenantId, id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess && result.Value is null)
        {
            return NotFound(new { error = $"Policy with ID '{id}' not found." });
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new policy.
    /// </summary>
    /// <param name="request">The policy creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created policy ID.</returns>
    /// <response code="201">Returns the newly created policy ID.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePolicy(
        [FromBody] CreatePolicyRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating policy for tenant {TenantId}, client {ClientId}", CurrentTenantId, request.ClientId);

        var command = new CreatePolicyCommand(
            CurrentTenantId,
            CurrentUserId,
            request.ClientId,
            request.CarrierId,
            request.LineOfBusiness,
            request.PolicyType,
            request.EffectiveDate,
            request.ExpirationDate,
            request.BillingType,
            request.PaymentPlan,
            request.QuoteId,
            request.Notes);

        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetPolicyById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Binds (commits) a policy.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the policy was bound successfully.</response>
    /// <response code="400">If the policy cannot be bound.</response>
    /// <response code="404">If the policy is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/bind")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BindPolicy(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Binding policy {PolicyId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new BindPolicyCommand(CurrentTenantId, CurrentUserId, id, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Cancels a policy.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="request">The cancellation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the policy was cancelled successfully.</response>
    /// <response code="400">If the policy cannot be cancelled.</response>
    /// <response code="404">If the policy is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelPolicy(
        Guid id,
        [FromBody] CancelPolicyRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling policy {PolicyId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new CancelPolicyCommand(
            CurrentTenantId,
            id,
            request.CancellationDate,
            request.Reason,
            request.CancellationType,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Reinstates a cancelled policy.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="request">The reinstatement request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the policy was reinstated successfully.</response>
    /// <response code="400">If the policy cannot be reinstated.</response>
    /// <response code="404">If the policy is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/reinstate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReinstatePolicy(
        Guid id,
        [FromBody] ReinstatePolicyRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reinstating policy {PolicyId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new ReinstatePolicyCommand(CurrentTenantId, id, request.Reason, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Adds a coverage to a policy.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="request">The coverage details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created coverage ID.</returns>
    /// <response code="201">Returns the newly created coverage ID.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the policy is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/coverages")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddCoverage(
        Guid id,
        [FromBody] AddCoverageRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding coverage to policy {PolicyId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new AddCoverageCommand(
            CurrentTenantId,
            id,
            request.Code,
            request.Name,
            request.PremiumAmount,
            request.Description,
            request.LimitAmount,
            request.DeductibleAmount,
            request.IsOptional,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, new { id = result.Value });
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Adds an endorsement to a policy.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="request">The endorsement details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created endorsement ID.</returns>
    /// <response code="201">Returns the newly created endorsement ID.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the policy is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/endorsements")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddEndorsement(
        Guid id,
        [FromBody] AddEndorsementRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding endorsement to policy {PolicyId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new AddEndorsementCommand(
            CurrentTenantId,
            id,
            request.EffectiveDate,
            request.Type,
            request.Description,
            request.PremiumChange,
            request.Notes,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, new { id = result.Value });
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Activates (issues) a bound policy.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the policy was activated successfully.</response>
    /// <response code="400">If the policy cannot be activated.</response>
    /// <response code="404">If the policy is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ActivatePolicy(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating policy {PolicyId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new ActivatePolicyCommand(CurrentTenantId, id, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a renewal policy from an existing active policy.
    /// </summary>
    /// <param name="id">The policy identifier to renew.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created renewal policy ID.</returns>
    /// <response code="201">Returns the newly created renewal policy ID.</response>
    /// <response code="400">If the policy cannot be renewed.</response>
    /// <response code="404">If the policy is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/renew")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RenewPolicy(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Renewing policy {PolicyId} for tenant {TenantId}", id, CurrentTenantId);

        var command = new RenewPolicyCommand(CurrentTenantId, CurrentUserId, id, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetPolicyById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Updates a coverage on a policy.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="coverageId">The coverage identifier.</param>
    /// <param name="request">The coverage update details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the coverage was updated successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the policy or coverage is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPut("{id:guid}/coverages/{coverageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCoverage(
        Guid id,
        Guid coverageId,
        [FromBody] UpdateCoverageRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating coverage {CoverageId} on policy {PolicyId} for tenant {TenantId}", coverageId, id, CurrentTenantId);

        var command = new UpdateCoverageCommand(
            CurrentTenantId,
            id,
            coverageId,
            request.Name,
            request.PremiumAmount,
            request.Description,
            request.LimitAmount,
            request.PerOccurrenceLimit,
            request.AggregateLimit,
            request.DeductibleAmount,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Removes a coverage from a policy.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="coverageId">The coverage identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the coverage was removed successfully.</response>
    /// <response code="400">If the coverage cannot be removed.</response>
    /// <response code="404">If the policy or coverage is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpDelete("{id:guid}/coverages/{coverageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveCoverage(
        Guid id,
        Guid coverageId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing coverage {CoverageId} from policy {PolicyId} for tenant {TenantId}", coverageId, id, CurrentTenantId);

        var command = new RemoveCoverageCommand(CurrentTenantId, id, coverageId, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Approves a pending endorsement.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="endorsementId">The endorsement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the endorsement was approved successfully.</response>
    /// <response code="400">If the endorsement cannot be approved.</response>
    /// <response code="404">If the policy or endorsement is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/endorsements/{endorsementId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ApproveEndorsement(
        Guid id,
        Guid endorsementId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving endorsement {EndorsementId} on policy {PolicyId} for tenant {TenantId}", endorsementId, id, CurrentTenantId);

        var command = new ApproveEndorsementCommand(CurrentTenantId, CurrentUserId, id, endorsementId, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Issues an approved endorsement and applies premium changes.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="endorsementId">The endorsement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the endorsement was issued successfully.</response>
    /// <response code="400">If the endorsement cannot be issued.</response>
    /// <response code="404">If the policy or endorsement is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/endorsements/{endorsementId:guid}/issue")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IssueEndorsement(
        Guid id,
        Guid endorsementId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Issuing endorsement {EndorsementId} on policy {PolicyId} for tenant {TenantId}", endorsementId, id, CurrentTenantId);

        var command = new IssueEndorsementCommand(CurrentTenantId, id, endorsementId, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Rejects a pending endorsement.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="endorsementId">The endorsement identifier.</param>
    /// <param name="request">The rejection details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the endorsement was rejected successfully.</response>
    /// <response code="400">If the endorsement cannot be rejected.</response>
    /// <response code="404">If the policy or endorsement is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/endorsements/{endorsementId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RejectEndorsement(
        Guid id,
        Guid endorsementId,
        [FromBody] RejectEndorsementRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rejecting endorsement {EndorsementId} on policy {PolicyId} for tenant {TenantId}", endorsementId, id, CurrentTenantId);

        var command = new RejectEndorsementCommand(CurrentTenantId, CurrentUserId, id, endorsementId, request.Reason, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets the renewal offer comparison for a policy.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Renewal comparison with current policy info and carrier offers.</returns>
    /// <response code="200">Returns the renewal comparison.</response>
    /// <response code="404">If the policy is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("{id:guid}/renewal-comparison")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRenewalComparison(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetRenewalComparisonQuery(CurrentTenantId, id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess && result.Value is null)
            return NotFound(new { error = $"Policy with ID '{id}' not found." });

        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a renewal quote linked to this policy.
    /// </summary>
    /// <param name="id">The policy identifier to create a renewal quote for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created quote ID.</returns>
    /// <response code="201">Returns the newly created renewal quote ID.</response>
    /// <response code="400">If the policy cannot have a renewal quote.</response>
    /// <response code="404">If the policy is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/renewal-quote")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateRenewalQuote(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating renewal quote for policy {PolicyId} in tenant {TenantId}", id, CurrentTenantId);

        var command = new CreateRenewalQuoteCommand(CurrentTenantId, CurrentUserId, id);
        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetQuoteById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Gets paginated history entries for a policy.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paginated history entries.</returns>
    /// <response code="200">Returns the history entries.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPolicyHistory(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPolicyHistoryQuery(CurrentTenantId, id, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets policies expiring within a date range.
    /// </summary>
    /// <param name="request">The expiring policies request parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of expiring policies.</returns>
    /// <response code="200">Returns the list of expiring policies.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("expiring")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetExpiringPolicies(
        [FromQuery] GetExpiringPoliciesRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting expiring policies for tenant {TenantId} between {StartDate} and {EndDate}",
            CurrentTenantId, request.StartDate, request.EndDate);

        var query = new GetExpiringPoliciesQuery(
            CurrentTenantId,
            request.StartDate,
            request.EndDate,
            request.Page,
            request.PageSize);

        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets policies due for renewal.
    /// </summary>
    /// <param name="request">The due for renewal request parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of policies due for renewal.</returns>
    /// <response code="200">Returns the list of policies due for renewal.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("due-for-renewal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPoliciesDueForRenewal(
        [FromQuery] GetPoliciesDueForRenewalRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting policies due for renewal for tenant {TenantId} within {Days} days",
            CurrentTenantId, request.DaysUntilExpiration);

        var query = new GetPoliciesDueForRenewalQuery(
            CurrentTenantId,
            request.DaysUntilExpiration,
            request.Page,
            request.PageSize);

        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets all policies for a specific client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="request">The pagination parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of policies for the client.</returns>
    /// <response code="200">Returns the list of client policies.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("~/api/v1/clients/{clientId:guid}/policies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPoliciesByClient(
        Guid clientId,
        [FromQuery] GetPoliciesByClientRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting policies for client {ClientId} for tenant {TenantId}", clientId, CurrentTenantId);

        var query = new GetPoliciesByClientQuery(
            CurrentTenantId,
            clientId,
            request.Page,
            request.PageSize,
            request.SortBy ?? "CreatedAt",
            request.SortDirection ?? "desc");

        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }
}

/// <summary>
/// Request model for listing policies with filtering.
/// </summary>
public sealed record GetPoliciesRequest
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
    /// Gets or sets the carrier ID filter.
    /// </summary>
    [FromQuery(Name = "carrierId")]
    public Guid? CarrierId { get; set; }

    /// <summary>
    /// Gets or sets the status filter.
    /// </summary>
    [FromQuery(Name = "status")]
    public PolicyStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the line of business filter.
    /// </summary>
    [FromQuery(Name = "lineOfBusiness")]
    public LineOfBusiness? LineOfBusiness { get; set; }

    /// <summary>
    /// Gets or sets the effective date from filter.
    /// </summary>
    [FromQuery(Name = "effectiveDateFrom")]
    public DateOnly? EffectiveDateFrom { get; set; }

    /// <summary>
    /// Gets or sets the effective date to filter.
    /// </summary>
    [FromQuery(Name = "effectiveDateTo")]
    public DateOnly? EffectiveDateTo { get; set; }

    /// <summary>
    /// Gets or sets the expiration date from filter.
    /// </summary>
    [FromQuery(Name = "expirationDateFrom")]
    public DateOnly? ExpirationDateFrom { get; set; }

    /// <summary>
    /// Gets or sets the expiration date to filter.
    /// </summary>
    [FromQuery(Name = "expirationDateTo")]
    public DateOnly? ExpirationDateTo { get; set; }

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
/// Request model for creating a policy.
/// </summary>
public sealed record CreatePolicyRequest
{
    /// <summary>
    /// Gets or sets the client ID.
    /// </summary>
    public Guid ClientId { get; init; }

    /// <summary>
    /// Gets or sets the carrier ID.
    /// </summary>
    public Guid CarrierId { get; init; }

    /// <summary>
    /// Gets or sets the line of business.
    /// </summary>
    public LineOfBusiness LineOfBusiness { get; init; }

    /// <summary>
    /// Gets or sets the policy type/product name.
    /// </summary>
    public string PolicyType { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the effective date.
    /// </summary>
    public DateOnly EffectiveDate { get; init; }

    /// <summary>
    /// Gets or sets the expiration date.
    /// </summary>
    public DateOnly ExpirationDate { get; init; }

    /// <summary>
    /// Gets or sets the billing type.
    /// </summary>
    public BillingType BillingType { get; init; } = BillingType.DirectBill;

    /// <summary>
    /// Gets or sets the payment plan.
    /// </summary>
    public PaymentPlan PaymentPlan { get; init; } = PaymentPlan.Annual;

    /// <summary>
    /// Gets or sets the quote ID (optional).
    /// </summary>
    public Guid? QuoteId { get; init; }

    /// <summary>
    /// Gets or sets the notes (optional).
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Request model for cancelling a policy.
/// </summary>
public sealed record CancelPolicyRequest
{
    /// <summary>
    /// Gets or sets the cancellation date.
    /// </summary>
    public DateOnly CancellationDate { get; init; }

    /// <summary>
    /// Gets or sets the cancellation reason.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the cancellation type.
    /// </summary>
    public CancellationType CancellationType { get; init; }
}

/// <summary>
/// Request model for adding a coverage to a policy.
/// </summary>
public sealed record AddCoverageRequest
{
    /// <summary>
    /// Gets or sets the coverage code.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the coverage name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the premium amount.
    /// </summary>
    public decimal PremiumAmount { get; init; }

    /// <summary>
    /// Gets or sets the description (optional).
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the limit amount (optional).
    /// </summary>
    public decimal? LimitAmount { get; init; }

    /// <summary>
    /// Gets or sets the deductible amount (optional).
    /// </summary>
    public decimal? DeductibleAmount { get; init; }

    /// <summary>
    /// Gets or sets whether this coverage is optional.
    /// </summary>
    public bool IsOptional { get; init; }
}

/// <summary>
/// Request model for adding an endorsement to a policy.
/// </summary>
public sealed record AddEndorsementRequest
{
    /// <summary>
    /// Gets or sets the effective date.
    /// </summary>
    public DateOnly EffectiveDate { get; init; }

    /// <summary>
    /// Gets or sets the endorsement type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the premium change (positive for increase, negative for decrease).
    /// </summary>
    public decimal PremiumChange { get; init; }

    /// <summary>
    /// Gets or sets the notes (optional).
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Request model for updating a coverage.
/// </summary>
public sealed record UpdateCoverageRequest
{
    /// <summary>
    /// Gets or sets the coverage name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the premium amount.
    /// </summary>
    public decimal PremiumAmount { get; init; }

    /// <summary>
    /// Gets or sets the description (optional).
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the limit amount (optional).
    /// </summary>
    public decimal? LimitAmount { get; init; }

    /// <summary>
    /// Gets or sets the per-occurrence limit (optional).
    /// </summary>
    public decimal? PerOccurrenceLimit { get; init; }

    /// <summary>
    /// Gets or sets the aggregate limit (optional).
    /// </summary>
    public decimal? AggregateLimit { get; init; }

    /// <summary>
    /// Gets or sets the deductible amount (optional).
    /// </summary>
    public decimal? DeductibleAmount { get; init; }
}

/// <summary>
/// Request model for rejecting an endorsement.
/// </summary>
public sealed record RejectEndorsementRequest
{
    /// <summary>
    /// Gets or sets the rejection reason.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Request model for reinstating a cancelled policy.
/// </summary>
public sealed record ReinstatePolicyRequest
{
    /// <summary>
    /// Gets or sets the reinstatement reason.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Request model for getting expiring policies.
/// </summary>
public sealed record GetExpiringPoliciesRequest
{
    /// <summary>
    /// Gets or sets the start date of the expiration range.
    /// </summary>
    [FromQuery(Name = "startDate")]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the expiration range.
    /// </summary>
    [FromQuery(Name = "endDate")]
    public DateOnly EndDate { get; set; }

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

/// <summary>
/// Request model for getting policies due for renewal.
/// </summary>
public sealed record GetPoliciesDueForRenewalRequest
{
    /// <summary>
    /// Gets or sets the number of days until expiration (default 60).
    /// </summary>
    [FromQuery(Name = "daysUntilExpiration")]
    public int DaysUntilExpiration { get; set; } = 60;

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

/// <summary>
/// Request model for getting policies by client.
/// </summary>
public sealed record GetPoliciesByClientRequest
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
