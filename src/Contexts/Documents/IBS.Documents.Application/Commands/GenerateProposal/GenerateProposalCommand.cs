using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Documents.Application.Commands.GenerateProposal;

/// <summary>
/// Command to generate a proposal PDF from a quote and store it as a document.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The user generating the proposal.</param>
/// <param name="TemplateId">The proposal template identifier.</param>
/// <param name="QuoteId">The quote identifier to generate the proposal for.</param>
public sealed record GenerateProposalCommand(
    Guid TenantId,
    string UserId,
    Guid TemplateId,
    Guid QuoteId) : ICommand<Guid>;
