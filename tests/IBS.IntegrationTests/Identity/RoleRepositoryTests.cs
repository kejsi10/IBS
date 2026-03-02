using FluentAssertions;
using IBS.Identity.Domain.Aggregates.Permission;
using IBS.Identity.Domain.Aggregates.Role;
using IBS.Identity.Domain.Aggregates.User;
using IBS.Identity.Domain.ValueObjects;
using IBS.Identity.Infrastructure.Persistence;
using IBS.Identity.Infrastructure.Queries;
using IBS.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace IBS.IntegrationTests.Identity;

/// <summary>
/// Integration tests for RoleRepository and RoleQueries using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class RoleRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private RoleTestDbContext _context = null!;
    private RoleRepository _repository = null!;
    private RoleQueries _queries = null!;
    private readonly Guid _tenantId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRepositoryTests"/> class.
    /// </summary>
    public RoleRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<RoleTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"RoleTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new RoleTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new RoleRepository(_context);
        _queries = new RoleQueries(_context);
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    #region Repository Tests

    [Fact]
    public async Task AddAsync_NewTenantRole_PersistsRole()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Claims Adjuster", "Handles claims");

        // Act
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(role.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Claims Adjuster");
        retrieved.Description.Should().Be("Handles claims");
        retrieved.TenantId.Should().Be(_tenantId);
        retrieved.IsSystemRole.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_NewSystemRole_PersistsRole()
    {
        // Arrange
        var role = Role.CreateSystemRole("Super Admin", "Full system access");

        // Act
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(role.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Super Admin");
        retrieved.IsSystemRole.Should().BeTrue();
        retrieved.TenantId.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRole_ReturnsRole()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Agent");
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(role.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(role.Id);
        retrieved.Name.Should().Be("Agent");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingRole_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithPermissionsAsync_RoleWithPermissions_LoadsPermissions()
    {
        // Arrange
        var permission = Permission.Create("roles:test:read", "TestModule", "Read permission");
        _context.Set<Permission>().Add(permission);
        await _context.SaveChangesAsync();

        var role = Role.CreateTenantRole(_tenantId, "Viewer");
        role.GrantPermission(permission.Id);
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdWithPermissionsAsync(role.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Permissions.Should().HaveCount(1);
        retrieved.Permissions.First().PermissionId.Should().Be(permission.Id);
        retrieved.Permissions.First().Permission.Should().NotBeNull();
        retrieved.Permissions.First().Permission!.Name.Should().Be("roles:test:read");
    }

    [Fact]
    public async Task ModifyRole_GrantAndRevokePermission_PersistsChanges()
    {
        // Arrange
        var perm1 = Permission.Create("roles:test:perm1", "TestModule", "Permission 1");
        var perm2 = Permission.Create("roles:test:perm2", "TestModule", "Permission 2");
        _context.Set<Permission>().Add(perm1);
        _context.Set<Permission>().Add(perm2);
        await _context.SaveChangesAsync();

        var role = Role.CreateTenantRole(_tenantId, "Editor");
        role.GrantPermission(perm1.Id);
        role.GrantPermission(perm2.Id);
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var loaded = await _repository.GetByIdWithPermissionsAsync(role.Id);
        loaded!.RevokePermission(perm1.Id);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var final = await _repository.GetByIdWithPermissionsAsync(role.Id);
        final.Should().NotBeNull();
        final!.Permissions.Should().HaveCount(1);
        final.Permissions.First().PermissionId.Should().Be(perm2.Id);
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task Queries_GetByIdAsync_ReturnsRoleDetails()
    {
        // Arrange
        var permission = Permission.Create("roles:qtest:read", "QTestModule", "Read");
        _context.Set<Permission>().Add(permission);
        await _context.SaveChangesAsync();

        var role = Role.CreateTenantRole(_tenantId, "Query Test Role", "Description here");
        role.GrantPermission(permission.Id);
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var dto = await _queries.GetByIdAsync(role.Id);

        // Assert
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(role.Id);
        dto.Name.Should().Be("Query Test Role");
        dto.Description.Should().Be("Description here");
        dto.TenantId.Should().Be(_tenantId);
        dto.IsSystemRole.Should().BeFalse();
        dto.Permissions.Should().HaveCount(1);
        dto.Permissions[0].Name.Should().Be("roles:qtest:read");
        dto.Permissions[0].Module.Should().Be("QTestModule");
    }

    [Fact]
    public async Task Queries_GetByIdAsync_NonExistingRole_ReturnsNull()
    {
        // Act
        var dto = await _queries.GetByIdAsync(Guid.NewGuid());

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public async Task Queries_GetByNameAsync_ExistingRole_ReturnsRole()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Named Role", "A named role");
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var dto = await _queries.GetByNameAsync(_tenantId, "Named Role");

        // Assert
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(role.Id);
        dto.Name.Should().Be("Named Role");
    }

    [Fact]
    public async Task Queries_GetTenantRolesAsync_ReturnsTenantAndSystemRoles()
    {
        // Arrange
        var tenantRole = Role.CreateTenantRole(_tenantId, "Tenant Role");
        var systemRole = Role.CreateSystemRole("System Role");
        await _repository.AddAsync(tenantRole);
        await _repository.AddAsync(systemRole);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var roles = await _queries.GetTenantRolesAsync(_tenantId);

        // Assert
        roles.Should().HaveCountGreaterThanOrEqualTo(2);
        roles.Should().Contain(r => r.Name == "Tenant Role" && !r.IsSystemRole);
        roles.Should().Contain(r => r.Name == "System Role" && r.IsSystemRole);
    }

    [Fact]
    public async Task Queries_GetSystemRolesAsync_ReturnsOnlySystemRoles()
    {
        // Arrange
        var tenantRole = Role.CreateTenantRole(_tenantId, "Non System");
        var systemRole = Role.CreateSystemRole("Only System");
        await _repository.AddAsync(tenantRole);
        await _repository.AddAsync(systemRole);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var roles = await _queries.GetSystemRolesAsync();

        // Assert
        roles.Should().OnlyContain(r => r.IsSystemRole);
        roles.Should().Contain(r => r.Name == "Only System");
    }

    [Fact]
    public async Task Queries_NameExistsAsync_ExistingName_ReturnsTrue()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Unique Name");
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _queries.NameExistsAsync(_tenantId, "Unique Name");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Queries_NameExistsAsync_WithExcludeId_ReturnsFalse()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Exclude Test");
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _queries.NameExistsAsync(_tenantId, "Exclude Test", role.Id);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Queries_NameExistsAsync_NonExistingName_ReturnsFalse()
    {
        // Act
        var exists = await _queries.NameExistsAsync(_tenantId, "Does Not Exist");

        // Assert
        exists.Should().BeFalse();
    }

    #endregion
}

/// <summary>
/// Test DbContext for Role integration tests.
/// Applies all Identity configurations to support Role, Permission, RolePermission, UserRole, and User entities.
/// </summary>
public class RoleTestDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleTestDbContext"/> class.
    /// </summary>
    public RoleTestDbContext(DbContextOptions<RoleTestDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the roles set.
    /// </summary>
    public DbSet<Role> Roles => Set<Role>();

    /// <summary>
    /// Gets the permissions set.
    /// </summary>
    public DbSet<Permission> Permissions => Set<Permission>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all Identity configurations (Role, Permission, RolePermission, UserRole, User)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Identity.Infrastructure.Persistence.Configurations.RoleConfiguration).Assembly);
    }
}
