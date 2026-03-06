using IBS.BuildingBlocks.Infrastructure.Multitenancy;
using IBS.BuildingBlocks.Infrastructure.Persistence;
using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Clients.Domain.Aggregates.Client;
using IBS.Identity.Domain.Aggregates.Permission;
using IBS.Identity.Domain.Aggregates.Role;
using IBS.Identity.Domain.Aggregates.User;
using IBS.Claims.Domain.Aggregates.Claim;
using IBS.Commissions.Domain.Aggregates.CommissionSchedule;
using IBS.Commissions.Domain.Aggregates.CommissionStatement;
using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Aggregates.DocumentTemplate;
using IBS.Policies.Domain.Aggregates.Policy;
using IBS.Policies.Domain.Aggregates.Quote;
using IBS.PolicyAssistant.Domain.Aggregates.Conversation;
using IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;
using IBS.Tenants.Domain.Aggregates.Tenant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IBS.Infrastructure.Persistence;

/// <summary>
/// Main database context for the IBS application.
/// </summary>
public sealed class IbsDbContext : BaseDbContext
{
    /// <summary>
    /// Initializes a new instance of the IbsDbContext class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    /// <param name="tenantContext">The tenant context.</param>
    /// <param name="mediator">The mediator for domain event dispatch (optional for design-time).</param>
    public IbsDbContext(DbContextOptions<IbsDbContext> options, ITenantContext tenantContext, IMediator? mediator = null)
        : base(options, tenantContext, mediator)
    {
    }

    /// <summary>
    /// Gets or sets the Tenants DbSet.
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>
    /// Gets or sets the TenantCarriers DbSet.
    /// </summary>
    public DbSet<TenantCarrier> TenantCarriers => Set<TenantCarrier>();

    /// <summary>
    /// Gets or sets the Users DbSet.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets or sets the Roles DbSet.
    /// </summary>
    public DbSet<Role> Roles => Set<Role>();

    /// <summary>
    /// Gets or sets the Permissions DbSet.
    /// </summary>
    public DbSet<Permission> Permissions => Set<Permission>();

    /// <summary>
    /// Gets or sets the UserRoles DbSet.
    /// </summary>
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    /// <summary>
    /// Gets or sets the RolePermissions DbSet.
    /// </summary>
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    /// <summary>
    /// Gets or sets the RefreshTokens DbSet.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Gets or sets the Clients DbSet.
    /// </summary>
    public DbSet<Client> Clients => Set<Client>();

    /// <summary>
    /// Gets or sets the Contacts DbSet.
    /// </summary>
    public DbSet<Contact> Contacts => Set<Contact>();

    /// <summary>
    /// Gets or sets the Addresses DbSet.
    /// </summary>
    public DbSet<Address> Addresses => Set<Address>();

    /// <summary>
    /// Gets or sets the Communications DbSet.
    /// </summary>
    public DbSet<Communication> Communications => Set<Communication>();

    /// <summary>
    /// Gets or sets the Carriers DbSet.
    /// </summary>
    public DbSet<Carrier> Carriers => Set<Carrier>();

    /// <summary>
    /// Gets or sets the Products DbSet.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Gets or sets the Appetites DbSet.
    /// </summary>
    public DbSet<Appetite> Appetites => Set<Appetite>();

    /// <summary>
    /// Gets or sets the Policies DbSet.
    /// </summary>
    public DbSet<Policy> Policies => Set<Policy>();

    /// <summary>
    /// Gets or sets the Coverages DbSet.
    /// </summary>
    public DbSet<Coverage> Coverages => Set<Coverage>();

    /// <summary>
    /// Gets or sets the Endorsements DbSet.
    /// </summary>
    public DbSet<Endorsement> Endorsements => Set<Endorsement>();

    /// <summary>
    /// Gets or sets the PolicyHistory DbSet.
    /// </summary>
    public DbSet<PolicyHistory> PolicyHistory => Set<PolicyHistory>();

    /// <summary>
    /// Gets or sets the Quotes DbSet.
    /// </summary>
    public DbSet<Quote> Quotes => Set<Quote>();

    /// <summary>
    /// Gets or sets the QuoteCarriers DbSet.
    /// </summary>
    public DbSet<QuoteCarrier> QuoteCarriers => Set<QuoteCarrier>();

    /// <summary>
    /// Gets or sets the Claims DbSet.
    /// </summary>
    public DbSet<Claim> Claims => Set<Claim>();

    /// <summary>
    /// Gets or sets the ClaimNotes DbSet.
    /// </summary>
    public DbSet<ClaimNote> ClaimNotes => Set<ClaimNote>();

    /// <summary>
    /// Gets or sets the ClaimReserves DbSet.
    /// </summary>
    public DbSet<Reserve> ClaimReserves => Set<Reserve>();

    /// <summary>
    /// Gets or sets the ClaimPayments DbSet.
    /// </summary>
    public DbSet<ClaimPayment> ClaimPayments => Set<ClaimPayment>();

    /// <summary>
    /// Gets or sets the CommissionSchedules DbSet.
    /// </summary>
    public DbSet<CommissionSchedule> CommissionSchedules => Set<CommissionSchedule>();

    /// <summary>
    /// Gets or sets the CommissionStatements DbSet.
    /// </summary>
    public DbSet<CommissionStatement> CommissionStatements => Set<CommissionStatement>();

    /// <summary>
    /// Gets or sets the CommissionLineItems DbSet.
    /// </summary>
    public DbSet<CommissionLineItem> CommissionLineItems => Set<CommissionLineItem>();

    /// <summary>
    /// Gets or sets the ProducerSplits DbSet.
    /// </summary>
    public DbSet<ProducerSplit> ProducerSplits => Set<ProducerSplit>();

    /// <summary>
    /// Gets or sets the Documents DbSet.
    /// </summary>
    public DbSet<Document> Documents => Set<Document>();

    /// <summary>
    /// Gets or sets the DocumentTemplates DbSet.
    /// </summary>
    public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();

    /// <summary>
    /// Gets or sets the PolicyAssistantConversations DbSet.
    /// </summary>
    public DbSet<Conversation> PolicyAssistantConversations => Set<Conversation>();

    /// <summary>
    /// Gets or sets the PolicyAssistantReferenceDocuments DbSet.
    /// </summary>
    public DbSet<ReferenceDocument> PolicyAssistantReferenceDocuments => Set<ReferenceDocument>();

    /// <summary>
    /// Gets or sets the AuditLogs DbSet.
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from all context infrastructure assemblies
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(Tenants.Infrastructure.Persistence.Configurations.TenantConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(Identity.Infrastructure.Persistence.Configurations.UserConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(Clients.Infrastructure.Persistence.Configurations.ClientConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(Carriers.Infrastructure.Persistence.Configurations.CarrierConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(Policies.Infrastructure.Persistence.Configurations.PolicyConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(Claims.Infrastructure.Persistence.Configurations.ClaimConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(Commissions.Infrastructure.Persistence.Configurations.CommissionScheduleConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(Documents.Infrastructure.Persistence.Configurations.DocumentConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PolicyAssistant.Infrastructure.Persistence.Configurations.ConversationConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(Configurations.AuditLogConfiguration).Assembly);

        // Apply global conventions (tenant filters, RowVersion) AFTER all entity types
        // have been discovered by ApplyConfigurationsFromAssembly. This ensures child
        // entities (e.g. ChatMessageEntity) that are only registered through relationship
        // configurations are included in the convention loop.
        ApplyGlobalConventions(modelBuilder);
    }
}
