using IBS.BuildingBlocks.Application.Commands;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.Commands.AddAppetite;

/// <summary>
/// Command to add an appetite rule to a carrier.
/// </summary>
/// <param name="CarrierId">The carrier identifier.</param>
/// <param name="LineOfBusiness">The line of business.</param>
/// <param name="States">The states covered (comma-separated, or "ALL").</param>
/// <param name="MinYearsInBusiness">Minimum years in business (optional).</param>
/// <param name="MaxYearsInBusiness">Maximum years in business (optional).</param>
/// <param name="MinAnnualRevenue">Minimum annual revenue (optional).</param>
/// <param name="MaxAnnualRevenue">Maximum annual revenue (optional).</param>
/// <param name="MinEmployees">Minimum number of employees (optional).</param>
/// <param name="MaxEmployees">Maximum number of employees (optional).</param>
/// <param name="AcceptedIndustries">Accepted industries (optional).</param>
/// <param name="ExcludedIndustries">Excluded industries (optional).</param>
/// <param name="Notes">Additional notes (optional).</param>
public sealed record AddAppetiteCommand(
    Guid CarrierId,
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
    string? Notes = null,
    string? ExpectedRowVersion = null) : ICommand<Guid>;
