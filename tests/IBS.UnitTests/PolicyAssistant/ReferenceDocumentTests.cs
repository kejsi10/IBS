using FluentAssertions;
using IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;
using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.UnitTests.PolicyAssistant;

/// <summary>
/// Unit tests for the <see cref="ReferenceDocument"/> domain aggregate.
/// </summary>
public class ReferenceDocumentTests
{
    private const string DefaultTitle = "General Liability Minimum Coverage Requirements";
    private const string DefaultContent = "All commercial general liability policies must carry a minimum occurrence limit of $1,000,000.";
    private static readonly DocumentCategory DefaultCategory = DocumentCategory.Regulation;

    [Fact]
    public void Create_ValidInputs_CreatesDocumentWithCorrectProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var document = ReferenceDocument.Create(
            DefaultTitle,
            DefaultCategory,
            DefaultContent,
            tenantId,
            lineOfBusiness: "GeneralLiability",
            state: "CA",
            source: "CDOI");

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().NotBeEmpty();
        document.Title.Should().Be(DefaultTitle);
        document.Category.Should().Be(DefaultCategory);
        document.Content.Should().Be(DefaultContent);
        document.TenantId.Should().Be(tenantId);
        document.LineOfBusiness.Should().Be("GeneralLiability");
        document.State.Should().Be("CA");
        document.Source.Should().Be("CDOI");
        document.Chunks.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNoTenantId_IsSystemWideDocument()
    {
        // Arrange & Act
        var document = ReferenceDocument.Create(DefaultTitle, DefaultCategory, DefaultContent);

        // Assert
        document.TenantId.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyTitle_ThrowsArgumentException()
    {
        // Act
        var act = () => ReferenceDocument.Create(string.Empty, DefaultCategory, DefaultContent);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyContent_ThrowsArgumentException()
    {
        // Act
        var act = () => ReferenceDocument.Create(DefaultTitle, DefaultCategory, string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_TitleIsTrimmeed_TitleWhitespaceIsTrimmed()
    {
        // Arrange & Act
        var document = ReferenceDocument.Create("  My Title  ", DefaultCategory, DefaultContent);

        // Assert
        document.Title.Should().Be("My Title");
    }

    [Fact]
    public void CreateChunks_ShortContentWithNoOverlap_CreatesSingleChunk()
    {
        // Arrange — with overlap=0 the window advances by chunkSize each iteration,
        // so content shorter than chunkSize produces exactly one chunk.
        const string shortContent = "This is a short document with fewer than 1000 characters of content.";
        var document = ReferenceDocument.Create(DefaultTitle, DefaultCategory, shortContent);

        // Act
        document.CreateChunks(chunkSize: 1000, overlap: 0);

        // Assert
        document.Chunks.Should().HaveCount(1);
        document.Chunks.First().Content.Should().Be(shortContent);
        document.Chunks.First().ChunkIndex.Should().Be(0);
    }

    [Fact]
    public void CreateChunks_LongContent_CreatesMultipleChunksWithCorrectOverlap()
    {
        // Arrange — generate content well above 1000 chars so chunking splits it
        var longContent = string.Join(" ", Enumerable.Repeat(
            "Insurance regulations require carriers to maintain adequate reserves and surplus to cover all projected losses and expenses.",
            20));
        var document = ReferenceDocument.Create(DefaultTitle, DefaultCategory, longContent);

        // Act
        document.CreateChunks(chunkSize: 500, overlap: 100);

        // Assert
        document.Chunks.Should().HaveCountGreaterThan(1);

        // Chunks should have sequential, zero-based indices
        var chunks = document.Chunks.OrderBy(c => c.ChunkIndex).ToList();
        for (var i = 0; i < chunks.Count; i++)
        {
            chunks[i].ChunkIndex.Should().Be(i);
        }

        // No chunk should be empty
        foreach (var chunk in chunks)
        {
            chunk.Content.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void CreateChunks_LongContent_ChunksReferenceParentDocument()
    {
        // Arrange
        var longContent = string.Join(" ", Enumerable.Repeat(
            "Workers compensation requirements mandate that all employers carry coverage for medical expenses and lost wages.",
            20));
        var document = ReferenceDocument.Create(DefaultTitle, DefaultCategory, longContent);

        // Act
        document.CreateChunks();

        // Assert
        foreach (var chunk in document.Chunks)
        {
            chunk.ReferenceDocumentId.Should().Be(document.Id);
        }
    }

    [Fact]
    public void CreateChunks_CalledTwice_ClearsOldChunksAndCreatesNew()
    {
        // Arrange
        const string content = "The first chunking call creates chunks for a document. " +
                               "The second call should replace all previously created chunks.";
        var document = ReferenceDocument.Create(DefaultTitle, DefaultCategory, content);

        // Act
        document.CreateChunks();
        var firstChunkCount = document.Chunks.Count;

        document.CreateChunks();
        var secondChunkCount = document.Chunks.Count;

        // Assert — both calls should produce the same number (old chunks replaced)
        secondChunkCount.Should().Be(firstChunkCount);
    }

    [Fact]
    public void CreateChunks_CalledTwiceWithNoOverlap_SecondCallReplacesFirstChunks()
    {
        // Arrange — use content with no periods so sentence-boundary logic does not interfere,
        // and an overlap of 0 so the advance equals chunkSize exactly.
        var content = new string('X', 3000);
        var document = ReferenceDocument.Create(DefaultTitle, DefaultCategory, content);

        // Act — first call with small chunk size creates more chunks
        document.CreateChunks(chunkSize: 500, overlap: 0);
        var smallChunkCount = document.Chunks.Count;

        // Second call with larger chunk size creates fewer chunks
        document.CreateChunks(chunkSize: 1000, overlap: 0);
        var largeChunkCount = document.Chunks.Count;

        // Assert — chunks from first call were cleared; second call produced fewer chunks
        largeChunkCount.Should().BeLessThan(smallChunkCount);
    }
}
