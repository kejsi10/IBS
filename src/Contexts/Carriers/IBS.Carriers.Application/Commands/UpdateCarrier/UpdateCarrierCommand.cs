using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Carriers.Application.Commands.UpdateCarrier;

/// <summary>
/// Command to update an existing carrier.
/// </summary>
/// <param name="CarrierId">The carrier identifier.</param>
/// <param name="Name">The carrier name.</param>
/// <param name="LegalName">The legal name (optional).</param>
/// <param name="AmBestRating">The A.M. Best rating (optional).</param>
/// <param name="NaicCode">The NAIC code (optional).</param>
/// <param name="WebsiteUrl">The website URL (optional).</param>
/// <param name="ApiEndpoint">The API endpoint (optional).</param>
/// <param name="Notes">Additional notes (optional).</param>
public sealed record UpdateCarrierCommand(
    Guid CarrierId,
    string Name,
    string? LegalName = null,
    string? AmBestRating = null,
    string? NaicCode = null,
    string? WebsiteUrl = null,
    string? ApiEndpoint = null,
    string? Notes = null,
    string? ExpectedRowVersion = null) : ICommand;
