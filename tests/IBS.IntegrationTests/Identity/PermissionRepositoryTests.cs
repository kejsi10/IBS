using FluentAssertions;
using IBS.Identity.Domain.Aggregates.Permission;
using IBS.Identity.Infrastructure.Persistence;
using IBS.Identity.Infrastructure.Queries;
using IBS.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace IBS.IntegrationTests.Identity;

/// <summary>
/// Integration tests for PermissionRepository and PermissionQueries using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class PermissionRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private RoleTestDbContext _context = null!;
    private PermissionRepository _repository = null!;
    private PermissionQueries _queries = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRepositoryTests"/> class.
    /// </summary>
    public PermissionRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<RoleTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"PermTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new RoleTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new PermissionRepository(_context);
        _queries = new PermissionQueries(_context);
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    #region Repository Tests

    [Fact]
    public async Task AddAsync_NewPermission_PersistsPermission()
    {
        // Arrange
        var permission = Permission.Create("perm:test:create", "TestModule", "Create things");

        // Act
        await _repository.AddAsync(permission);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(permission.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("perm:test:create");
        retrieved.Module.Should().Be("TestModule");
        retrieved.Description.Should().Be("Create things");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPermission_ReturnsPermission()
    {
        // Arrange
        var permission = Permission.Create("perm:test:read", "TestModule", "Read things");
        await _repository.AddAsync(permission);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(permission.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(permission.Id);
        retrieved.Name.Should().Be("perm:test:read");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingPermission_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ExistingName_ReturnsPermission()
    {
        // Arrange
        var permission = Permission.Create("perm:test:findbyname", "TestModule", "Find by name");
        await _repository.AddAsync(permission);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByNameAsync("perm:test:findbyname");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(permission.Id);
    }

    [Fact]
    public async Task GetByNameAsync_NonExistingName_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("nonexistent:permission");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByModuleAsync_ExistingModule_ReturnsFiltered()
    {
        // Arrange
        var perm1 = Permission.Create("perm:mod:read", "ModuleA", "Read");
        var perm2 = Permission.Create("perm:mod:write", "ModuleA", "Write");
        var perm3 = Permission.Create("perm:other:read", "ModuleB", "Other read");
        await _repository.AddAsync(perm1);
        await _repository.AddAsync(perm2);
        await _repository.AddAsync(perm3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByModuleAsync("ModuleA");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Module == "ModuleA");
    }

    [Fact]
    public async Task GetByIdsAsync_ValidIds_ReturnsMatching()
    {
        // Arrange
        var perm1 = Permission.Create("perm:ids:one", "IdsModule", "One");
        var perm2 = Permission.Create("perm:ids:two", "IdsModule", "Two");
        var perm3 = Permission.Create("perm:ids:three", "IdsModule", "Three");
        await _repository.AddAsync(perm1);
        await _repository.AddAsync(perm2);
        await _repository.AddAsync(perm3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdsAsync([perm1.Id, perm3.Id]);

        // Assert
        result.Should().HaveCount(2);
        result.Select(p => p.Id).Should().Contain(perm1.Id);
        result.Select(p => p.Id).Should().Contain(perm3.Id);
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task Queries_GetAllAsync_NoFilter_ReturnsAll()
    {
        // Arrange
        var perm1 = Permission.Create("perm:all:one", "AllModule", "One");
        var perm2 = Permission.Create("perm:all:two", "AllModule", "Two");
        await _repository.AddAsync(perm1);
        await _repository.AddAsync(perm2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetAllAsync();

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Queries_GetAllAsync_WithModuleFilter_ReturnsFiltered()
    {
        // Arrange
        var perm1 = Permission.Create("perm:filter:read", "FilterModule", "Read");
        var perm2 = Permission.Create("perm:filter:write", "FilterModule", "Write");
        var perm3 = Permission.Create("perm:other:read", "OtherFilterModule", "Other read");
        await _repository.AddAsync(perm1);
        await _repository.AddAsync(perm2);
        await _repository.AddAsync(perm3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetAllAsync("FilterModule");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Module == "FilterModule");
    }

    [Fact]
    public async Task Queries_GetByIdAsync_ExistingPermission_ReturnsDto()
    {
        // Arrange
        var permission = Permission.Create("perm:qid:read", "QidModule", "Read desc");
        await _repository.AddAsync(permission);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var dto = await _queries.GetByIdAsync(permission.Id);

        // Assert
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(permission.Id);
        dto.Name.Should().Be("perm:qid:read");
        dto.Module.Should().Be("QidModule");
        dto.Description.Should().Be("Read desc");
    }

    [Fact]
    public async Task Queries_GetByIdAsync_NonExistingPermission_ReturnsNull()
    {
        // Act
        var dto = await _queries.GetByIdAsync(Guid.NewGuid());

        // Assert
        dto.Should().BeNull();
    }

    #endregion
}
