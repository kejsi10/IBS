using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Commands.AssignRole;
using IBS.Identity.Domain.Aggregates.Role;
using IBS.Identity.Domain.Aggregates.User;
using IBS.Identity.Domain.Repositories;
using IBS.Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace IBS.UnitTests.Identity.Application;

/// <summary>
/// Unit tests for the AssignRoleCommandHandler.
/// </summary>
public class AssignRoleCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AssignRoleCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="AssignRoleCommandHandlerTests"/> class.
    /// </summary>
    public AssignRoleCommandHandlerTests()
    {
        _handler = new AssignRoleCommandHandler(_userRepository, _roleRepository, _unitOfWork, NullLogger<AssignRoleCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidCommand_AssignsRole()
    {
        // Arrange
        var user = CreateTestUser();
        var role = Role.CreateTenantRole(_tenantId, "Agent");
        var command = new AssignRoleCommand(user.Id, role.Id);
        _userRepository.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _roleRepository.GetByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.UserRoles.Should().HaveCount(1);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new AssignRoleCommand(userId, Guid.NewGuid());
        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_RoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId = Guid.NewGuid();
        var command = new AssignRoleCommand(user.Id, roleId);
        _userRepository.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _roleRepository.GetByIdAsync(roleId, Arg.Any<CancellationToken>()).Returns((Role?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_DuplicateRole_SucceedsIdempotently()
    {
        // Arrange
        var user = CreateTestUser();
        var role = Role.CreateTenantRole(_tenantId, "Agent");
        user.AssignRole(role.Id);
        var command = new AssignRoleCommand(user.Id, role.Id);
        _userRepository.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _roleRepository.GetByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.UserRoles.Should().HaveCount(1);
    }

    private User CreateTestUser()
    {
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.FromHash("hashed_password_123");
        return User.Create(_tenantId, email, passwordHash, "John", "Doe");
    }
}
