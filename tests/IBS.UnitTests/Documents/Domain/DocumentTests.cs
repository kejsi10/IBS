using FluentAssertions;
using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Events;

namespace IBS.UnitTests.Documents.Domain;

/// <summary>
/// Unit tests for the Document aggregate.
/// </summary>
public class DocumentTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private const string FileName = "test-document.pdf";
    private const string ContentType = "application/pdf";
    private const long FileSizeBytes = 204800L;
    private const string BlobKey = "tenant/Policy/uuid/test-document.pdf";
    private const string UploadedBy = "user-123";

    [Fact]
    public void Create_WithValidInputs_CreatesDocument()
    {
        // Arrange & Act
        var document = Document.Create(
            TenantId,
            DocumentEntityType.Policy,
            Guid.NewGuid(),
            FileName,
            ContentType,
            FileSizeBytes,
            BlobKey,
            DocumentCategory.Policy,
            UploadedBy);

        // Assert
        document.Id.Should().NotBeEmpty();
        document.TenantId.Should().Be(TenantId);
        document.FileName.Should().Be(FileName);
        document.ContentType.Should().Be(ContentType);
        document.FileSizeBytes.Should().Be(FileSizeBytes);
        document.BlobKey.Should().Be(BlobKey);
        document.Category.Should().Be(DocumentCategory.Policy);
        document.Version.Should().Be(1);
        document.IsArchived.Should().BeFalse();
        document.UploadedBy.Should().Be(UploadedBy);
    }

    [Fact]
    public void Create_RaisesDocumentUploadedEvent()
    {
        // Arrange & Act
        var document = Document.Create(
            TenantId,
            DocumentEntityType.Policy,
            null,
            FileName,
            ContentType,
            FileSizeBytes,
            BlobKey,
            DocumentCategory.Policy,
            UploadedBy);

        // Assert
        document.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DocumentUploadedEvent>();
    }

    [Fact]
    public void Create_WithNullEntityId_Succeeds()
    {
        // Arrange & Act
        var document = Document.Create(
            TenantId,
            DocumentEntityType.General,
            null,
            FileName,
            ContentType,
            FileSizeBytes,
            BlobKey,
            DocumentCategory.Other,
            UploadedBy);

        // Assert
        document.EntityId.Should().BeNull();
        document.EntityType.Should().Be(DocumentEntityType.General);
    }

    [Fact]
    public void Create_WithEmptyFileName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Document.Create(
            TenantId,
            DocumentEntityType.Policy,
            null,
            string.Empty,
            ContentType,
            FileSizeBytes,
            BlobKey,
            DocumentCategory.Policy,
            UploadedBy);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("fileName");
    }

    [Fact]
    public void Create_WithEmptyContentType_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Document.Create(
            TenantId,
            DocumentEntityType.Policy,
            null,
            FileName,
            string.Empty,
            FileSizeBytes,
            BlobKey,
            DocumentCategory.Policy,
            UploadedBy);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("contentType");
    }

    [Fact]
    public void Create_WithZeroFileSize_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Document.Create(
            TenantId,
            DocumentEntityType.Policy,
            null,
            FileName,
            ContentType,
            0L,
            BlobKey,
            DocumentCategory.Policy,
            UploadedBy);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("fileSizeBytes");
    }

    [Fact]
    public void Create_WithEmptyBlobKey_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Document.Create(
            TenantId,
            DocumentEntityType.Policy,
            null,
            FileName,
            ContentType,
            FileSizeBytes,
            string.Empty,
            DocumentCategory.Policy,
            UploadedBy);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("blobKey");
    }

    [Fact]
    public void Archive_SetsIsArchivedTrue()
    {
        // Arrange
        var document = Document.Create(TenantId, DocumentEntityType.Policy, null,
            FileName, ContentType, FileSizeBytes, BlobKey, DocumentCategory.Policy, UploadedBy);
        document.ClearDomainEvents();

        // Act
        document.Archive();

        // Assert
        document.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Archive_RaisesDocumentArchivedEvent()
    {
        // Arrange
        var document = Document.Create(TenantId, DocumentEntityType.Policy, null,
            FileName, ContentType, FileSizeBytes, BlobKey, DocumentCategory.Policy, UploadedBy);
        document.ClearDomainEvents();

        // Act
        document.Archive();

        // Assert
        document.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DocumentArchivedEvent>();
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ThrowsInvalidOperationException()
    {
        // Arrange
        var document = Document.Create(TenantId, DocumentEntityType.Policy, null,
            FileName, ContentType, FileSizeBytes, BlobKey, DocumentCategory.Policy, UploadedBy);
        document.Archive();

        // Act
        var act = () => document.Archive();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already archived*");
    }
}
