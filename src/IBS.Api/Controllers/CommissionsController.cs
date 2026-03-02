using IBS.Commissions.Application.Commands.AddLineItem;
using IBS.Commissions.Application.Commands.AddProducerSplit;
using IBS.Commissions.Application.Commands.CreateSchedule;
using IBS.Commissions.Application.Commands.CreateStatement;
using IBS.Commissions.Application.Commands.DeactivateSchedule;
using IBS.Commissions.Application.Commands.DisputeLineItem;
using IBS.Commissions.Application.Commands.ReconcileLineItem;
using IBS.Commissions.Application.Commands.UpdateSchedule;
using IBS.Commissions.Application.Commands.UpdateStatementStatus;
using IBS.Commissions.Application.Queries.GetCommissionStatistics;
using IBS.Commissions.Application.Queries.GetCommissionSummaryReport;
using IBS.Commissions.Application.Queries.GetProducerReport;
using IBS.Commissions.Application.Queries.GetScheduleById;
using IBS.Commissions.Application.Queries.GetSchedules;
using IBS.Commissions.Application.Queries.GetStatementById;
using IBS.Commissions.Application.Queries.GetStatements;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for managing commissions, schedules, statements, and reports.
/// </summary>
[Authorize]
public sealed class CommissionsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CommissionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the CommissionsController class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public CommissionsController(IMediator mediator, ILogger<CommissionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // ── Schedules ──────────────────────────────────────────────────────

    /// <summary>
    /// Gets a paginated list of commission schedules with optional filtering.
    /// </summary>
    [HttpGet("schedules")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchedules(
        [FromQuery] GetSchedulesRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetSchedulesQuery(
            CurrentTenantId,
            request.Search,
            request.CarrierId,
            request.LineOfBusiness,
            request.IsActive,
            request.Page,
            request.PageSize,
            request.SortBy ?? "CreatedAt",
            request.SortDirection ?? "desc");

        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a commission schedule by its identifier.
    /// </summary>
    [HttpGet("schedules/{id:guid}", Name = "GetScheduleById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScheduleById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetScheduleByIdQuery(CurrentTenantId, id);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new commission schedule.
    /// </summary>
    [HttpPost("schedules")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSchedule(
        [FromBody] CreateScheduleRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating commission schedule for carrier {CarrierId}", request.CarrierId);

        var command = new CreateScheduleCommand(
            CurrentTenantId,
            request.CarrierId,
            request.CarrierName,
            request.LineOfBusiness,
            request.NewBusinessRate,
            request.RenewalRate,
            request.EffectiveFrom,
            request.EffectiveTo);

        var result = await _mediator.Send(command, cancellationToken);
        return ToCreatedResult(result, "GetScheduleById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Updates an existing commission schedule.
    /// </summary>
    [HttpPut("schedules/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSchedule(
        Guid id,
        [FromBody] UpdateScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateScheduleCommand(
            CurrentTenantId,
            id,
            request.CarrierName,
            request.LineOfBusiness,
            request.NewBusinessRate,
            request.RenewalRate,
            request.EffectiveFrom,
            request.EffectiveTo);

        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Deactivates a commission schedule.
    /// </summary>
    [HttpPost("schedules/{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateSchedule(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeactivateScheduleCommand(CurrentTenantId, id);
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    // ── Statements ─────────────────────────────────────────────────────

    /// <summary>
    /// Gets a paginated list of commission statements with optional filtering.
    /// </summary>
    [HttpGet("statements")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatements(
        [FromQuery] GetStatementsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetStatementsQuery(
            CurrentTenantId,
            request.Search,
            request.CarrierId,
            request.Status,
            request.PeriodMonth,
            request.PeriodYear,
            request.Page,
            request.PageSize,
            request.SortBy ?? "CreatedAt",
            request.SortDirection ?? "desc");

        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a commission statement by its identifier with line items and splits.
    /// </summary>
    [HttpGet("statements/{id:guid}", Name = "GetStatementById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatementById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetStatementByIdQuery(CurrentTenantId, id);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new commission statement.
    /// </summary>
    [HttpPost("statements")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStatement(
        [FromBody] CreateStatementRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating commission statement for carrier {CarrierId}", request.CarrierId);

        var command = new CreateStatementCommand(
            CurrentTenantId,
            request.CarrierId,
            request.CarrierName,
            request.StatementNumber,
            request.PeriodMonth,
            request.PeriodYear,
            request.StatementDate,
            request.TotalPremium,
            request.TotalPremiumCurrency,
            request.TotalCommission,
            request.TotalCommissionCurrency);

        var result = await _mediator.Send(command, cancellationToken);
        return ToCreatedResult(result, "GetStatementById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Adds a line item to a commission statement.
    /// </summary>
    [HttpPost("statements/{id:guid}/line-items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddLineItem(
        Guid id,
        [FromBody] AddLineItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddLineItemCommand(
            CurrentTenantId,
            id,
            request.PolicyNumber,
            request.InsuredName,
            request.LineOfBusiness,
            request.EffectiveDate,
            request.TransactionType,
            request.GrossPremium,
            request.GrossPremiumCurrency,
            request.CommissionRate,
            request.CommissionAmount,
            request.CommissionAmountCurrency,
            request.PolicyId,
            ExpectedRowVersion);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(new { id = result.Value });

        return ToActionResult(result);
    }

    /// <summary>
    /// Reconciles a line item on a commission statement.
    /// </summary>
    [HttpPut("statements/{id:guid}/line-items/{lineItemId:guid}/reconcile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReconcileLineItem(
        Guid id,
        Guid lineItemId,
        CancellationToken cancellationToken)
    {
        var command = new ReconcileLineItemCommand(CurrentTenantId, id, lineItemId, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Disputes a line item on a commission statement.
    /// </summary>
    [HttpPost("statements/{id:guid}/line-items/{lineItemId:guid}/dispute")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisputeLineItem(
        Guid id,
        Guid lineItemId,
        [FromBody] DisputeLineItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new DisputeLineItemCommand(CurrentTenantId, id, lineItemId, request.Reason, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Adds a producer split to a line item.
    /// </summary>
    [HttpPost("statements/{id:guid}/line-items/{lineItemId:guid}/splits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProducerSplit(
        Guid id,
        Guid lineItemId,
        [FromBody] AddProducerSplitRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddProducerSplitCommand(
            CurrentTenantId,
            id,
            lineItemId,
            request.ProducerName,
            request.ProducerId,
            request.SplitPercentage,
            request.SplitAmount,
            request.SplitAmountCurrency);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(new { id = result.Value });

        return ToActionResult(result);
    }

    /// <summary>
    /// Updates the status of a commission statement.
    /// </summary>
    [HttpPut("statements/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatementStatus(
        Guid id,
        [FromBody] UpdateStatementStatusRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating statement {StatementId} status to {Status}", id, request.NewStatus);

        var command = new UpdateStatementStatusCommand(CurrentTenantId, id, request.NewStatus, ExpectedRowVersion);
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    // ── Statistics & Reports ───────────────────────────────────────────

    /// <summary>
    /// Gets commission statistics for the dashboard.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var query = new GetCommissionStatisticsQuery(CurrentTenantId);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets commission summary report by carrier and period.
    /// </summary>
    [HttpGet("reports/summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummaryReport(
        [FromQuery] Guid? carrierId,
        [FromQuery] int? periodMonth,
        [FromQuery] int? periodYear,
        CancellationToken cancellationToken)
    {
        var query = new GetCommissionSummaryReportQuery(CurrentTenantId, carrierId, periodMonth, periodYear);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets producer report by producer and period.
    /// </summary>
    [HttpGet("reports/producer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducerReport(
        [FromQuery] Guid? producerId,
        [FromQuery] int? periodMonth,
        [FromQuery] int? periodYear,
        CancellationToken cancellationToken)
    {
        var query = new GetProducerReportQuery(CurrentTenantId, producerId, periodMonth, periodYear);
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }
}

// ── Request Records ────────────────────────────────────────────────────

/// <summary>
/// Request for getting schedules with filtering.
/// </summary>
public sealed record GetSchedulesRequest(
    string? Search = null,
    Guid? CarrierId = null,
    string? LineOfBusiness = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDirection = null
);

/// <summary>
/// Request for creating a commission schedule.
/// </summary>
public sealed record CreateScheduleRequest(
    Guid CarrierId,
    string CarrierName,
    string LineOfBusiness,
    decimal NewBusinessRate,
    decimal RenewalRate,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo = null
);

/// <summary>
/// Request for updating a commission schedule.
/// </summary>
public sealed record UpdateScheduleRequest(
    string CarrierName,
    string LineOfBusiness,
    decimal NewBusinessRate,
    decimal RenewalRate,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo = null
);

/// <summary>
/// Request for getting statements with filtering.
/// </summary>
public sealed record GetStatementsRequest(
    string? Search = null,
    Guid? CarrierId = null,
    StatementStatus? Status = null,
    int? PeriodMonth = null,
    int? PeriodYear = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDirection = null
);

/// <summary>
/// Request for creating a commission statement.
/// </summary>
public sealed record CreateStatementRequest(
    Guid CarrierId,
    string CarrierName,
    string StatementNumber,
    int PeriodMonth,
    int PeriodYear,
    DateOnly StatementDate,
    decimal TotalPremium,
    string TotalPremiumCurrency,
    decimal TotalCommission,
    string TotalCommissionCurrency
);

/// <summary>
/// Request for adding a line item.
/// </summary>
public sealed record AddLineItemRequest(
    string PolicyNumber,
    string InsuredName,
    string LineOfBusiness,
    DateOnly EffectiveDate,
    TransactionType TransactionType,
    decimal GrossPremium,
    string GrossPremiumCurrency,
    decimal CommissionRate,
    decimal CommissionAmount,
    string CommissionAmountCurrency,
    Guid? PolicyId = null
);

/// <summary>
/// Request for disputing a line item.
/// </summary>
public sealed record DisputeLineItemRequest(
    string Reason
);

/// <summary>
/// Request for adding a producer split.
/// </summary>
public sealed record AddProducerSplitRequest(
    string ProducerName,
    Guid ProducerId,
    decimal SplitPercentage,
    decimal SplitAmount,
    string SplitAmountCurrency
);

/// <summary>
/// Request for updating statement status.
/// </summary>
public sealed record UpdateStatementStatusRequest(
    StatementStatus NewStatus
);
