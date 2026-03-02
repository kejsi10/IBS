using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Commands.UpdateRole;
using IBS.Identity.Application.Queries;
using IBS.Identity.Domain.Aggregates.Role;
using IBS.Identity.Domain.Repositories;
using NSubstitute;

namespace IBS.UnitTests.Identity.Application;

/// <summary>
/// Unit tests for the UpdateRoleCommandHandler.
/// </summary>
public class UpdateRoleCommandHandlerTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IRoleQueries _roleQueries = Substitute.For<IRoleQueries>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateRoleCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleCommandHandlerTests"/> class.
    /// </summary>
    public UpdateRoleCommandHandlerTests()
    {
        _handler = new UpdateRoleCommandHandler(_roleRepository, _roleQueries, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesRole()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "OldName", "Old description");
        var command = new UpdateRoleCommand(role.Id, _tenantId, "NewName", "New description");
        _roleRepository.GetByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _roleQueries.NameExistsAsync(_tenantId, "NewName", role.Id, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.Name.Should().Be("NewName");
        role.Description.Should().Be("New description");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new UpdateRoleCommand(roleId, _tenantId, "NewName");
        _roleRepository.GetByIdAsync(roleId, Arg.Any<CancellationToken>()).Returns((Role?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_SystemRole_ReturnsValidationError()
    {
        // Arrange
        var role = Role.CreateSystemRole("Admin", "System admin role");
        var command = new UpdateRoleCommand(role.Id, _tenantId, "NewName");
        _roleRepository.GetByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateName_ReturnsConflictError()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "OldName");
        var command = new UpdateRoleCommand(role.Id, _tenantId, "ExistingName");
        _roleRepository.GetByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _roleQueries.NameExistsAsync(_tenantId, "ExistingName", role.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Conflict");
    }
}
