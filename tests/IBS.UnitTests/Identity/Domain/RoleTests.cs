using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Aggregates.Role;
using IBS.Identity.Domain.Events;

namespace IBS.UnitTests.Identity.Domain;

/// <summary>
/// Unit tests for the Role aggregate root.
/// </summary>
public class RoleTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void CreateTenantRole_ValidInputs_CreatesRole()
    {
        // Arrange & Act
        var role = Role.CreateTenantRole(_tenantId, "Admin", "Administrator role");

        // Assert
        role.Should().NotBeNull();
        role.Name.Should().Be("Admin");
        role.NormalizedName.Should().Be("ADMIN");
        role.Description.Should().Be("Administrator role");
        role.TenantId.Should().Be(_tenantId);
        role.IsSystemRole.Should().BeFalse();
    }

    [Fact]
    public void CreateSystemRole_ValidInputs_CreatesRole()
    {
        // Arrange & Act
        var role = Role.CreateSystemRole("SuperAdmin", "Super administrator role");

        // Assert
        role.Should().NotBeNull();
        role.Name.Should().Be("SuperAdmin");
        role.NormalizedName.Should().Be("SUPERADMIN");
        role.Description.Should().Be("Super administrator role");
        role.TenantId.Should().BeNull();
        role.IsSystemRole.Should().BeTrue();
    }

    [Fact]
    public void CreateTenantRole_EmptyName_ThrowsException()
    {
        // Act
        var act = () => Role.CreateTenantRole(_tenantId, "", "Description");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void CreateSystemRole_EmptyName_ThrowsException()
    {
        // Act
        var act = () => Role.CreateSystemRole("", "Description");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void CreateTenantRole_TrimsWhitespace()
    {
        // Act
        var role = Role.CreateTenantRole(_tenantId, "  Admin  ", "  Description  ");

        // Assert
        role.Name.Should().Be("Admin");
        role.Description.Should().Be("Description");
    }

    [Fact]
    public void Update_TenantRole_UpdatesNameAndDescription()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "OldName", "Old description");

        // Act
        role.Update("NewName", "New description");

        // Assert
        role.Name.Should().Be("NewName");
        role.NormalizedName.Should().Be("NEWNAME");
        role.Description.Should().Be("New description");
    }

    [Fact]
    public void Update_SystemRole_ThrowsException()
    {
        // Arrange
        var role = Role.CreateSystemRole("SystemRole");

        // Act
        var act = () => role.Update("NewName", "New description");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*System roles*");
    }

    [Fact]
    public void Update_EmptyName_ThrowsException()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "OldName");

        // Act
        var act = () => role.Update("", "New description");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void GrantPermission_ValidPermission_AddsPermission()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Admin");
        var permissionId = Guid.NewGuid();

        // Act
        role.GrantPermission(permissionId);

        // Assert
        role.Permissions.Should().HaveCount(1);
        role.Permissions.First().PermissionId.Should().Be(permissionId);
        role.DomainEvents.Should().HaveCount(1);
        role.DomainEvents.First().Should().BeOfType<PermissionGrantedEvent>();
    }

    [Fact]
    public void GrantPermission_SystemRole_DoesNotRaiseEvent()
    {
        // Arrange
        var role = Role.CreateSystemRole("SystemRole");
        var permissionId = Guid.NewGuid();

        // Act
        role.GrantPermission(permissionId);

        // Assert
        role.Permissions.Should().HaveCount(1);
        role.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void GrantPermission_SamePermissionTwice_OnlyAddsOnce()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Admin");
        var permissionId = Guid.NewGuid();
        role.GrantPermission(permissionId);

        // Act
        role.GrantPermission(permissionId);

        // Assert
        role.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public void RevokePermission_ExistingPermission_RemovesPermission()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Admin");
        var permissionId = Guid.NewGuid();
        role.GrantPermission(permissionId);

        // Act
        role.RevokePermission(permissionId);

        // Assert
        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void RevokePermission_NonExistingPermission_DoesNothing()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Admin");

        // Act
        role.RevokePermission(Guid.NewGuid());

        // Assert
        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void HasPermission_ExistingPermission_ReturnsTrue()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Admin");
        var permissionId = Guid.NewGuid();
        role.GrantPermission(permissionId);

        // Act
        var hasPermission = role.HasPermission(permissionId);

        // Assert
        hasPermission.Should().BeTrue();
    }

    [Fact]
    public void HasPermission_NonExistingPermission_ReturnsFalse()
    {
        // Arrange
        var role = Role.CreateTenantRole(_tenantId, "Admin");

        // Act
        var hasPermission = role.HasPermission(Guid.NewGuid());

        // Assert
        hasPermission.Should().BeFalse();
    }
}
