using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.CreateBusinessClient;

/// <summary>
/// Command to create a new business client.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The creating user identifier.</param>
/// <param name="BusinessName">The legal business name.</param>
/// <param name="BusinessType">The business type (LLC, Corporation, etc.).</param>
/// <param name="DbaName">The DBA name (optional).</param>
/// <param name="Industry">The industry classification (optional).</param>
/// <param name="YearEstablished">The year established (optional).</param>
/// <param name="NumberOfEmployees">The number of employees (optional).</param>
/// <param name="AnnualRevenue">The annual revenue (optional).</param>
/// <param name="Website">The website URL (optional).</param>
/// <param name="Email">The email address (optional).</param>
/// <param name="Phone">The phone number (optional).</param>
public sealed record CreateBusinessClientCommand(
    Guid TenantId,
    Guid UserId,
    string BusinessName,
    string BusinessType,
    string? DbaName = null,
    string? Industry = null,
    int? YearEstablished = null,
    int? NumberOfEmployees = null,
    decimal? AnnualRevenue = null,
    string? Website = null,
    string? Email = null,
    string? Phone = null) : ICommand<Guid>;
