using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;
using IBS.PolicyAssistant.Domain.Repositories;

namespace IBS.PolicyAssistant.Application.Commands.ImportReferenceDocument;

/// <summary>
/// Handler for the <see cref="ImportReferenceDocumentCommand"/>.
/// Creates the document and generates text chunks for full-text search.
/// </summary>
public sealed class ImportReferenceDocumentCommandHandler(
    IReferenceDocumentRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<ImportReferenceDocumentCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(ImportReferenceDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = ReferenceDocument.Create(
            request.Title,
            request.Category,
            request.Content,
            request.TenantId,
            request.LineOfBusiness,
            request.State,
            request.Source);

        // Generate text chunks for full-text search
        document.CreateChunks();

        await repository.AddAsync(document, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return document.Id;
    }
}
