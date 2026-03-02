using IBS.Clients.Domain.Aggregates.Client;
using IBS.Clients.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Clients.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Client entity.
/// </summary>
public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.ClientType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // PersonName value object for individual clients
        builder.OwnsOne(x => x.PersonName, pn =>
        {
            pn.Property(p => p.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100);
            pn.Property(p => p.MiddleName)
                .HasColumnName("MiddleName")
                .HasMaxLength(100);
            pn.Property(p => p.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100);
            pn.Property(p => p.Suffix)
                .HasColumnName("Suffix")
                .HasMaxLength(20);
        });

        builder.Property(x => x.DateOfBirth);

        builder.Property(x => x.SsnEncrypted)
            .HasMaxLength(256);

        // BusinessInfo value object for business clients
        builder.OwnsOne(x => x.BusinessInfo, bi =>
        {
            bi.Property(b => b.Name)
                .HasColumnName("BusinessName")
                .HasMaxLength(255);
            bi.Property(b => b.DbaName)
                .HasColumnName("DbaName")
                .HasMaxLength(255);
            bi.Property(b => b.BusinessType)
                .HasColumnName("BusinessType")
                .HasMaxLength(50);
            bi.Property(b => b.Industry)
                .HasColumnName("Industry")
                .HasMaxLength(100);
            bi.Property(b => b.YearEstablished)
                .HasColumnName("YearEstablished");
            bi.Property(b => b.NumberOfEmployees)
                .HasColumnName("NumberOfEmployees");
            bi.Property(b => b.AnnualRevenue)
                .HasColumnName("AnnualRevenue")
                .HasPrecision(18, 2);
            bi.Property(b => b.Website)
                .HasColumnName("Website")
                .HasMaxLength(500);
        });

        builder.Property(x => x.EinEncrypted)
            .HasMaxLength(256);

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

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(x => x.Contacts)
            .WithOne()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Addresses)
            .WithOne()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Communications)
            .WithOne()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);

        // Indexes
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.Status });
    }
}
