using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.UpdateClient;

/// <summary>
/// Command to update a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="FirstName">The first name (for individual clients).</param>
/// <param name="LastName">The last name (for individual clients).</param>
/// <param name="MiddleName">The middle name (for individual clients, optional).</param>
/// <param name="Suffix">The name suffix (for individual clients, optional).</param>
/// <param name="BusinessName">The business name (for business clients).</param>
/// <param name="BusinessType">The business type (for business clients).</param>
/// <param name="DbaName">The DBA name (for business clients, optional).</param>
/// <param name="Industry">The industry (for business clients, optional).</param>
/// <param name="YearEstablished">The year established (for business clients, optional).</param>
/// <param name="NumberOfEmployees">The number of employees (for business clients, optional).</param>
/// <param name="AnnualRevenue">The annual revenue (for business clients, optional).</param>
/// <param name="Website">The website (for business clients, optional).</param>
/// <param name="Email">The email address (optional).</param>
/// <param name="Phone">The phone number (optional).</param>
public sealed record UpdateClientCommand(
    Guid ClientId,
    string? FirstName = null,
    string? LastName = null,
    string? MiddleName = null,
    string? Suffix = null,
    string? BusinessName = null,
    string? BusinessType = null,
    string? DbaName = null,
    string? Industry = null,
    int? YearEstablished = null,
    int? NumberOfEmployees = null,
    decimal? AnnualRevenue = null,
    string? Website = null,
    string? Email = null,
    string? Phone = null) : ICommand;
