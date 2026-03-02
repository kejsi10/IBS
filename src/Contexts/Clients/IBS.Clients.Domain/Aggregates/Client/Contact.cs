using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Domain.Aggregates.Client;

/// <summary>
/// Represents a contact person for a client (typically for business clients).
/// </summary>
public sealed class Contact : TenantEntity
{
    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the contact's name.
    /// </summary>
    public PersonName Name { get; private set; } = null!;

    /// <summary>
    /// Gets the contact's title/role.
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// Gets the contact's email address.
    /// </summary>
    public EmailAddress? Email { get; private set; }

    /// <summary>
    /// Gets the contact's phone number.
    /// </summary>
    public PhoneNumber? Phone { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this is the primary contact.
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Contact() { }

    /// <summary>
    /// Creates a new contact.
    /// </summary>
    internal static Contact Create(
        Guid clientId,
        Guid tenantId,
        PersonName name,
        string? title,
        EmailAddress? email,
        PhoneNumber? phone,
        bool isPrimary)
    {
        return new Contact
        {
            ClientId = clientId,
            TenantId = tenantId,
            Name = name,
            Title = title,
            Email = email,
            Phone = phone,
            IsPrimary = isPrimary
        };
    }

    /// <summary>
    /// Updates the contact information.
    /// </summary>
    public void Update(PersonName name, string? title, EmailAddress? email, PhoneNumber? phone)
    {
        Name = name;
        Title = title;
        Email = email;
        Phone = phone;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets whether this contact is the primary contact.
    /// </summary>
    internal void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        MarkAsUpdated();
    }
}
