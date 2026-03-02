using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Documents.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IBS.Documents.Application.Commands.DeleteDocument;

/// <summary>
/// Handler for the DeleteDocumentCommand.
/// </summary>
public sealed class DeleteDocumentCommandHandler(
    IDocumentRepository documentRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteDocumentCommandHandler> logger) : ICommandHandler<DeleteDocumentCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null || document.TenantId != request.TenantId)
            return Error.NotFound("Document not found.");

        try
        {
            document.Archive();
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation(ex.Message);
        }

        await documentRepository.UpdateAsync(document, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[Audit] Document {DocumentId} archived in tenant {TenantId}",
            request.DocumentId, request.TenantId);
        return Result.Success();
    }
}
