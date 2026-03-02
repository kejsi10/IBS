using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Events;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Domain.Aggregates.Client;

/// <summary>
/// Represents a client (insured party) in the system.
/// Supports both individual and business clients.
/// </summary>
public sealed class Client : TenantAggregateRoot
{
    private readonly List<Contact> _contacts = [];
    private readonly List<Address> _addresses = [];
    private readonly List<Communication> _communications = [];

    /// <summary>
    /// Gets the client type (Individual or Business).
    /// </summary>
    public ClientType ClientType { get; private set; }

    /// <summary>
    /// Gets the client status.
    /// </summary>
    public ClientStatus Status { get; private set; }

    /// <summary>
    /// Gets the person name for individual clients.
    /// </summary>
    public PersonName? PersonName { get; private set; }

    /// <summary>
    /// Gets the date of birth for individual clients.
    /// </summary>
    public DateOnly? DateOfBirth { get; private set; }

    /// <summary>
    /// Gets the encrypted SSN for individual clients.
    /// </summary>
    public byte[]? SsnEncrypted { get; private set; }

    /// <summary>
    /// Gets the business information for business clients.
    /// </summary>
    public BusinessInfo? BusinessInfo { get; private set; }

    /// <summary>
    /// Gets the encrypted EIN for business clients.
    /// </summary>
    public byte[]? EinEncrypted { get; private set; }

    /// <summary>
    /// Gets the primary email address.
    /// </summary>
    public EmailAddress? Email { get; private set; }

    /// <summary>
    /// Gets the primary phone number.
    /// </summary>
    public PhoneNumber? Phone { get; private set; }

    /// <summary>
    /// Gets the user who created this client.
    /// </summary>
    public Guid CreatedBy { get; private set; }

    /// <summary>
    /// Gets the contacts associated with this client.
    /// </summary>
    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();

    /// <summary>
    /// Gets the addresses associated with this client.
    /// </summary>
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    /// <summary>
    /// Gets the communication history for this client.
    /// </summary>
    public IReadOnlyCollection<Communication> Communications => _communications.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Client() { }

    /// <summary>
    /// Creates a new individual client.
    /// </summary>
    public static Client CreateIndividual(
        Guid tenantId,
        PersonName personName,
        Guid createdBy,
        DateOnly? dateOfBirth = null,
        EmailAddress? email = null,
        PhoneNumber? phone = null)
    {
        var client = new Client
        {
            TenantId = tenantId,
            ClientType = ClientType.Individual,
            Status = ClientStatus.Active,
            PersonName = personName,
            DateOfBirth = dateOfBirth,
            Email = email,
            Phone = phone,
            CreatedBy = createdBy
        };

        client.RaiseDomainEvent(new ClientRegisteredEvent(
            client.Id,
            client.TenantId,
            client.ClientType,
            client.GetDisplayName()));

        return client;
    }

    /// <summary>
    /// Creates a new business client.
    /// </summary>
    public static Client CreateBusiness(
        Guid tenantId,
        BusinessInfo businessInfo,
        Guid createdBy,
        EmailAddress? email = null,
        PhoneNumber? phone = null)
    {
        var client = new Client
        {
            TenantId = tenantId,
            ClientType = ClientType.Business,
            Status = ClientStatus.Active,
            BusinessInfo = businessInfo,
            Email = email,
            Phone = phone,
            CreatedBy = createdBy
        };

        client.RaiseDomainEvent(new ClientRegisteredEvent(
            client.Id,
            client.TenantId,
            client.ClientType,
            client.GetDisplayName()));

        return client;
    }

