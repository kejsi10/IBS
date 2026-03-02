using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.Documents.Application.Services;
using IBS.Documents.Domain.Repositories;

namespace IBS.Documents.Application.Commands.EditTemplateWithAI;

/// <summary>
/// Fetches the requested template, sends it and the instruction to the AI editing service,
/// and returns the original + modified versions for user review.
/// Does not persist the change — the user must save it explicitly.
/// </summary>
public sealed class EditTemplateWithAICommandHandler(
    IDocumentTemplateRepository templateRepository,
    ITemplateEditingService templateEditingService) : ICommandHandler<EditTemplateWithAICommand, EditTemplateWithAIResult>
{
    /// <inheritdoc />
    public async Task<Result<EditTemplateWithAIResult>> Handle(
        EditTemplateWithAICommand request,
        CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null || template.TenantId != request.TenantId)
            return Error.NotFound("Template not found.");

        if (string.IsNullOrWhiteSpace(request.Instruction))
            return Error.Validation("Instruction cannot be empty.");

        string modifiedContent;
        try
        {
            modifiedContent = await templateEditingService.EditTemplateAsync(
                template.Content,
                request.Instruction,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return Error.Internal($"AI edit failed: {ex.Message}");
        }

        return new EditTemplateWithAIResult(template.Content, modifiedContent);
    }
}
