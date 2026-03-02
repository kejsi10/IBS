using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Commands.ActivateUser;
using IBS.Identity.Domain.Aggregates.User;
using IBS.Identity.Domain.Repositories;
using IBS.Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace IBS.UnitTests.Identity.Application;

/// <summary>
/// Unit tests for the ActivateUserCommandHandler.
/// </summary>
public class ActivateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ActivateUserCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivateUserCommandHandlerTests"/> class.
    /// </summary>
    public ActivateUserCommandHandlerTests()
    {
        _handler = new ActivateUserCommandHandler(_userRepository, _unitOfWork, NullLogger<ActivateUserCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_PendingUser_ActivatesUser()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new ActivateUserCommand(user.Id);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ActivateUserCommand(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_AlreadyActiveUser_SucceedsIdempotently()
    {
        // Arrange
        var user = CreateTestUser();
        user.Activate();
        var command = new ActivateUserCommand(user.Id);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
    }

    private User CreateTestUser()
    {
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.FromHash("hashed_password_123");
        return User.Create(_tenantId, email, passwordHash, "John", "Doe");
    }
}
