using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Commands.CreateRole;
using IBS.Identity.Application.Queries;
using IBS.Identity.Domain.Aggregates.Role;
using IBS.Identity.Domain.Repositories;
using NSubstitute;

namespace IBS.UnitTests.Identity.Application;

/// <summary>
/// Unit tests for the CreateRoleCommandHandler.
/// </summary>
public class CreateRoleCommandHandlerTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IRoleQueries _roleQueries = Substitute.For<IRoleQueries>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateRoleCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateRoleCommandHandlerTests"/> class.
    /// </summary>
    public CreateRoleCommandHandlerTests()
    {
        _handler = new CreateRoleCommandHandler(_roleRepository, _roleQueries, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesRoleAndReturnsId()
    {
        // Arrange
        var command = new CreateRoleCommand(_tenantId, "Agent", "Insurance agent role");
        _roleQueries.NameExistsAsync(_tenantId, "Agent", null, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _roleRepository.Received(1).AddAsync(Arg.Is<Role>(r => r.Name == "Agent"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateName_ReturnsConflictError()
    {
        // Arrange
        var command = new CreateRoleCommand(_tenantId, "Agent", "Insurance agent role");
        _roleQueries.NameExistsAsync(_tenantId, "Agent", null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Conflict");
        await _roleRepository.DidNotReceive().AddAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesTenantRole()
    {
        // Arrange
        var command = new CreateRoleCommand(_tenantId, "Manager");
        _roleQueries.NameExistsAsync(_tenantId, "Manager", null, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _roleRepository.Received(1).AddAsync(
            Arg.Is<Role>(r =>
                r.Name == "Manager" &&
                r.TenantId == _tenantId &&
                !r.IsSystemRole),
            Arg.Any<CancellationToken>());
    }
}
