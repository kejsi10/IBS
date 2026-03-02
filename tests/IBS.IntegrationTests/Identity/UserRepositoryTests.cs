using FluentAssertions;
using IBS.Identity.Domain.Aggregates.User;
using IBS.Identity.Domain.ValueObjects;
using IBS.Identity.Infrastructure.Persistence;
using IBS.Identity.Infrastructure.Queries;
using IBS.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace IBS.IntegrationTests.Identity;

/// <summary>
/// Integration tests for UserRepository using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class UserRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private UserTestDbContext _context = null!;
    private UserRepository _repository = null!;
    private UserQueries _queries = null!;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UserRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<UserTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"UserTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new UserTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new UserRepository(_context);
        _queries = new UserQueries(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_NewUser_PersistsUser()
    {
        // Arrange
        var user = User.Create(
            _tenantId,
            Email.Create("test@example.com"),
            PasswordHash.FromHash("$2a$12$hashedpassword"),
            "John",
            "Doe",
            title: "Agent");

        // Act
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(user.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FirstName.Should().Be("John");
        retrieved.LastName.Should().Be("Doe");
        retrieved.Title.Should().Be("Agent");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        // Arrange
        var user = User.Create(
            _tenantId,
            Email.Create("find@example.com"),
            PasswordHash.FromHash("$2a$12$hashedpassword"),
            "Jane",
            "Smith");

        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByEmailAsync(_tenantId, "find@example.com");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_DifferentTenant_ReturnsNull()
    {
        // Arrange
        var user = User.Create(
            _tenantId,
            Email.Create("tenant@example.com"),
            PasswordHash.FromHash("$2a$12$hashedpassword"),
            "Jane",
            "Smith");

        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByEmailAsync(Guid.NewGuid(), "tenant@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        var user = User.Create(
            _tenantId,
            Email.Create("exists@example.com"),
            PasswordHash.FromHash("$2a$12$hashedpassword"),
            "Test",
            "User");

        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.EmailExistsAsync(_tenantId, "exists@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WithExcludeId_ExcludesUser()
    {
        // Arrange
        var user = User.Create(
            _tenantId,
            Email.Create("exclude@example.com"),
            PasswordHash.FromHash("$2a$12$hashedpassword"),
            "Test",
            "User");

        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.EmailExistsAsync(_tenantId, "exclude@example.com", user.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_MatchingTerm_ReturnsResults()
    {
        // Arrange
        var user1 = User.Create(
            _tenantId,
            Email.Create("john@example.com"),
            PasswordHash.FromHash("$2a$12$hashedpassword"),
            "John",
            "Doe");
        var user2 = User.Create(
            _tenantId,
            Email.Create("jane@example.com"),
            PasswordHash.FromHash("$2a$12$hashedpassword"),
            "Jane",
            "Smith");

        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.SearchAsync(_tenantId, "John", 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_NullTerm_ReturnsAllForTenant()
    {
        // Arrange
        var user1 = User.Create(
            _tenantId,
            Email.Create("user1@example.com"),
            PasswordHash.FromHash("$2a$12$hashedpassword"),
            "User",
            "One");
        var user2 = User.Create(
            _tenantId,
            Email.Create("user2@example.com"),
            PasswordHash.FromHash("$2a$12$hashedpassword"),
            "User",
            "Two");

        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.SearchAsync(_tenantId, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
    }
}

/// <summary>
/// Test DbContext for User integration tests.
/// </summary>
public class UserTestDbContext : DbContext
{
    public UserTestDbContext(DbContextOptions<UserTestDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the users set.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Identity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Identity.Infrastructure.Persistence.Configurations.UserConfiguration).Assembly);
    }
}
