using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Documents.Application.Commands.GenerateCOI;

/// <summary>
/// Command to generate a Certificate of Insurance (COI) PDF.
/// </summary>
public sealed record GenerateCOICommand(
    Guid TenantId,
    string UserId,
    Guid TemplateId,
    Guid PolicyId
) : ICommand<Guid>;
