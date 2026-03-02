using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Repositories;
using IBS.Documents.Application.Services;

namespace IBS.Documents.Application.Commands.UploadDocument;

/// <summary>
/// Handler for the UploadDocumentCommand.
/// </summary>
public sealed class UploadDocumentCommandHandler(
    IDocumentRepository documentRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork) : ICommandHandler<UploadDocumentCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var blobKey = $"{request.TenantId}/{request.Category}/{Guid.NewGuid()}/{request.FileName}";

        try
        {
            await blobStorageService.UploadAsync(blobKey, request.FileContent, request.ContentType, cancellationToken);
        }
        catch (Exception ex)
        {
            return Error.Internal($"Failed to upload file to storage: {ex.Message}");
        }

        Document document;
        try
        {
            document = Document.Create(
                request.TenantId,
                request.EntityType,
                request.EntityId,
                request.FileName,
                request.ContentType,
                request.FileSizeBytes,
                blobKey,
                request.Category,
                request.UserId,
                request.Description);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        await documentRepository.AddAsync(document, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return document.Id;
    }
}
