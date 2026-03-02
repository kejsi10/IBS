using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Commands.GrantPermission;
using IBS.Identity.Domain.Aggregates.Permission;
using IBS.Identity.Domain.Aggregates.Role;
using IBS.Identity.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace IBS.UnitTests.Identity.Application;

/// <summary>
/// Unit tests for the GrantPermissionCommandHandler.
/// </summary>
public class GrantPermissionCommandHandlerTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly GrantPermissionCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GrantPermissionCommandHandlerTests"/> class.
    /// </summary>
    public GrantPermissionCommandHandlerTests()
    {
        _handler = new GrantPermissionCommandHandler(_roleRepository, _permissionRepository, _unitOfWork, NullLogger<GrantPermissionCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidCommand_GrantsPermission()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Agent");
        var permission = Permission.Create("clients:read", "Clients", "Read clients");
        var command = new GrantPermissionCommand(role.Id, permission.Id);
        _roleRepository.GetByIdWithPermissionsAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _permissionRepository.GetByIdAsync(permission.Id, Arg.Any<CancellationToken>()).Returns(permission);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.Permissions.Should().HaveCount(1);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new GrantPermissionCommand(roleId, Guid.NewGuid());
        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>()).Returns((Role?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_PermissionNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Agent");
        var permissionId = Guid.NewGuid();
        var command = new GrantPermissionCommand(role.Id, permissionId);
        _roleRepository.GetByIdWithPermissionsAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _permissionRepository.GetByIdAsync(permissionId, Arg.Any<CancellationToken>()).Returns((Permission?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_DuplicatePermission_SucceedsIdempotently()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Agent");
        var permission = Permission.Create("clients:read", "Clients", "Read clients");
        role.GrantPermission(permission.Id);
        var command = new GrantPermissionCommand(role.Id, permission.Id);
        _roleRepository.GetByIdWithPermissionsAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _permissionRepository.GetByIdAsync(permission.Id, Arg.Any<CancellationToken>()).Returns(permission);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.Permissions.Should().HaveCount(1);
    }
}
