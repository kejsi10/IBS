using IBS.Clients.Domain.Aggregates.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Clients.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Contact entity.
/// </summary>
public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.ClientId)
            .IsRequired();

        // PersonName value object
        builder.OwnsOne(x => x.Name, pn =>
        {
            pn.Property(p => p.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();
            pn.Property(p => p.MiddleName)
                .HasColumnName("MiddleName")
                .HasMaxLength(100);
            pn.Property(p => p.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
            pn.Property(p => p.Suffix)
                .HasColumnName("Suffix")
                .HasMaxLength(20);
        });

        builder.Property(x => x.Title)
            .HasMaxLength(100);

        // Email value object
        builder.OwnsOne(x => x.Email, e =>
        {
            e.Property(p => p.Value)
                .HasColumnName("Email")
                .HasMaxLength(255);
        });

        // Phone value object
        builder.OwnsOne(x => x.Phone, p =>
        {
            p.Property(x => x.Value)
                .HasColumnName("Phone")
                .HasMaxLength(20);
            p.Property(x => x.Type)
                .HasColumnName("PhoneType")
                .HasConversion<string>()
                .HasMaxLength(20);
            p.Property(x => x.Extension)
                .HasColumnName("PhoneExtension")
                .HasMaxLength(10);
        });

        builder.Property(x => x.IsPrimary)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.ClientId);
    }
}