    /// <summary>
    /// Gets the display name for this client.
    /// </summary>
    public string GetDisplayName()
    {
        return ClientType switch
        {
            ClientType.Individual => PersonName?.FullName ?? "Unknown",
            ClientType.Business => BusinessInfo?.Name ?? "Unknown",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Updates the person name for individual clients.
    /// </summary>
    public void UpdatePersonName(PersonName personName)
    {
        EnsureIndividualClient();
        PersonName = personName;
        MarkAsUpdated();
        RaiseDomainEvent(new ClientUpdatedEvent(Id, TenantId));
    }

    /// <summary>
    /// Updates the business info for business clients.
    /// </summary>
    public void UpdateBusinessInfo(BusinessInfo businessInfo)
    {
        EnsureBusinessClient();
        BusinessInfo = businessInfo;
        MarkAsUpdated();
        RaiseDomainEvent(new ClientUpdatedEvent(Id, TenantId));
    }

    /// <summary>
    /// Updates the email address.
    /// </summary>
    public void UpdateEmail(EmailAddress? email)
    {
        Email = email;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the phone number.
    /// </summary>
    public void UpdatePhone(PhoneNumber? phone)
    {
        Phone = phone;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates this client.
    /// </summary>
    public void Deactivate()
    {
        if (Status == ClientStatus.Inactive)
            return;

        Status = ClientStatus.Inactive;
        MarkAsUpdated();
        RaiseDomainEvent(new ClientDeactivatedEvent(Id, TenantId));
    }

    /// <summary>
    /// Reactivates this client.
    /// </summary>
    public void Reactivate()
    {
        if (Status == ClientStatus.Active)
            return;

        Status = ClientStatus.Active;
        MarkAsUpdated();
    }

    /// <summary>
    /// Adds a contact to this client.
    /// </summary>
    public Contact AddContact(PersonName name, string? title = null, EmailAddress? email = null, PhoneNumber? phone = null, bool isPrimary = false)
    {
        if (isPrimary && _contacts.Any(c => c.IsPrimary))
        {
            foreach (var existingContact in _contacts.Where(c => c.IsPrimary))
            {
                existingContact.SetPrimary(false);
            }
        }

        var contact = Contact.Create(Id, TenantId, name, title, email, phone, isPrimary);
        _contacts.Add(contact);
        MarkAsUpdated();
        RaiseDomainEvent(new ContactAddedEvent(Id, TenantId, contact.Id));

        return contact;
    }

    /// <summary>
    /// Removes a contact from this client.
    /// </summary>
    public void RemoveContact(Guid contactId)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == contactId)
            ?? throw new BusinessRuleViolationException($"Contact {contactId} not found.");

        _contacts.Remove(contact);
        MarkAsUpdated();
        RaiseDomainEvent(new ContactRemovedEvent(Id, TenantId, contactId));
    }

    /// <summary>
    /// Adds an address to this client.
    /// </summary>
    public Address AddAddress(AddressType type, string streetLine1, string city, string state, string postalCode, string? streetLine2 = null, string country = "USA", bool isPrimary = false)
    {
        if (isPrimary && _addresses.Any(a => a.AddressType == type && a.IsPrimary))
        {
            foreach (var existingAddress in _addresses.Where(a => a.AddressType == type && a.IsPrimary))
            {
                existingAddress.SetPrimary(false);
            }
        }

        var address = Address.Create(Id, TenantId, type, streetLine1, streetLine2, city, state, postalCode, country, isPrimary);
        _addresses.Add(address);
        MarkAsUpdated();
        RaiseDomainEvent(new AddressAddedEvent(Id, TenantId, address.Id));

        return address;
    }

    /// <summary>
    /// Removes an address from this client.
    /// </summary>
    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new BusinessRuleViolationException($"Address {addressId} not found.");

        _addresses.Remove(address);
        MarkAsUpdated();
        RaiseDomainEvent(new AddressRemovedEvent(Id, TenantId, addressId));
    }

    /// <summary>
    /// Sets an address as the primary address for its type.
    /// </summary>
    public void SetPrimaryAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new BusinessRuleViolationException($"Address {addressId} not found.");

        // Remove primary flag from other addresses of the same type
        foreach (var existingAddress in _addresses.Where(a => a.AddressType == address.AddressType && a.IsPrimary && a.Id != addressId))
        {
            existingAddress.SetPrimary(false);
        }

        address.SetPrimary(true);
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets a contact as the primary contact.
    /// </summary>
    public void SetPrimaryContact(Guid contactId)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == contactId)
            ?? throw new BusinessRuleViolationException($"Contact {contactId} not found.");

        // Remove primary flag from other contacts
        foreach (var existingContact in _contacts.Where(c => c.IsPrimary && c.Id != contactId))
        {
            existingContact.SetPrimary(false);
        }

        contact.SetPrimary(true);
        MarkAsUpdated();
    }

    /// <summary>
    /// Logs a communication with this client.
    /// </summary>
    public Communication LogCommunication(CommunicationType type, string subject, string? notes, Guid loggedBy)
    {
        var communication = Communication.Create(Id, TenantId, type, subject, notes, loggedBy);
        _communications.Add(communication);
        MarkAsUpdated();
        RaiseDomainEvent(new CommunicationLoggedEvent(Id, TenantId, communication.Id, type));

        return communication;
    }

    private void EnsureIndividualClient()
    {
        if (ClientType != ClientType.Individual)
            throw new BusinessRuleViolationException("This operation is only valid for individual clients.");
    }

    private void EnsureBusinessClient()
    {
        if (ClientType != ClientType.Business)
            throw new BusinessRuleViolationException("This operation is only valid for business clients.");
    }
}
