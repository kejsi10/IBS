using IBS.Identity.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the User entity.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        // Email value object
        builder.OwnsOne(x => x.Email, e =>
        {
            e.Property(p => p.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();
            e.Property(p => p.NormalizedValue)
                .HasColumnName("NormalizedEmail")
                .HasMaxLength(255)
                .IsRequired();

            e.HasIndex(p => new { p.NormalizedValue })
                .HasDatabaseName("IX_Users_NormalizedEmail");
        });

        // Password hash value object
        builder.OwnsOne(x => x.PasswordHash, p =>
        {
            p.Property(x => x.Value)
                .HasColumnName("PasswordHash")
                .HasMaxLength(500)
                .IsRequired();
        });

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(100);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.LastLoginAt);

        builder.Property(x => x.FailedLoginAttempts)
            .IsRequired();

        builder.Property(x => x.LockedUntil);

        builder.Property(x => x.PasswordResetToken)
            .HasMaxLength(100);

        builder.Property(x => x.PasswordResetTokenExpiry);

        builder.Property(x => x.EmailConfirmationToken)
            .HasMaxLength(100);

        builder.Property(x => x.EmailConfirmed)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(x => x.UserRoles)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RefreshTokens)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);

        // Indexes
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.Status);
    }
}
