using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Aggregates.Client;
using IBS.Clients.Domain.Events;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.UnitTests.Clients.Domain;

/// <summary>
/// Unit tests for the Client aggregate root.
/// </summary>
public class ClientTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void CreateIndividual_ValidInputs_CreatesClient()
    {
        // Arrange
        var personName = PersonName.Create("John", "Doe");

        // Act
        var client = Client.CreateIndividual(_tenantId, personName, _userId);

        // Assert
        client.Should().NotBeNull();
        client.ClientType.Should().Be(ClientType.Individual);
        client.Status.Should().Be(ClientStatus.Active);
        client.PersonName.Should().Be(personName);
        client.TenantId.Should().Be(_tenantId);
        client.CreatedBy.Should().Be(_userId);
        client.DomainEvents.Should().HaveCount(1);
        client.DomainEvents.First().Should().BeOfType<ClientRegisteredEvent>();
    }

    [Fact]
    public void CreateIndividual_WithOptionalFields_SetsAllFields()
    {
        // Arrange
        var personName = PersonName.Create("John", "Doe", "William", "Jr.");
        var dateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-30));
        var email = EmailAddress.Create("john.doe@example.com");
        var phone = PhoneNumber.Create("555-123-4567");

        // Act
        var client = Client.CreateIndividual(_tenantId, personName, _userId, dateOfBirth, email, phone);

        // Assert
        client.PersonName!.FirstName.Should().Be("John");
        client.PersonName.LastName.Should().Be("Doe");
        client.PersonName.MiddleName.Should().Be("William");
        client.PersonName.Suffix.Should().Be("Jr.");
        client.DateOfBirth.Should().Be(dateOfBirth);
        client.Email!.Value.Should().Be("john.doe@example.com");
        client.Phone!.Value.Should().Be("5551234567");
    }

    [Fact]
    public void CreateBusiness_ValidInputs_CreatesClient()
    {
        // Arrange
        var businessInfo = BusinessInfo.Create("Acme Corp", "LLC");

        // Act
        var client = Client.CreateBusiness(_tenantId, businessInfo, _userId);

        // Assert
        client.Should().NotBeNull();
        client.ClientType.Should().Be(ClientType.Business);
        client.Status.Should().Be(ClientStatus.Active);
        client.BusinessInfo.Should().Be(businessInfo);
        client.TenantId.Should().Be(_tenantId);
        client.DomainEvents.Should().HaveCount(1);
        client.DomainEvents.First().Should().BeOfType<ClientRegisteredEvent>();
    }

    [Fact]
    public void CreateBusiness_WithAllFields_SetsAllFields()
    {
        // Arrange
        var businessInfo = BusinessInfo.Create(
            "Acme Corporation",
            "Corporation",
            "Acme Co",
            "Technology",
            2010,
            100,
            5000000m,
            "https://acme.com");
        var email = EmailAddress.Create("info@acme.com");
        var phone = PhoneNumber.Create("555-999-0000");

        // Act
        var client = Client.CreateBusiness(_tenantId, businessInfo, _userId, email, phone);

        // Assert
        client.BusinessInfo!.Name.Should().Be("Acme Corporation");
        client.BusinessInfo.BusinessType.Should().Be("Corporation");
        client.BusinessInfo.DbaName.Should().Be("Acme Co");
        client.BusinessInfo.Industry.Should().Be("Technology");
        client.BusinessInfo.YearEstablished.Should().Be(2010);
        client.BusinessInfo.NumberOfEmployees.Should().Be(100);
        client.BusinessInfo.AnnualRevenue.Should().Be(5000000m);
        client.BusinessInfo.Website.Should().Be("https://acme.com");
        client.Email!.Value.Should().Be("info@acme.com");
    }

    [Fact]
    public void GetDisplayName_IndividualClient_ReturnsFullName()
    {
        // Arrange
        var personName = PersonName.Create("John", "Doe", "William");
        var client = Client.CreateIndividual(_tenantId, personName, _userId);

        // Act
        var displayName = client.GetDisplayName();

        // Assert
        displayName.Should().Be("John William Doe");
    }

    [Fact]
    public void GetDisplayName_BusinessClient_ReturnsBusinessName()
    {
        // Arrange
        var businessInfo = BusinessInfo.Create("Acme Corp", "LLC");
        var client = Client.CreateBusiness(_tenantId, businessInfo, _userId);

        // Act
        var displayName = client.GetDisplayName();

        // Assert
        displayName.Should().Be("Acme Corp");
    }

    [Fact]
    public void UpdatePersonName_IndividualClient_UpdatesName()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        client.ClearDomainEvents();
        var newName = PersonName.Create("Jane", "Smith");

        // Act
        client.UpdatePersonName(newName);

        // Assert
        client.PersonName.Should().Be(newName);
        client.DomainEvents.Should().HaveCount(1);
        client.DomainEvents.First().Should().BeOfType<ClientUpdatedEvent>();
    }

    [Fact]
    public void UpdatePersonName_BusinessClient_ThrowsException()
    {
        // Arrange
        var client = CreateTestBusinessClient();
        var newName = PersonName.Create("Jane", "Smith");

        // Act
        var act = () => client.UpdatePersonName(newName);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*individual*");
    }

    [Fact]
    public void UpdateBusinessInfo_BusinessClient_UpdatesInfo()
    {
        // Arrange
        var client = CreateTestBusinessClient();
        client.ClearDomainEvents();
        var newInfo = BusinessInfo.Create("New Corp", "Inc");

        // Act
        client.UpdateBusinessInfo(newInfo);

        // Assert
        client.BusinessInfo.Should().Be(newInfo);
        client.DomainEvents.Should().HaveCount(1);
        client.DomainEvents.First().Should().BeOfType<ClientUpdatedEvent>();
    }

    [Fact]
    public void UpdateBusinessInfo_IndividualClient_ThrowsException()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        var newInfo = BusinessInfo.Create("New Corp", "Inc");

        // Act
        var act = () => client.UpdateBusinessInfo(newInfo);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*business*");
    }

    [Fact]
    public void Deactivate_ActiveClient_DeactivatesAndRaisesEvent()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        client.ClearDomainEvents();

        // Act
        client.Deactivate();

        // Assert
        client.Status.Should().Be(ClientStatus.Inactive);
        client.DomainEvents.Should().HaveCount(1);
        client.DomainEvents.First().Should().BeOfType<ClientDeactivatedEvent>();
    }

    [Fact]
    public void Deactivate_InactiveClient_DoesNothing()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        client.Deactivate();
        client.ClearDomainEvents();

        // Act
        client.Deactivate();

        // Assert
        client.Status.Should().Be(ClientStatus.Inactive);
        client.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Reactivate_InactiveClient_Reactivates()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        client.Deactivate();
        client.ClearDomainEvents();

        // Act
        client.Reactivate();

        // Assert
        client.Status.Should().Be(ClientStatus.Active);
    }

    [Fact]
    public void AddContact_ValidInputs_AddsContact()
    {
        // Arrange
        var client = CreateTestBusinessClient();
        client.ClearDomainEvents();
        var contactName = PersonName.Create("Jane", "Smith");

        // Act
        var contact = client.AddContact(contactName, "CEO", null, null, true);

        // Assert
        contact.Should().NotBeNull();
        contact.Name.Should().Be(contactName);
        contact.Title.Should().Be("CEO");
        contact.IsPrimary.Should().BeTrue();
        client.Contacts.Should().HaveCount(1);
        client.DomainEvents.Should().HaveCount(1);
        client.DomainEvents.First().Should().BeOfType<ContactAddedEvent>();
    }

    [Fact]
    public void AddContact_NewPrimaryContact_RemovesPrimaryFromExisting()
    {
        // Arrange
        var client = CreateTestBusinessClient();
        var firstContactName = PersonName.Create("First", "Contact");
        var secondContactName = PersonName.Create("Second", "Contact");
        var firstContact = client.AddContact(firstContactName, isPrimary: true);

        // Act
        var secondContact = client.AddContact(secondContactName, isPrimary: true);

        // Assert
        firstContact.IsPrimary.Should().BeFalse();
        secondContact.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void RemoveContact_ExistingContact_RemovesContact()
    {
        // Arrange
        var client = CreateTestBusinessClient();
        var contactName = PersonName.Create("Jane", "Smith");
        var contact = client.AddContact(contactName);
        client.ClearDomainEvents();

        // Act
        client.RemoveContact(contact.Id);

        // Assert
        client.Contacts.Should().BeEmpty();
        client.DomainEvents.Should().HaveCount(1);
        client.DomainEvents.First().Should().BeOfType<ContactRemovedEvent>();
    }

    [Fact]
    public void RemoveContact_NonExistingContact_ThrowsException()
    {
        // Arrange
        var client = CreateTestBusinessClient();

        // Act
        var act = () => client.RemoveContact(Guid.NewGuid());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void AddAddress_ValidInputs_AddsAddress()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        client.ClearDomainEvents();

        // Act
        var address = client.AddAddress(
            AddressType.Mailing,
            "123 Main St",
            "Anytown",
            "CA",
            "90210",
            isPrimary: true);

        // Assert
        address.Should().NotBeNull();
        address.StreetLine1.Should().Be("123 Main St");
        address.City.Should().Be("Anytown");
        address.State.Should().Be("CA");
        address.PostalCode.Should().Be("90210");
        address.IsPrimary.Should().BeTrue();
        client.Addresses.Should().HaveCount(1);
        client.DomainEvents.Should().HaveCount(1);
        client.DomainEvents.First().Should().BeOfType<AddressAddedEvent>();
    }

    [Fact]
    public void LogCommunication_ValidInputs_LogsCommunication()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        client.ClearDomainEvents();

        // Act
        var communication = client.LogCommunication(
            CommunicationType.Email,
            "Welcome Email",
            "Sent welcome email to new client",
            _userId);

        // Assert
        communication.Should().NotBeNull();
        communication.Type.Should().Be(CommunicationType.Email);
        communication.Subject.Should().Be("Welcome Email");
        communication.Notes.Should().Be("Sent welcome email to new client");
        communication.LoggedBy.Should().Be(_userId);
        client.Communications.Should().HaveCount(1);
        client.DomainEvents.Should().HaveCount(1);
        client.DomainEvents.First().Should().BeOfType<CommunicationLoggedEvent>();
    }

    [Fact]
    public void UpdateEmail_ValidEmail_UpdatesEmail()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        var newEmail = EmailAddress.Create("new@example.com");

        // Act
        client.UpdateEmail(newEmail);

        // Assert
        client.Email.Should().Be(newEmail);
    }

    [Fact]
    public void UpdateEmail_NullEmail_RemovesEmail()
    {
        // Arrange
        var email = EmailAddress.Create("old@example.com");
        var personName = PersonName.Create("John", "Doe");
        var client = Client.CreateIndividual(_tenantId, personName, _userId, email: email);

        // Act
        client.UpdateEmail(null);

        // Assert
        client.Email.Should().BeNull();
    }

    [Fact]
    public void UpdatePhone_ValidPhone_UpdatesPhone()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        var newPhone = PhoneNumber.Create("555-999-0000");

        // Act
        client.UpdatePhone(newPhone);

        // Assert
        client.Phone.Should().Be(newPhone);
    }

    [Fact]
    public void SetPrimaryContact_ExistingContact_SetsPrimaryAndRemovesFromOthers()
    {
        // Arrange
        var client = CreateTestBusinessClient();
        var firstContactName = PersonName.Create("First", "Contact");
        var secondContactName = PersonName.Create("Second", "Contact");
        var firstContact = client.AddContact(firstContactName, isPrimary: true);
        var secondContact = client.AddContact(secondContactName, isPrimary: false);

        // Act
        client.SetPrimaryContact(secondContact.Id);

        // Assert
        firstContact.IsPrimary.Should().BeFalse();
        secondContact.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void SetPrimaryContact_NonExistingContact_ThrowsException()
    {
        // Arrange
        var client = CreateTestBusinessClient();

        // Act
        var act = () => client.SetPrimaryContact(Guid.NewGuid());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Contact*not found*");
    }

    [Fact]
    public void RemoveAddress_ExistingAddress_RemovesAddress()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        var address = client.AddAddress(
            AddressType.Mailing,
            "123 Main St",
            "Anytown",
            "CA",
            "90210");
        client.ClearDomainEvents();

        // Act
        client.RemoveAddress(address.Id);

        // Assert
        client.Addresses.Should().BeEmpty();
        client.DomainEvents.Should().HaveCount(1);
        client.DomainEvents.First().Should().BeOfType<AddressRemovedEvent>();
    }

    [Fact]
    public void RemoveAddress_NonExistingAddress_ThrowsException()
    {
        // Arrange
        var client = CreateTestIndividualClient();

        // Act
        var act = () => client.RemoveAddress(Guid.NewGuid());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Address*not found*");
    }

    [Fact]
    public void SetPrimaryAddress_ExistingAddress_SetsPrimaryForSameType()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        var firstAddress = client.AddAddress(
            AddressType.Mailing,
            "123 First St",
            "Anytown",
            "CA",
            "90210",
            isPrimary: true);
        var secondAddress = client.AddAddress(
            AddressType.Mailing,
            "456 Second St",
            "Othertown",
            "CA",
            "90211",
            isPrimary: false);

        // Act
        client.SetPrimaryAddress(secondAddress.Id);

        // Assert
        firstAddress.IsPrimary.Should().BeFalse();
        secondAddress.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void SetPrimaryAddress_DifferentAddressTypes_OnlyAffectsSameType()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        var mailingAddress = client.AddAddress(
            AddressType.Mailing,
            "123 Mailing St",
            "Anytown",
            "CA",
            "90210",
            isPrimary: true);
        var billingAddress = client.AddAddress(
            AddressType.Billing,
            "456 Billing St",
            "Othertown",
            "CA",
            "90211",
            isPrimary: false);

        // Act
        client.SetPrimaryAddress(billingAddress.Id);

        // Assert - mailing address should still be primary (different type)
        mailingAddress.IsPrimary.Should().BeTrue();
        billingAddress.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void SetPrimaryAddress_NonExistingAddress_ThrowsException()
    {
        // Arrange
        var client = CreateTestIndividualClient();

        // Act
        var act = () => client.SetPrimaryAddress(Guid.NewGuid());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Address*not found*");
    }

    [Fact]
    public void AddAddress_NewPrimaryAddress_RemovesPrimaryFromExistingOfSameType()
    {
        // Arrange
        var client = CreateTestIndividualClient();
        var firstAddress = client.AddAddress(
            AddressType.Mailing,
            "123 First St",
            "Anytown",
            "CA",
            "90210",
            isPrimary: true);

        // Act
        var secondAddress = client.AddAddress(
            AddressType.Mailing,
            "456 Second St",
            "Othertown",
            "CA",
            "90211",
            isPrimary: true);

        // Assert
        firstAddress.IsPrimary.Should().BeFalse();
        secondAddress.IsPrimary.Should().BeTrue();
    }

    private Client CreateTestIndividualClient()
    {
        var personName = PersonName.Create("Test", "User");
        return Client.CreateIndividual(_tenantId, personName, _userId);
    }

    private Client CreateTestBusinessClient()
    {
        var businessInfo = BusinessInfo.Create("Test Business", "LLC");
        return Client.CreateBusiness(_tenantId, businessInfo, _userId);
    }
}
