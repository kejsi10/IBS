using FluentAssertions;
using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Aggregates.DocumentTemplate;
using IBS.Documents.Domain.Queries;
using IBS.Documents.Infrastructure.Persistence;
using IBS.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace IBS.IntegrationTests.Documents;

/// <summary>
/// Integration tests for DocumentRepository, DocumentTemplateRepository,
/// DocumentQueries, and DocumentTemplateQueries using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class DocumentRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private DocumentTestDbContext _context = null!;
    private DocumentRepository _documentRepository = null!;
    private DocumentTemplateRepository _templateRepository = null!;
    private DocumentQueries _documentQueries = null!;
    private DocumentTemplateQueries _templateQueries = null!;
    private readonly Guid _tenantId = Guid.NewGuid();

    public DocumentRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<DocumentTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"DocumentTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new DocumentTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _documentRepository = new DocumentRepository(_context);
        _templateRepository = new DocumentTemplateRepository(_context);
        _documentQueries = new DocumentQueries(_context);
        _templateQueries = new DocumentTemplateQueries(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    // ─── DocumentRepository ──────────────────────────────────────────────────

    [Fact]
    public async Task DocumentRepository_AddAsync_NewDocument_PersistsDocument()
    {
        // Arrange
        var document = CreateDocument();

        // Act
        await _documentRepository.AddAsync(document);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _documentRepository.GetByIdAsync(document.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FileName.Should().Be("report.pdf");
        retrieved.Category.Should().Be(DocumentCategory.Policy);
        retrieved.IsArchived.Should().BeFalse();
        retrieved.Version.Should().Be(1);
    }

    [Fact]
    public async Task DocumentRepository_GetByIdAsync_NonExistingDocument_ReturnsNull()
    {
        // Act
        var result = await _documentRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DocumentRepository_GetByEntityAsync_ReturnsMatchingDocuments()
    {
        // Arrange
        var policyId = Guid.NewGuid();
        var doc1 = CreateDocument(entityType: DocumentEntityType.Policy, entityId: policyId);
        var doc2 = CreateDocument(entityType: DocumentEntityType.Policy, entityId: policyId, fileName: "endorsement.pdf", category: DocumentCategory.Endorsement);
        var otherDoc = CreateDocument(entityType: DocumentEntityType.Client, entityId: Guid.NewGuid());

        await _documentRepository.AddAsync(doc1);
        await _documentRepository.AddAsync(doc2);
        await _documentRepository.AddAsync(otherDoc);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _documentRepository.GetByEntityAsync(DocumentEntityType.Policy, policyId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => d.EntityType == DocumentEntityType.Policy && d.EntityId == policyId);
    }

    [Fact]
    public async Task DocumentRepository_GetByEntityAsync_ExcludesArchivedDocuments()
    {
        // Arrange
        var policyId = Guid.NewGuid();
        var activeDoc = CreateDocument(entityType: DocumentEntityType.Policy, entityId: policyId);
        var archivedDoc = CreateDocument(entityType: DocumentEntityType.Policy, entityId: policyId, fileName: "old.pdf");
        archivedDoc.Archive();

        await _documentRepository.AddAsync(activeDoc);
        await _documentRepository.AddAsync(archivedDoc);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _documentRepository.GetByEntityAsync(DocumentEntityType.Policy, policyId);

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(d => !d.IsArchived);
    }

    [Fact]
    public async Task DocumentRepository_UpdateAsync_ArchivesDocument_PersistsChange()
    {
        // Arrange
        var document = CreateDocument();
        await _documentRepository.AddAsync(document);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _documentRepository.GetByIdAsync(document.Id);
        retrieved!.Archive();
        await _documentRepository.UpdateAsync(retrieved);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _documentRepository.GetByIdAsync(document.Id);
        updated!.IsArchived.Should().BeTrue();
    }

    // ─── DocumentTemplateRepository ──────────────────────────────────────────

    [Fact]
    public async Task TemplateRepository_AddAsync_NewTemplate_PersistsTemplate()
    {
        // Arrange
        var template = CreateTemplate();

        // Act
        await _templateRepository.AddAsync(template);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _templateRepository.GetByIdAsync(template.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Standard COI");
        retrieved.TemplateType.Should().Be(TemplateType.CertificateOfInsurance);
        retrieved.IsActive.Should().BeFalse();
        retrieved.Version.Should().Be(1);
    }

    [Fact]
    public async Task TemplateRepository_GetByIdAsync_NonExistingTemplate_ReturnsNull()
    {
        // Act
        var result = await _templateRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task TemplateRepository_GetAllAsync_ReturnsAllTemplates()
    {
        // Arrange
        var t1 = CreateTemplate(name: "Alpha Template");
        var t2 = CreateTemplate(name: "Beta Template", templateType: TemplateType.PolicySummary);
        await _templateRepository.AddAsync(t1);
        await _templateRepository.AddAsync(t2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _templateRepository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task TemplateRepository_GetActiveByTypeAsync_ReturnsOnlyActiveMatchingType()
    {
        // Arrange
        var activeCoi = CreateTemplate(name: "Active COI");
        activeCoi.Activate();
        var inactiveCoi = CreateTemplate(name: "Inactive COI");
        var activePolicy = CreateTemplate(name: "Active Policy", templateType: TemplateType.PolicySummary);
        activePolicy.Activate();

        await _templateRepository.AddAsync(activeCoi);
        await _templateRepository.AddAsync(inactiveCoi);
        await _templateRepository.AddAsync(activePolicy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _templateRepository.GetActiveByTypeAsync(TemplateType.CertificateOfInsurance);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active COI");
    }

    [Fact]
    public async Task TemplateRepository_UpdateAsync_ActivateAndUpdate_PersistsChanges()
    {
        // Arrange
        var template = CreateTemplate();
        await _templateRepository.AddAsync(template);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _templateRepository.GetByIdAsync(template.Id);
        retrieved!.Update("Updated Name", "Updated description", "<h1>{{PolicyNumber}}</h1>");
        retrieved.Activate();
        await _templateRepository.UpdateAsync(retrieved);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _templateRepository.GetByIdAsync(template.Id);
        updated!.Name.Should().Be("Updated Name");
        updated.Version.Should().Be(2);
        updated.IsActive.Should().BeTrue();
    }

    // ─── DocumentQueries ─────────────────────────────────────────────────────

    [Fact]
    public async Task DocumentQueries_GetByIdAsync_ReturnsReadModel()
    {
        // Arrange
        var document = CreateDocument(description: "Annual policy");
        await _documentRepository.AddAsync(document);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _documentQueries.GetByIdAsync(_tenantId, document.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(document.Id);
        result.FileName.Should().Be("report.pdf");
        result.Description.Should().Be("Annual policy");
    }

    [Fact]
    public async Task DocumentQueries_GetByIdAsync_WrongTenant_ReturnsNull()
    {
        // Arrange
        var document = CreateDocument();
        await _documentRepository.AddAsync(document);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _documentQueries.GetByIdAsync(Guid.NewGuid(), document.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DocumentQueries_SearchAsync_FiltersByCategory()
    {
        // Arrange
        var policyDoc = CreateDocument(category: DocumentCategory.Policy);
        var claimDoc = CreateDocument(fileName: "claim.pdf", category: DocumentCategory.ClaimReport);
        await _documentRepository.AddAsync(policyDoc);
        await _documentRepository.AddAsync(claimDoc);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new DocumentSearchFilter(_tenantId, Category: DocumentCategory.Policy, Page: 1, PageSize: 20);
        var result = await _documentQueries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Category.Should().Be(DocumentCategory.Policy);
    }

    [Fact]
    public async Task DocumentQueries_SearchAsync_FiltersBySearchTerm()
    {
        // Arrange
        var doc1 = CreateDocument(fileName: "invoice-2024.pdf");
        var doc2 = CreateDocument(fileName: "policy-renewal.pdf");
        await _documentRepository.AddAsync(doc1);
        await _documentRepository.AddAsync(doc2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new DocumentSearchFilter(_tenantId, SearchTerm: "invoice", Page: 1, PageSize: 20);
        var result = await _documentQueries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].FileName.Should().Be("invoice-2024.pdf");
    }

    [Fact]
    public async Task DocumentQueries_SearchAsync_ExcludesArchivedByDefault()
    {
        // Arrange
        var activeDoc = CreateDocument();
        var archivedDoc = CreateDocument(fileName: "archived.pdf");
        archivedDoc.Archive();

        await _documentRepository.AddAsync(activeDoc);
        await _documentRepository.AddAsync(archivedDoc);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new DocumentSearchFilter(_tenantId, Page: 1, PageSize: 20);
        var result = await _documentQueries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().OnlyContain(d => !d.IsArchived);
    }

    // ─── DocumentTemplateQueries ─────────────────────────────────────────────

    [Fact]
    public async Task TemplateQueries_GetByIdAsync_ReturnsReadModel()
    {
        // Arrange
        var template = CreateTemplate();
        await _templateRepository.AddAsync(template);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _templateQueries.GetByIdAsync(_tenantId, template.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(template.Id);
        result.Name.Should().Be("Standard COI");
        result.TemplateType.Should().Be(TemplateType.CertificateOfInsurance);
    }

    [Fact]
    public async Task TemplateQueries_SearchAsync_FiltersByIsActive()
    {
        // Arrange
        var active = CreateTemplate(name: "Active");
        active.Activate();
        var inactive = CreateTemplate(name: "Inactive");

        await _templateRepository.AddAsync(active);
        await _templateRepository.AddAsync(inactive);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new DocumentTemplateSearchFilter(_tenantId, IsActive: true, Page: 1, PageSize: 20);
        var result = await _templateQueries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task TemplateQueries_SearchAsync_FiltersByTemplateType()
    {
        // Arrange
        var coi = CreateTemplate(name: "COI Template", templateType: TemplateType.CertificateOfInsurance);
        var policy = CreateTemplate(name: "Policy Template", templateType: TemplateType.PolicySummary);

        await _templateRepository.AddAsync(coi);
        await _templateRepository.AddAsync(policy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new DocumentTemplateSearchFilter(_tenantId, TemplateType: TemplateType.PolicySummary, Page: 1, PageSize: 20);
        var result = await _templateQueries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].Name.Should().Be("Policy Template");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private Document CreateDocument(
        DocumentEntityType entityType = DocumentEntityType.General,
        Guid? entityId = null,
        string fileName = "report.pdf",
        DocumentCategory category = DocumentCategory.Policy,
        string? description = null)
    {
        return Document.Create(
            _tenantId,
            entityType,
            entityId,
            fileName,
            "application/pdf",
            204800L,
            $"blobs/{Guid.NewGuid()}/{fileName}",
            category,
            "admin@test.com",
            description);
    }

    private DocumentTemplate CreateTemplate(
        string name = "Standard COI",
        TemplateType templateType = TemplateType.CertificateOfInsurance)
    {
        return DocumentTemplate.Create(
            _tenantId,
            name,
            "Test template description",
            templateType,
            "<h1>{{PolicyNumber}}</h1><p>{{ClientName}}</p>",
            "admin@test.com");
    }
}

/// <summary>
/// Test DbContext for Document integration tests.
/// </summary>
public class DocumentTestDbContext : DbContext
{
    public DocumentTestDbContext(DbContextOptions<DocumentTestDbContext> options) : base(options)
    {
    }

    /// <summary>Gets the documents set.</summary>
    public DbSet<Document> Documents => Set<Document>();

    /// <summary>Gets the document templates set.</summary>
    public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Documents.Infrastructure.Persistence.Configurations.DocumentConfiguration).Assembly);
    }
}
