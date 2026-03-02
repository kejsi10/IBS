using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Documents.Application.Commands.EditTemplateWithAI;

/// <summary>
/// Command to apply a natural language instruction to an existing template using an AI model.
/// Returns both the original and modified HTML for side-by-side preview without saving.
/// </summary>
public sealed record EditTemplateWithAICommand(
    Guid TenantId,
    Guid TemplateId,
    string Instruction
) : ICommand<EditTemplateWithAIResult>;
