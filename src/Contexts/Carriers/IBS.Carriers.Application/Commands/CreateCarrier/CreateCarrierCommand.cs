using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Carriers.Application.Commands.CreateCarrier;

/// <summary>
/// Command to create a new carrier.
/// </summary>
/// <param name="Name">The carrier name.</param>
/// <param name="Code">The carrier code (2-10 alphanumeric characters).</param>
/// <param name="LegalName">The legal name (optional).</param>
/// <param name="AmBestRating">The A.M. Best rating (optional).</param>
/// <param name="NaicCode">The NAIC code (optional).</param>
/// <param name="WebsiteUrl">The website URL (optional).</param>
/// <param name="Notes">Additional notes (optional).</param>
public sealed record CreateCarrierCommand(
    string Name,
    string Code,
    string? LegalName = null,
    string? AmBestRating = null,
    string? NaicCode = null,
    string? WebsiteUrl = null,
    string? Notes = null) : ICommand<Guid>;
