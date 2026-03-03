-- IBS - Test Data Seed Script
-- This script populates the database with sample data for testing
-- Run this after applying EF migrations

-- =====================================================
-- TENANT DATA
-- =====================================================
DECLARE @TenantId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();
DECLARE @EmptyGuid UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000';

-- Insert Demo Tenant
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Id = @TenantId)
BEGIN
    INSERT INTO Tenants (Id, Name, Subdomain, Status, SubscriptionTier, CreatedAt, UpdatedAt)
    VALUES (@TenantId, 'Demo Insurance Agency', 'demo', 'Active', 'Professional', @Now, @Now);
    PRINT 'Created Demo Tenant';
END

-- Tenant Carrier Relationships
IF NOT EXISTS (SELECT 1 FROM TenantCarriers WHERE TenantId = @TenantId)
BEGIN
    -- Will be populated after carriers are created (see below)
    PRINT 'Tenant carrier relationships will be created after carriers';
END

-- =====================================================
-- USER DATA
-- =====================================================
-- Password hashes are BCrypt hashes with work factor 12
-- Admin123!, Agent123!, User123!

DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @AgentUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @UserUserId UNIQUEIDENTIFIER = NEWID();

-- Admin User (Password: Admin123!)
IF NOT EXISTS (SELECT 1 FROM Users WHERE NormalizedEmail = 'ADMIN@TEST.COM' AND TenantId = @TenantId)
BEGIN
    SET @AdminUserId = NEWID();
    INSERT INTO Users (Id, TenantId, Email, NormalizedEmail, PasswordHash, FirstName, LastName, Title, Status, EmailConfirmed, FailedLoginAttempts, CreatedAt, UpdatedAt)
    VALUES (@AdminUserId, @TenantId, 'admin@test.com', 'ADMIN@TEST.COM',
            '$2a$12$4a/gXy0K3izeVunxijt5lO1b8.1.KCJ9t9YVs9gF4o1xqrfyRi0aK',
            'Admin', 'User', 'System Administrator', 'Active', 1, 0, @Now, @Now);
    PRINT 'Created Admin User';
END
ELSE
    SELECT @AdminUserId = Id FROM Users WHERE NormalizedEmail = 'ADMIN@TEST.COM' AND TenantId = @TenantId;

-- Agent User (Password: Agent123!)
IF NOT EXISTS (SELECT 1 FROM Users WHERE NormalizedEmail = 'AGENT@TEST.COM' AND TenantId = @TenantId)
BEGIN
    SET @AgentUserId = NEWID();
    INSERT INTO Users (Id, TenantId, Email, NormalizedEmail, PasswordHash, FirstName, LastName, Title, Status, EmailConfirmed, FailedLoginAttempts, CreatedAt, UpdatedAt)
    VALUES (@AgentUserId, @TenantId, 'agent@test.com', 'AGENT@TEST.COM',
            '$2a$12$Bjv8n4Bw63J9bjQV2czmz.tjJRv.Bc7.dpwdhU5Xw/9O89pS8/L.K',
            'John', 'Agent', 'Insurance Agent', 'Active', 1, 0, @Now, @Now);
    PRINT 'Created Agent User';
END
ELSE
    SELECT @AgentUserId = Id FROM Users WHERE NormalizedEmail = 'AGENT@TEST.COM' AND TenantId = @TenantId;

-- Regular User (Password: User123!)
IF NOT EXISTS (SELECT 1 FROM Users WHERE NormalizedEmail = 'USER@TEST.COM' AND TenantId = @TenantId)
BEGIN
    SET @UserUserId = NEWID();
    INSERT INTO Users (Id, TenantId, Email, NormalizedEmail, PasswordHash, FirstName, LastName, Title, Status, EmailConfirmed, FailedLoginAttempts, CreatedAt, UpdatedAt)
    VALUES (@UserUserId, @TenantId, 'user@test.com', 'USER@TEST.COM',
            '$2a$12$9/0DJOvWyClywet/eI6OXu25jDo4n9SBQup.QOj.7orcO8/b5Rdz6',
            'Jane', 'Smith', 'Account Manager', 'Active', 1, 0, @Now, @Now);
    PRINT 'Created Regular User';
END
ELSE
    SELECT @UserUserId = Id FROM Users WHERE NormalizedEmail = 'USER@TEST.COM' AND TenantId = @TenantId;

-- =====================================================
-- PERMISSIONS DATA (System-defined, no TenantId)
-- =====================================================
-- These match the Permissions static class in IBS.Identity.Domain

-- Client permissions
DECLARE @PermClientsRead UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000001';
DECLARE @PermClientsCreate UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000002';
DECLARE @PermClientsUpdate UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000003';
DECLARE @PermClientsDelete UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000004';

-- Policy permissions
DECLARE @PermPoliciesRead UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000011';
DECLARE @PermPoliciesCreate UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000012';
DECLARE @PermPoliciesBind UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000013';
DECLARE @PermPoliciesCancel UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000014';

-- Claims permissions
DECLARE @PermClaimsRead UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000021';
DECLARE @PermClaimsCreate UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000022';
DECLARE @PermClaimsUpdate UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000023';
DECLARE @PermClaimsClose UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000024';

-- Quote permissions
DECLARE @PermQuotesRead UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000031';
DECLARE @PermQuotesCreate UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000032';
DECLARE @PermQuotesSubmit UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000033';

-- Report permissions
DECLARE @PermReportsView UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000041';
DECLARE @PermReportsExport UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000042';

-- Admin permissions
DECLARE @PermAdminUsers UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000051';
DECLARE @PermAdminSettings UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000052';
DECLARE @PermAdminCarriers UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000053';

-- Commission permissions
DECLARE @PermCommissionsRead UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000061';
DECLARE @PermCommissionsCreate UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000062';
DECLARE @PermCommissionsUpdate UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000063';
DECLARE @PermCommissionsReconcile UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000064';

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Id = @PermClientsRead)
BEGIN
    INSERT INTO Permissions (Id, Name, Module, Description, CreatedAt, UpdatedAt) VALUES
        (@PermClientsRead,    'clients:read',    'Clients',  'View clients',            @Now, @Now),
        (@PermClientsCreate,  'clients:create',  'Clients',  'Create clients',          @Now, @Now),
        (@PermClientsUpdate,  'clients:update',  'Clients',  'Update clients',          @Now, @Now),
        (@PermClientsDelete,  'clients:delete',  'Clients',  'Delete clients',          @Now, @Now),
        (@PermPoliciesRead,   'policies:read',   'Policies', 'View policies',           @Now, @Now),
        (@PermPoliciesCreate, 'policies:create', 'Policies', 'Create policies',         @Now, @Now),
        (@PermPoliciesBind,   'policies:bind',   'Policies', 'Bind policies',           @Now, @Now),
        (@PermPoliciesCancel, 'policies:cancel', 'Policies', 'Cancel policies',         @Now, @Now),
        (@PermClaimsRead,     'claims:read',     'Claims',   'View claims',             @Now, @Now),
        (@PermClaimsCreate,   'claims:create',   'Claims',   'Create claims',           @Now, @Now),
        (@PermClaimsUpdate,   'claims:update',   'Claims',   'Update claims',           @Now, @Now),
        (@PermClaimsClose,    'claims:close',    'Claims',   'Close claims',            @Now, @Now),
        (@PermQuotesRead,     'quotes:read',     'Quotes',   'View quotes',             @Now, @Now),
        (@PermQuotesCreate,   'quotes:create',   'Quotes',   'Create quotes',           @Now, @Now),
        (@PermQuotesSubmit,   'quotes:submit',   'Quotes',   'Submit quotes',           @Now, @Now),
        (@PermReportsView,    'reports:view',    'Reports',  'View reports',            @Now, @Now),
        (@PermReportsExport,  'reports:export',  'Reports',  'Export reports',          @Now, @Now),
        (@PermAdminUsers,     'admin:users',     'Admin',    'Manage users',            @Now, @Now),
        (@PermAdminSettings,  'admin:settings',  'Admin',    'Manage settings',         @Now, @Now),
        (@PermAdminCarriers,  'admin:carriers',  'Admin',    'Manage carriers',         @Now, @Now),
        (@PermCommissionsRead,      'commissions:read',      'Commissions', 'View commission schedules and statements',        @Now, @Now),
        (@PermCommissionsCreate,    'commissions:create',    'Commissions', 'Create commission schedules and statements',      @Now, @Now),
        (@PermCommissionsUpdate,    'commissions:update',    'Commissions', 'Update commission schedules and statements',      @Now, @Now),
        (@PermCommissionsReconcile, 'commissions:reconcile', 'Commissions', 'Reconcile and manage commission statements',      @Now, @Now);
    PRINT 'Created Permissions';
END

-- =====================================================
-- ROLES & ROLE-PERMISSION DATA
-- =====================================================
DECLARE @AdminRoleId UNIQUEIDENTIFIER = '20000000-0000-0000-0000-000000000001';
DECLARE @AgentRoleId UNIQUEIDENTIFIER = '20000000-0000-0000-0000-000000000002';
DECLARE @UserRoleId UNIQUEIDENTIFIER = '20000000-0000-0000-0000-000000000003';

-- System Roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = @AdminRoleId)
BEGIN
    INSERT INTO Roles (Id, TenantId, Name, NormalizedName, Description, IsSystemRole, CreatedAt, UpdatedAt) VALUES
        (@AdminRoleId, NULL, 'Admin', 'ADMIN', 'Full system administrator with all permissions', 1, @Now, @Now),
        (@AgentRoleId, NULL, 'Agent', 'AGENT', 'Insurance agent with client and policy management', 1, @Now, @Now),
        (@UserRoleId,  NULL, 'User',  'USER',  'Basic user with read-only access', 1, @Now, @Now);

    -- Admin gets ALL permissions
    INSERT INTO RolePermissions (Id, RoleId, PermissionId, GrantedAt, CreatedAt, UpdatedAt) VALUES
        (NEWID(), @AdminRoleId, @PermClientsRead, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermClientsCreate, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermClientsUpdate, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermClientsDelete, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermPoliciesRead, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermPoliciesCreate, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermPoliciesBind, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermPoliciesCancel, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermClaimsRead, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermClaimsCreate, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermClaimsUpdate, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermClaimsClose, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermQuotesRead, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermQuotesCreate, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermQuotesSubmit, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermReportsView, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermReportsExport, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermAdminUsers, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermAdminSettings, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermAdminCarriers, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermCommissionsRead, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermCommissionsCreate, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermCommissionsUpdate, @Now, @Now, @Now),
        (NEWID(), @AdminRoleId, @PermCommissionsReconcile, @Now, @Now, @Now);

    -- Agent gets client, policy, claims, quotes, and reports (no admin)
    INSERT INTO RolePermissions (Id, RoleId, PermissionId, GrantedAt, CreatedAt, UpdatedAt) VALUES
        (NEWID(), @AgentRoleId, @PermClientsRead, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermClientsCreate, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermClientsUpdate, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermPoliciesRead, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermPoliciesCreate, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermPoliciesBind, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermClaimsRead, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermClaimsCreate, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermClaimsUpdate, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermQuotesRead, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermQuotesCreate, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermQuotesSubmit, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermReportsView, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermCommissionsRead, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermCommissionsCreate, @Now, @Now, @Now),
        (NEWID(), @AgentRoleId, @PermCommissionsUpdate, @Now, @Now, @Now);

    -- User gets read-only access
    INSERT INTO RolePermissions (Id, RoleId, PermissionId, GrantedAt, CreatedAt, UpdatedAt) VALUES
        (NEWID(), @UserRoleId, @PermClientsRead, @Now, @Now, @Now),
        (NEWID(), @UserRoleId, @PermPoliciesRead, @Now, @Now, @Now),
        (NEWID(), @UserRoleId, @PermClaimsRead, @Now, @Now, @Now),
        (NEWID(), @UserRoleId, @PermQuotesRead, @Now, @Now, @Now),
        (NEWID(), @UserRoleId, @PermReportsView, @Now, @Now, @Now),
        (NEWID(), @UserRoleId, @PermCommissionsRead, @Now, @Now, @Now);

    PRINT 'Created Roles and Role-Permission assignments';
END

-- =====================================================
-- USER-ROLE ASSIGNMENTS
-- =====================================================

-- Admin user gets Admin role
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO UserRoles (Id, UserId, RoleId, AssignedAt, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @AdminUserId, @AdminRoleId, @Now, @Now, @Now);
    PRINT 'Assigned Admin role to admin user';
END

-- Agent user gets Agent role
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @AgentUserId AND RoleId = @AgentRoleId)
BEGIN
    INSERT INTO UserRoles (Id, UserId, RoleId, AssignedAt, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @AgentUserId, @AgentRoleId, @Now, @Now, @Now);
    PRINT 'Assigned Agent role to agent user';
END

-- Regular user gets User role
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserUserId AND RoleId = @UserRoleId)
BEGIN
    INSERT INTO UserRoles (Id, UserId, RoleId, AssignedAt, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @UserUserId, @UserRoleId, @Now, @Now, @Now);
    PRINT 'Assigned User role to regular user';
END

-- =====================================================
-- CARRIER DATA (No TenantId - Carriers are global)
-- =====================================================
DECLARE @Carrier1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Carrier2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Carrier3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Carrier4Id UNIQUEIDENTIFIER = NEWID();

-- State Farm
IF NOT EXISTS (SELECT 1 FROM Carriers WHERE Code = 'STATEFARM')
BEGIN
    SET @Carrier1Id = NEWID();
    INSERT INTO Carriers (Id, Code, Name, LegalName, AmBestRating, NaicCode, Status, WebsiteUrl, CreatedAt, UpdatedAt)
    VALUES (@Carrier1Id, 'STATEFARM', 'State Farm', 'State Farm Mutual Automobile Insurance Company',
            'A++', '25178', 'Active', 'https://www.statefarm.com', @Now, @Now);

    -- Products
    INSERT INTO Products (Id, CarrierId, Code, Name, LineOfBusiness, IsActive, CreatedAt, UpdatedAt)
    VALUES
        (NEWID(), @Carrier1Id, 'SF-GL', 'Commercial General Liability', 'GeneralLiability', 1, @Now, @Now),
        (NEWID(), @Carrier1Id, 'SF-CA', 'Commercial Auto', 'CommercialAuto', 1, @Now, @Now),
        (NEWID(), @Carrier1Id, 'SF-WC', 'Workers Compensation', 'WorkersCompensation', 1, @Now, @Now);

    PRINT 'Created State Farm Carrier';
END
ELSE
    SELECT @Carrier1Id = Id FROM Carriers WHERE Code = 'STATEFARM';

-- Liberty Mutual
IF NOT EXISTS (SELECT 1 FROM Carriers WHERE Code = 'LIBERTY')
BEGIN
    SET @Carrier2Id = NEWID();
    INSERT INTO Carriers (Id, Code, Name, LegalName, AmBestRating, NaicCode, Status, WebsiteUrl, CreatedAt, UpdatedAt)
    VALUES (@Carrier2Id, 'LIBERTY', 'Liberty Mutual', 'Liberty Mutual Insurance Company',
            'A', '23043', 'Active', 'https://www.libertymutual.com', @Now, @Now);

    -- Products
    INSERT INTO Products (Id, CarrierId, Code, Name, LineOfBusiness, IsActive, CreatedAt, UpdatedAt)
    VALUES
        (NEWID(), @Carrier2Id, 'LM-CP', 'Commercial Property', 'CommercialProperty', 1, @Now, @Now),
        (NEWID(), @Carrier2Id, 'LM-GL', 'General Liability', 'GeneralLiability', 1, @Now, @Now),
        (NEWID(), @Carrier2Id, 'LM-UMB', 'Umbrella/Excess', 'CommercialUmbrella', 1, @Now, @Now);

    PRINT 'Created Liberty Mutual Carrier';
END
ELSE
    SELECT @Carrier2Id = Id FROM Carriers WHERE Code = 'LIBERTY';

-- Travelers
IF NOT EXISTS (SELECT 1 FROM Carriers WHERE Code = 'TRAVELERS')
BEGIN
    SET @Carrier3Id = NEWID();
    INSERT INTO Carriers (Id, Code, Name, LegalName, AmBestRating, NaicCode, Status, WebsiteUrl, CreatedAt, UpdatedAt)
    VALUES (@Carrier3Id, 'TRAVELERS', 'Travelers', 'The Travelers Indemnity Company',
            'A++', '25658', 'Active', 'https://www.travelers.com', @Now, @Now);

    -- Products
    INSERT INTO Products (Id, CarrierId, Code, Name, LineOfBusiness, IsActive, CreatedAt, UpdatedAt)
    VALUES
        (NEWID(), @Carrier3Id, 'TV-CYBER', 'Cyber Liability', 'CyberLiability', 1, @Now, @Now),
        (NEWID(), @Carrier3Id, 'TV-DO', 'Directors & Officers', 'DirectorsAndOfficers', 1, @Now, @Now),
        (NEWID(), @Carrier3Id, 'TV-EPL', 'Employment Practices Liability', 'ProfessionalLiability', 1, @Now, @Now);

    PRINT 'Created Travelers Carrier';
END
ELSE
    SELECT @Carrier3Id = Id FROM Carriers WHERE Code = 'TRAVELERS';

-- Hartford (Inactive for testing)
IF NOT EXISTS (SELECT 1 FROM Carriers WHERE Code = 'HARTFORD')
BEGIN
    SET @Carrier4Id = NEWID();
    INSERT INTO Carriers (Id, Code, Name, LegalName, AmBestRating, NaicCode, Status, WebsiteUrl, CreatedAt, UpdatedAt)
    VALUES (@Carrier4Id, 'HARTFORD', 'The Hartford', 'Hartford Fire Insurance Company',
            'A+', '19682', 'Inactive', 'https://www.thehartford.com', @Now, @Now);

    PRINT 'Created Hartford Carrier (Inactive)';
END
ELSE
    SELECT @Carrier4Id = Id FROM Carriers WHERE Code = 'HARTFORD';

-- =====================================================
-- TENANT-CARRIER RELATIONSHIPS
-- =====================================================

-- Link carriers to tenant (agency appointments)
IF NOT EXISTS (SELECT 1 FROM TenantCarriers WHERE TenantId = @TenantId AND CarrierId = @Carrier1Id)
BEGIN
    INSERT INTO TenantCarriers (Id, TenantId, CarrierId, AgencyCode, CommissionRate, IsActive, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @TenantId, @Carrier1Id, 'SF-AGC-001', 0.15, 1, @Now, @Now);
    PRINT 'Created State Farm carrier relationship';
END

IF NOT EXISTS (SELECT 1 FROM TenantCarriers WHERE TenantId = @TenantId AND CarrierId = @Carrier2Id)
BEGIN
    INSERT INTO TenantCarriers (Id, TenantId, CarrierId, AgencyCode, CommissionRate, IsActive, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @TenantId, @Carrier2Id, 'LM-AGC-002', 0.12, 1, @Now, @Now);
    PRINT 'Created Liberty Mutual carrier relationship';
END

IF NOT EXISTS (SELECT 1 FROM TenantCarriers WHERE TenantId = @TenantId AND CarrierId = @Carrier3Id)
BEGIN
    INSERT INTO TenantCarriers (Id, TenantId, CarrierId, AgencyCode, CommissionRate, IsActive, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @TenantId, @Carrier3Id, 'TV-AGC-003', 0.10, 1, @Now, @Now);
    PRINT 'Created Travelers carrier relationship';
END

-- =====================================================
-- CLIENT DATA
-- =====================================================
DECLARE @Client1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Client2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Client3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Client4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Client5Id UNIQUEIDENTIFIER = NEWID();

-- Business Client 1: Acme Corporation
IF NOT EXISTS (SELECT 1 FROM Clients WHERE TenantId = @TenantId AND BusinessName = 'Acme Corporation')
BEGIN
    SET @Client1Id = NEWID();
    INSERT INTO Clients (Id, TenantId, ClientType, BusinessName, DbaName, Email, Phone, Status, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (@Client1Id, @TenantId, 'Business', 'Acme Corporation', 'Acme Corp',
            'info@acmecorp.com', '555-100-1000', 'Active', @AdminUserId, @Now, @Now);

    -- Contacts
    INSERT INTO Contacts (Id, ClientId, TenantId, FirstName, LastName, Email, Phone, Title, IsPrimary, CreatedAt, UpdatedAt)
    VALUES
        (NEWID(), @Client1Id, @TenantId, 'Robert', 'Johnson', 'rjohnson@acmecorp.com', '555-100-1001', 'CEO', 1, @Now, @Now),
        (NEWID(), @Client1Id, @TenantId, 'Sarah', 'Williams', 'swilliams@acmecorp.com', '555-100-1002', 'CFO', 0, @Now, @Now);

    -- Addresses
    INSERT INTO Addresses (Id, ClientId, TenantId, AddressType, StreetLine1, StreetLine2, City, State, PostalCode, Country, IsPrimary, CreatedAt, UpdatedAt)
    VALUES
        (NEWID(), @Client1Id, @TenantId, 'Physical', '100 Main Street', 'Suite 500', 'New York', 'NY', '10001', 'USA', 1, @Now, @Now),
        (NEWID(), @Client1Id, @TenantId, 'Mailing', 'PO Box 1234', NULL, 'New York', 'NY', '10001', 'USA', 0, @Now, @Now);

    PRINT 'Created Acme Corporation Client';
END
ELSE
    SELECT @Client1Id = Id FROM Clients WHERE TenantId = @TenantId AND BusinessName = 'Acme Corporation';

-- Business Client 2: TechStart Inc
IF NOT EXISTS (SELECT 1 FROM Clients WHERE TenantId = @TenantId AND BusinessName = 'TechStart Inc')
BEGIN
    SET @Client2Id = NEWID();
    INSERT INTO Clients (Id, TenantId, ClientType, BusinessName, DbaName, Email, Phone, Status, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (@Client2Id, @TenantId, 'Business', 'TechStart Inc', 'TechStart',
            'contact@techstart.io', '555-200-2000', 'Active', @AdminUserId, @Now, @Now);

    -- Contacts
    INSERT INTO Contacts (Id, ClientId, TenantId, FirstName, LastName, Email, Phone, Title, IsPrimary, CreatedAt, UpdatedAt)
    VALUES
        (NEWID(), @Client2Id, @TenantId, 'Emily', 'Chen', 'echen@techstart.io', '555-200-2001', 'Founder', 1, @Now, @Now);

    -- Addresses
    INSERT INTO Addresses (Id, ClientId, TenantId, AddressType, StreetLine1, City, State, PostalCode, Country, IsPrimary, CreatedAt, UpdatedAt)
    VALUES
        (NEWID(), @Client2Id, @TenantId, 'Physical', '456 Innovation Drive', 'San Francisco', 'CA', '94105', 'USA', 1, @Now, @Now);

    PRINT 'Created TechStart Inc Client';
END
ELSE
    SELECT @Client2Id = Id FROM Clients WHERE TenantId = @TenantId AND BusinessName = 'TechStart Inc';

-- Business Client 3: Green Construction LLC
IF NOT EXISTS (SELECT 1 FROM Clients WHERE TenantId = @TenantId AND BusinessName = 'Green Construction LLC')
BEGIN
    SET @Client3Id = NEWID();
    INSERT INTO Clients (Id, TenantId, ClientType, BusinessName, Email, Phone, Status, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (@Client3Id, @TenantId, 'Business', 'Green Construction LLC',
            'office@greenconstruction.com', '555-300-3000', 'Active', @AdminUserId, @Now, @Now);

    -- Contacts
    INSERT INTO Contacts (Id, ClientId, TenantId, FirstName, LastName, Email, Phone, Title, IsPrimary, CreatedAt, UpdatedAt)
    VALUES
        (NEWID(), @Client3Id, @TenantId, 'Michael', 'Green', 'mgreen@greenconstruction.com', '555-300-3001', 'Owner', 1, @Now, @Now);

    -- Addresses
    INSERT INTO Addresses (Id, ClientId, TenantId, AddressType, StreetLine1, City, State, PostalCode, Country, IsPrimary, CreatedAt, UpdatedAt)
    VALUES
        (NEWID(), @Client3Id, @TenantId, 'Physical', '789 Builder Lane', 'Austin', 'TX', '78701', 'USA', 1, @Now, @Now);

    PRINT 'Created Green Construction LLC Client';
END
ELSE
    SELECT @Client3Id = Id FROM Clients WHERE TenantId = @TenantId AND BusinessName = 'Green Construction LLC';

-- Individual Client 1: John Smith
IF NOT EXISTS (SELECT 1 FROM Clients WHERE TenantId = @TenantId AND Email = 'john.smith@email.com')
BEGIN
    SET @Client4Id = NEWID();
    INSERT INTO Clients (Id, TenantId, ClientType, FirstName, LastName, Email, Phone, DateOfBirth, Status, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (@Client4Id, @TenantId, 'Individual', 'John', 'Smith',
            'john.smith@email.com', '555-400-4000', '1985-03-15', 'Active', @AdminUserId, @Now, @Now);

    -- Addresses
    INSERT INTO Addresses (Id, ClientId, TenantId, AddressType, StreetLine1, City, State, PostalCode, Country, IsPrimary, CreatedAt, UpdatedAt)
    VALUES
        (NEWID(), @Client4Id, @TenantId, 'Physical', '123 Residential Ave', 'Chicago', 'IL', '60601', 'USA', 1, @Now, @Now);

    PRINT 'Created John Smith Client';
END
ELSE
    SELECT @Client4Id = Id FROM Clients WHERE TenantId = @TenantId AND Email = 'john.smith@email.com';

-- Individual Client 2: Maria Garcia (Inactive for testing)
IF NOT EXISTS (SELECT 1 FROM Clients WHERE TenantId = @TenantId AND Email = 'maria.garcia@email.com')
BEGIN
    SET @Client5Id = NEWID();
    INSERT INTO Clients (Id, TenantId, ClientType, FirstName, LastName, Email, Phone, DateOfBirth, Status, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (@Client5Id, @TenantId, 'Individual', 'Maria', 'Garcia',
            'maria.garcia@email.com', '555-500-5000', '1990-07-22', 'Inactive', @AdminUserId, @Now, @Now);

    PRINT 'Created Maria Garcia Client (Inactive)';
END
ELSE
    SELECT @Client5Id = Id FROM Clients WHERE TenantId = @TenantId AND Email = 'maria.garcia@email.com';

-- =====================================================
-- COMMUNICATION LOG DATA
-- =====================================================

-- Add sample communication logs for Acme Corporation
IF @Client1Id IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Communications WHERE ClientId = @Client1Id)
    BEGIN
        INSERT INTO Communications (Id, ClientId, TenantId, Type, Subject, Notes, CommunicatedAt, LoggedBy, CreatedAt, UpdatedAt)
        VALUES
            (NEWID(), @Client1Id, @TenantId, 'Phone', 'Initial Consultation', 'Discussed insurance needs and current coverage gaps.', @Now, @AdminUserId, @Now, @Now),
            (NEWID(), @Client1Id, @TenantId, 'Email', 'Quote Proposal Sent', 'Sent comprehensive GL and WC quote package.', @Now, @AgentUserId, @Now, @Now),
            (NEWID(), @Client1Id, @TenantId, 'Meeting', 'Policy Review Meeting', 'Annual review of all active policies. Client satisfied with coverage.', @Now, @AgentUserId, @Now, @Now);

        PRINT 'Created Communication Logs for Acme Corporation';
    END
END

-- =====================================================
-- POLICY DATA
-- =====================================================
DECLARE @Policy1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Policy2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Policy3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Policy4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Policy5Id UNIQUEIDENTIFIER = NEWID();

-- Active Policy: Acme Corp GL
IF @Client1Id IS NOT NULL AND @Carrier1Id IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Policies WHERE PolicyNumber = 'POL-2024-0001')
    BEGIN
        SET @Policy1Id = NEWID();
        INSERT INTO Policies (Id, TenantId, ClientId, CarrierId, PolicyNumber, LineOfBusiness, PolicyType,
                              Status, EffectiveDate, ExpirationDate, TotalPremium, PremiumCurrency, BillingType, PaymentPlan,
                              CreatedBy, CreatedAt, UpdatedAt)
        VALUES (@Policy1Id, @TenantId, @Client1Id, @Carrier1Id, 'POL-2024-0001', 'GeneralLiability',
                'Commercial General Liability', 'Active',
                DATEADD(MONTH, -6, CAST(GETDATE() AS DATE)), DATEADD(MONTH, 6, CAST(GETDATE() AS DATE)),
                12500.00, 'USD', 'DirectBill', 'Annual', @AdminUserId, @Now, @Now);

        -- Coverages
        INSERT INTO Coverages (Id, PolicyId, Code, Name, LimitAmount, LimitCurrency, DeductibleAmount, DeductibleCurrency, PremiumAmount, PremiumCurrency, IsActive, CreatedAt, UpdatedAt)
        VALUES
            (NEWID(), @Policy1Id, 'EO', 'Each Occurrence', 1000000, 'USD', 5000, 'USD', 8500.00, 'USD', 1, @Now, @Now),
            (NEWID(), @Policy1Id, 'GA', 'General Aggregate', 2000000, 'USD', 0, 'USD', 2500.00, 'USD', 1, @Now, @Now),
            (NEWID(), @Policy1Id, 'PCO', 'Products/Completed Ops', 1000000, 'USD', 5000, 'USD', 1500.00, 'USD', 1, @Now, @Now);

        PRINT 'Created Acme Corp GL Policy';
    END
END

-- Active Policy: Acme Corp Workers Comp
IF @Client1Id IS NOT NULL AND @Carrier1Id IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Policies WHERE PolicyNumber = 'POL-2024-0002')
    BEGIN
        SET @Policy2Id = NEWID();
        INSERT INTO Policies (Id, TenantId, ClientId, CarrierId, PolicyNumber, LineOfBusiness, PolicyType,
                              Status, EffectiveDate, ExpirationDate, TotalPremium, PremiumCurrency, BillingType, PaymentPlan,
                              CreatedBy, CreatedAt, UpdatedAt)
        VALUES (@Policy2Id, @TenantId, @Client1Id, @Carrier1Id, 'POL-2024-0002', 'WorkersCompensation',
                'Workers Compensation', 'Active',
                DATEADD(MONTH, -3, CAST(GETDATE() AS DATE)), DATEADD(MONTH, 9, CAST(GETDATE() AS DATE)),
                45000.00, 'USD', 'AgencyBill', 'Quarterly', @AdminUserId, @Now, @Now);

        -- Coverages
        INSERT INTO Coverages (Id, PolicyId, Code, Name, LimitAmount, LimitCurrency, PremiumAmount, PremiumCurrency, IsActive, CreatedAt, UpdatedAt)
        VALUES
            (NEWID(), @Policy2Id, 'WCS', 'Workers Comp Statutory', 1000000, 'USD', 35000.00, 'USD', 1, @Now, @Now),
            (NEWID(), @Policy2Id, 'EL', 'Employers Liability', 500000, 'USD', 10000.00, 'USD', 1, @Now, @Now);

        PRINT 'Created Acme Corp WC Policy';
    END
END

-- Expiring Soon Policy: TechStart Cyber (expires in 15 days)
IF @Client2Id IS NOT NULL AND @Carrier3Id IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Policies WHERE PolicyNumber = 'POL-2024-0003')
    BEGIN
        SET @Policy3Id = NEWID();
        INSERT INTO Policies (Id, TenantId, ClientId, CarrierId, PolicyNumber, LineOfBusiness, PolicyType,
                              Status, EffectiveDate, ExpirationDate, TotalPremium, PremiumCurrency, BillingType, PaymentPlan,
                              CreatedBy, CreatedAt, UpdatedAt)
        VALUES (@Policy3Id, @TenantId, @Client2Id, @Carrier3Id, 'POL-2024-0003', 'CyberLiability',
                'Cyber Liability', 'Active',
                DATEADD(YEAR, -1, DATEADD(DAY, 15, CAST(GETDATE() AS DATE))), DATEADD(DAY, 15, CAST(GETDATE() AS DATE)),
                8500.00, 'USD', 'DirectBill', 'Annual', @AdminUserId, @Now, @Now);

        -- Coverages
        INSERT INTO Coverages (Id, PolicyId, Code, Name, LimitAmount, LimitCurrency, DeductibleAmount, DeductibleCurrency, PremiumAmount, PremiumCurrency, IsActive, CreatedAt, UpdatedAt)
        VALUES
            (NEWID(), @Policy3Id, 'DB', 'Data Breach', 1000000, 'USD', 10000, 'USD', 5000.00, 'USD', 1, @Now, @Now),
            (NEWID(), @Policy3Id, 'NS', 'Network Security', 500000, 'USD', 5000, 'USD', 2500.00, 'USD', 1, @Now, @Now),
            (NEWID(), @Policy3Id, 'BI', 'Business Interruption', 250000, 'USD', 2500, 'USD', 1000.00, 'USD', 1, @Now, @Now);

        PRINT 'Created TechStart Cyber Policy (Expiring Soon)';
    END
END

-- Draft Policy: Green Construction GL
IF @Client3Id IS NOT NULL AND @Carrier2Id IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Policies WHERE PolicyNumber = 'POL-2024-0004')
    BEGIN
        SET @Policy4Id = NEWID();
        INSERT INTO Policies (Id, TenantId, ClientId, CarrierId, PolicyNumber, LineOfBusiness, PolicyType,
                              Status, EffectiveDate, ExpirationDate, TotalPremium, PremiumCurrency, BillingType, PaymentPlan,
                              CreatedBy, CreatedAt, UpdatedAt)
        VALUES (@Policy4Id, @TenantId, @Client3Id, @Carrier2Id, 'POL-2024-0004', 'GeneralLiability',
                'Contractors General Liability', 'Draft',
                DATEADD(MONTH, 1, CAST(GETDATE() AS DATE)), DATEADD(MONTH, 13, CAST(GETDATE() AS DATE)),
                22000.00, 'USD', 'AgencyBill', 'Monthly', @AdminUserId, @Now, @Now);

        -- Coverages
        INSERT INTO Coverages (Id, PolicyId, Code, Name, LimitAmount, LimitCurrency, DeductibleAmount, DeductibleCurrency, PremiumAmount, PremiumCurrency, IsActive, CreatedAt, UpdatedAt)
        VALUES
            (NEWID(), @Policy4Id, 'EO', 'Each Occurrence', 2000000, 'USD', 10000, 'USD', 15000.00, 'USD', 1, @Now, @Now),
            (NEWID(), @Policy4Id, 'GA', 'General Aggregate', 4000000, 'USD', 0, 'USD', 5000.00, 'USD', 1, @Now, @Now),
            (NEWID(), @Policy4Id, 'DRP', 'Damage to Rented Premises', 100000, 'USD', 1000, 'USD', 2000.00, 'USD', 1, @Now, @Now);

        PRINT 'Created Green Construction Draft Policy';
    END
END

-- Cancelled Policy: For testing cancelled status
IF @Client2Id IS NOT NULL AND @Carrier1Id IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Policies WHERE PolicyNumber = 'POL-2023-0099')
    BEGIN
        SET @Policy5Id = NEWID();
        INSERT INTO Policies (Id, TenantId, ClientId, CarrierId, PolicyNumber, LineOfBusiness, PolicyType,
                              Status, EffectiveDate, ExpirationDate, CancellationDate, CancellationType, CancellationReason,
                              TotalPremium, PremiumCurrency, BillingType, PaymentPlan, CreatedBy, CreatedAt, UpdatedAt)
        VALUES (@Policy5Id, @TenantId, @Client2Id, @Carrier1Id, 'POL-2023-0099', 'CommercialAuto',
                'Commercial Auto', 'Cancelled',
                DATEADD(YEAR, -1, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS DATE),
                DATEADD(MONTH, -2, CAST(GETDATE() AS DATE)), 'InsuredRequest', 'Client requested cancellation',
                6500.00, 'USD', 'DirectBill', 'SemiAnnual', @AdminUserId, @Now, @Now);

        PRINT 'Created Cancelled Policy';
    END
END

-- =====================================================
-- QUOTE DATA
-- =====================================================
DECLARE @Quote1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Quote2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Quote3Id UNIQUEIDENTIFIER = NEWID();

-- Quote 1: Quoted status (for Acme Corp, General Liability, 2 carriers responded)
IF NOT EXISTS (SELECT 1 FROM Quotes WHERE TenantId = @TenantId AND ClientId = @Client2Id AND Status = 'Quoted')
BEGIN
    SET @Quote1Id = NEWID();
    INSERT INTO Quotes (Id, TenantId, ClientId, LineOfBusiness, EffectiveDate, ExpirationDate,
                        Status, ExpiresAt, AcceptedCarrierId, PolicyId, Notes, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (@Quote1Id, @TenantId, @Client2Id, 'GeneralLiability',
            DATEADD(MONTH, 1, CAST(GETDATE() AS DATE)),
            DATEADD(MONTH, 13, CAST(GETDATE() AS DATE)),
            'Quoted',
            DATEADD(MONTH, 2, CAST(GETDATE() AS DATE)),
            NULL, NULL,
            'Requesting GL coverage for Acme Corp expansion.',
            @AgentUserId, @Now, @Now);

    -- Carrier 1: State Farm - Quoted
    INSERT INTO QuoteCarriers (Id, QuoteId, CarrierId, Status, PremiumAmount, PremiumCurrency,
                                DeclinationReason, Conditions, ProposedCoverages, RespondedAt, ExpiresAt, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @Quote1Id, @Carrier1Id, 'Quoted', 4500.00, 'USD',
            NULL, 'Clean loss history required for 3 years', NULL,
            DATEADD(DAY, -5, @Now), DATEADD(MONTH, 1, CAST(GETDATE() AS DATE)),
            @Now, @Now);

    -- Carrier 2: Liberty Mutual - Quoted (lower premium)
    INSERT INTO QuoteCarriers (Id, QuoteId, CarrierId, Status, PremiumAmount, PremiumCurrency,
                                DeclinationReason, Conditions, ProposedCoverages, RespondedAt, ExpiresAt, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @Quote1Id, @Carrier2Id, 'Quoted', 3800.00, 'USD',
            NULL, 'Standard exclusions apply', NULL,
            DATEADD(DAY, -3, @Now), DATEADD(MONTH, 1, CAST(GETDATE() AS DATE)),
            @Now, @Now);

    PRINT 'Created Quoted Quote with 2 carrier responses';
END

-- Quote 2: Draft status (for John Smith, Personal Auto, 1 carrier added)
IF NOT EXISTS (SELECT 1 FROM Quotes WHERE TenantId = @TenantId AND ClientId = @Client1Id AND Status = 'Draft')
BEGIN
    SET @Quote2Id = NEWID();
    INSERT INTO Quotes (Id, TenantId, ClientId, LineOfBusiness, EffectiveDate, ExpirationDate,
                        Status, ExpiresAt, AcceptedCarrierId, PolicyId, Notes, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (@Quote2Id, @TenantId, @Client1Id, 'PersonalAuto',
            DATEADD(MONTH, 2, CAST(GETDATE() AS DATE)),
            DATEADD(MONTH, 14, CAST(GETDATE() AS DATE)),
            'Draft',
            DATEADD(MONTH, 3, CAST(GETDATE() AS DATE)),
            NULL, NULL,
            'New auto policy for John Smith.',
            @AgentUserId, @Now, @Now);

    -- Carrier: State Farm - Pending
    INSERT INTO QuoteCarriers (Id, QuoteId, CarrierId, Status, PremiumAmount, PremiumCurrency,
                                DeclinationReason, Conditions, ProposedCoverages, RespondedAt, ExpiresAt, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @Quote2Id, @Carrier1Id, 'Pending', NULL, NULL,
            NULL, NULL, NULL, NULL, NULL,
            @Now, @Now);

    PRINT 'Created Draft Quote with 1 pending carrier';
END

-- Quote 3: Submitted status (for Global Industries, Commercial Property, 3 carriers)
IF NOT EXISTS (SELECT 1 FROM Quotes WHERE TenantId = @TenantId AND ClientId = @Client3Id AND Status = 'Submitted')
BEGIN
    SET @Quote3Id = NEWID();
    INSERT INTO Quotes (Id, TenantId, ClientId, LineOfBusiness, EffectiveDate, ExpirationDate,
                        Status, ExpiresAt, AcceptedCarrierId, PolicyId, Notes, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (@Quote3Id, @TenantId, @Client3Id, 'CommercialProperty',
            DATEADD(MONTH, 1, CAST(GETDATE() AS DATE)),
            DATEADD(MONTH, 13, CAST(GETDATE() AS DATE)),
            'Submitted',
            DATEADD(MONTH, 2, CAST(GETDATE() AS DATE)),
            NULL, NULL,
            'Commercial property coverage for Global Industries warehouse.',
            @AgentUserId, @Now, @Now);

    -- Carrier 1: State Farm - Pending
    INSERT INTO QuoteCarriers (Id, QuoteId, CarrierId, Status, PremiumAmount, PremiumCurrency,
                                DeclinationReason, Conditions, ProposedCoverages, RespondedAt, ExpiresAt, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @Quote3Id, @Carrier1Id, 'Pending', NULL, NULL,
            NULL, NULL, NULL, NULL, NULL,
            @Now, @Now);

    -- Carrier 2: Liberty Mutual - Pending
    INSERT INTO QuoteCarriers (Id, QuoteId, CarrierId, Status, PremiumAmount, PremiumCurrency,
                                DeclinationReason, Conditions, ProposedCoverages, RespondedAt, ExpiresAt, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @Quote3Id, @Carrier2Id, 'Pending', NULL, NULL,
            NULL, NULL, NULL, NULL, NULL,
            @Now, @Now);

    -- Carrier 3: Travelers - Declined
    INSERT INTO QuoteCarriers (Id, QuoteId, CarrierId, Status, PremiumAmount, PremiumCurrency,
                                DeclinationReason, Conditions, ProposedCoverages, RespondedAt, ExpiresAt, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @Quote3Id, @Carrier3Id, 'Declined', NULL, NULL,
            'Property type not within appetite', NULL, NULL,
            DATEADD(DAY, -1, @Now), NULL,
            @Now, @Now);

    PRINT 'Created Submitted Quote with 3 carriers (1 declined)';
END

-- =====================================================
-- CLAIMS DATA
-- =====================================================
DECLARE @Claim1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Claim2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Claim3Id UNIQUEIDENTIFIER = NEWID();

-- Claim 1: Under Investigation (on Acme Corp GL Policy)
IF @Client1Id IS NOT NULL AND @Policy1Id IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Claims WHERE TenantId = @TenantId AND ClaimNumber = 'CLM-20240315-0001')
    BEGIN
        SET @Claim1Id = NEWID();
        INSERT INTO Claims (Id, TenantId, ClaimNumber, PolicyId, ClientId, Status, LossDate, ReportedDate,
                           LossType, LossDescription, LossAmount, LossCurrency,
                           AssignedAdjusterId, CreatedBy, CreatedAt, UpdatedAt)
        VALUES (@Claim1Id, @TenantId, 'CLM-20240315-0001', @Policy1Id, @Client1Id, 'UnderInvestigation',
                '2024-03-15T10:30:00Z', '2024-03-16T09:00:00Z',
                'PropertyDamage', 'Water damage from burst pipe in main office area. Affecting carpet, drywall, and several workstations.',
                50000.00, 'USD', 'ADJ-001', @AdminUserId, @Now, @Now);

        -- Notes
        INSERT INTO ClaimNotes (Id, ClaimId, Content, AuthorBy, IsInternal, CreatedAt, UpdatedAt)
        VALUES
            (NEWID(), @Claim1Id, 'Initial inspection completed. Damage consistent with water intrusion from burst pipe. Photographs taken and documented.', 'ADJ-001', 0, @Now, @Now),
            (NEWID(), @Claim1Id, 'Client may have had pre-existing water damage issue. Need to verify policy terms.', 'ADJ-001', 1, @Now, @Now);

        -- Reserve
        INSERT INTO ClaimReserves (Id, ClaimId, ReserveType, Amount, Currency, SetBy, SetAt, Notes, CreatedAt, UpdatedAt)
        VALUES
            (NEWID(), @Claim1Id, 'Indemnity', 50000.00, 'USD', 'ADJ-001', @Now, 'Initial reserve based on preliminary damage assessment.', @Now, @Now);

        PRINT 'Created Claim 1: Under Investigation (Water Damage)';
    END
END

-- Claim 2: Approved (on Acme Corp WC Policy)
IF @Client1Id IS NOT NULL AND @Policy2Id IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Claims WHERE TenantId = @TenantId AND ClaimNumber = 'CLM-20240320-0002')
    BEGIN
        SET @Claim2Id = NEWID();
        INSERT INTO Claims (Id, TenantId, ClaimNumber, PolicyId, ClientId, Status, LossDate, ReportedDate,
                           LossType, LossDescription, LossAmount, LossCurrency,
                           ClaimAmount, ClaimCurrency, AssignedAdjusterId,
                           CreatedBy, CreatedAt, UpdatedAt)
        VALUES (@Claim2Id, @TenantId, 'CLM-20240320-0002', @Policy2Id, @Client1Id, 'Approved',
                '2024-03-20T14:00:00Z', '2024-03-20T16:00:00Z',
                'BodilyInjury', 'Employee injured while operating heavy machinery. Required emergency medical attention and ongoing treatment.',
                35000.00, 'USD', 28000.00, 'USD', 'ADJ-002', @AdminUserId, @Now, @Now);

        -- Notes
        INSERT INTO ClaimNotes (Id, ClaimId, Content, AuthorBy, IsInternal, CreatedAt, UpdatedAt)
        VALUES
            (NEWID(), @Claim2Id, 'OSHA incident report filed. Safety team investigation complete.', 'ADJ-002', 0, @Now, @Now);

        -- Reserve
        INSERT INTO ClaimReserves (Id, ClaimId, ReserveType, Amount, Currency, SetBy, SetAt, Notes, CreatedAt, UpdatedAt)
        VALUES
            (NEWID(), @Claim2Id, 'Indemnity', 28000.00, 'USD', 'ADJ-002', @Now, 'Reserve set based on medical bills and lost wages.', @Now, @Now),
            (NEWID(), @Claim2Id, 'Expense', 5000.00, 'USD', 'ADJ-002', @Now, 'Legal and investigation expenses.', @Now, @Now);

        -- Payment (Authorized, not yet issued)
        INSERT INTO ClaimPayments (Id, ClaimId, PaymentType, Amount, Currency, PayeeName,
                                   PaymentDate, CheckNumber, Status, AuthorizedBy, AuthorizedAt,
                                   CreatedAt, UpdatedAt)
        VALUES
            (NEWID(), @Claim2Id, 'Indemnity', 15000.00, 'USD', 'St. Luke''s Medical Center',
             CAST(GETDATE() AS DATE), 'CHK-10001', 'Authorized', @AgentUserId, @Now,
             @Now, @Now);

        PRINT 'Created Claim 2: Approved (Workers Comp Injury)';
    END
END

-- Claim 3: FNOL (on TechStart Cyber Policy)
IF @Client2Id IS NOT NULL AND @Policy3Id IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Claims WHERE TenantId = @TenantId AND ClaimNumber = 'CLM-20240401-0003')
    BEGIN
        SET @Claim3Id = NEWID();
        INSERT INTO Claims (Id, TenantId, ClaimNumber, PolicyId, ClientId, Status, LossDate, ReportedDate,
                           LossType, LossDescription, LossAmount, LossCurrency,
                           CreatedBy, CreatedAt, UpdatedAt)
        VALUES (@Claim3Id, @TenantId, 'CLM-20240401-0003', @Policy3Id, @Client2Id, 'FNOL',
                '2024-04-01T08:00:00Z', '2024-04-01T10:00:00Z',
                'Cyber', 'Suspected ransomware attack on company servers. IT team has isolated affected systems. Forensic investigation pending.',
                100000.00, 'USD', @AgentUserId, @Now, @Now);

        PRINT 'Created Claim 3: FNOL (Cyber Attack)';
    END
END

-- =====================================================
-- COMMISSION SCHEDULE DATA
-- =====================================================
DECLARE @Schedule1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Schedule2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Schedule3Id UNIQUEIDENTIFIER = NEWID();

-- Schedule 1: State Farm - General Liability
IF NOT EXISTS (SELECT 1 FROM CommissionSchedules WHERE TenantId = @TenantId AND CarrierId = @Carrier1Id AND LineOfBusiness = 'GeneralLiability')
BEGIN
    SET @Schedule1Id = NEWID();
    INSERT INTO CommissionSchedules (Id, TenantId, CarrierId, CarrierName, LineOfBusiness, NewBusinessRate, RenewalRate, EffectiveFrom, EffectiveTo, IsActive, CreatedAt, UpdatedAt)
    VALUES (@Schedule1Id, @TenantId, @Carrier1Id, 'State Farm', 'GeneralLiability', 15.0000, 12.0000, '2024-01-01', NULL, 1, @Now, @Now);
    PRINT 'Created Commission Schedule: State Farm - General Liability';
END

-- Schedule 2: Liberty Mutual - Commercial Property
IF NOT EXISTS (SELECT 1 FROM CommissionSchedules WHERE TenantId = @TenantId AND CarrierId = @Carrier2Id AND LineOfBusiness = 'CommercialProperty')
BEGIN
    SET @Schedule2Id = NEWID();
    INSERT INTO CommissionSchedules (Id, TenantId, CarrierId, CarrierName, LineOfBusiness, NewBusinessRate, RenewalRate, EffectiveFrom, EffectiveTo, IsActive, CreatedAt, UpdatedAt)
    VALUES (@Schedule2Id, @TenantId, @Carrier2Id, 'Liberty Mutual', 'CommercialProperty', 12.5000, 10.0000, '2024-01-01', '2025-12-31', 1, @Now, @Now);
    PRINT 'Created Commission Schedule: Liberty Mutual - Commercial Property';
END

-- Schedule 3: Travelers - Cyber Liability
IF NOT EXISTS (SELECT 1 FROM CommissionSchedules WHERE TenantId = @TenantId AND CarrierId = @Carrier3Id AND LineOfBusiness = 'CyberLiability')
BEGIN
    SET @Schedule3Id = NEWID();
    INSERT INTO CommissionSchedules (Id, TenantId, CarrierId, CarrierName, LineOfBusiness, NewBusinessRate, RenewalRate, EffectiveFrom, EffectiveTo, IsActive, CreatedAt, UpdatedAt)
    VALUES (@Schedule3Id, @TenantId, @Carrier3Id, 'Travelers', 'CyberLiability', 10.0000, 8.5000, '2024-06-01', NULL, 1, @Now, @Now);
    PRINT 'Created Commission Schedule: Travelers - Cyber Liability';
END

-- =====================================================
-- COMMISSION STATEMENT DATA
-- =====================================================
DECLARE @Statement1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Statement2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @LineItem1aId UNIQUEIDENTIFIER = NEWID();
DECLARE @LineItem1bId UNIQUEIDENTIFIER = NEWID();
DECLARE @LineItem1cId UNIQUEIDENTIFIER = NEWID();
DECLARE @LineItem2aId UNIQUEIDENTIFIER = NEWID();
DECLARE @LineItem2bId UNIQUEIDENTIFIER = NEWID();

-- Statement 1: State Farm - January 2026 (Received)
IF NOT EXISTS (SELECT 1 FROM CommissionStatements WHERE TenantId = @TenantId AND StatementNumber = 'STMT-SF-202601')
BEGIN
    SET @Statement1Id = NEWID();
    INSERT INTO CommissionStatements (Id, TenantId, CarrierId, CarrierName, StatementNumber, PeriodMonth, PeriodYear, StatementDate,
                                      Status, TotalPremiumAmount, TotalPremiumCurrency, TotalCommissionAmount, TotalCommissionCurrency,
                                      ReceivedAt, CreatedAt, UpdatedAt)
    VALUES (@Statement1Id, @TenantId, @Carrier1Id, 'State Farm', 'STMT-SF-202601', 1, 2026, '2026-02-01',
            'Received', 57500.00, 'USD', 7875.00, 'USD',
            @Now, @Now, @Now);

    -- Line Item 1a: Acme Corp GL - New Business
    SET @LineItem1aId = NEWID();
    INSERT INTO CommissionLineItems (Id, StatementId, PolicyId, PolicyNumber, InsuredName, LineOfBusiness, EffectiveDate,
                                     TransactionType, GrossPremiumAmount, GrossPremiumCurrency, CommissionRate, CommissionAmount, CommissionCurrency,
                                     IsReconciled, ReconciledAt, DisputeReason, CreatedAt, UpdatedAt)
    VALUES (@LineItem1aId, @Statement1Id, @Policy1Id, 'POL-2024-0001', 'Acme Corporation', 'GeneralLiability', '2024-06-15',
            'NewBusiness', 12500.00, 'USD', 15.0000, 1875.00, 'USD',
            0, NULL, NULL, @Now, @Now);

    -- Line Item 1b: Acme Corp WC - Renewal
    SET @LineItem1bId = NEWID();
    INSERT INTO CommissionLineItems (Id, StatementId, PolicyId, PolicyNumber, InsuredName, LineOfBusiness, EffectiveDate,
                                     TransactionType, GrossPremiumAmount, GrossPremiumCurrency, CommissionRate, CommissionAmount, CommissionCurrency,
                                     IsReconciled, ReconciledAt, DisputeReason, CreatedAt, UpdatedAt)
    VALUES (@LineItem1bId, @Statement1Id, @Policy2Id, 'POL-2024-0002', 'Acme Corporation', 'WorkersCompensation', '2024-09-15',
            'Renewal', 45000.00, 'USD', 12.0000, 5400.00, 'USD',
            0, NULL, NULL, @Now, @Now);

    -- Line Item 1c: Endorsement on existing policy
    SET @LineItem1cId = NEWID();
    INSERT INTO CommissionLineItems (Id, StatementId, PolicyId, PolicyNumber, InsuredName, LineOfBusiness, EffectiveDate,
                                     TransactionType, GrossPremiumAmount, GrossPremiumCurrency, CommissionRate, CommissionAmount, CommissionCurrency,
                                     IsReconciled, ReconciledAt, DisputeReason, CreatedAt, UpdatedAt)
    VALUES (@LineItem1cId, @Statement1Id, NULL, 'POL-2024-EXT-0055', 'Green Construction LLC', 'GeneralLiability', '2025-11-01',
            'Endorsement', 600.00, 'USD', 15.0000, 600.00, 'USD',
            0, NULL, NULL, @Now, @Now);

    -- Producer Split: Agent gets 60% on Acme GL
    INSERT INTO ProducerSplits (Id, StatementId, LineItemId, ProducerName, ProducerId, SplitPercentage, SplitAmount, SplitCurrency, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @Statement1Id, @LineItem1aId, 'John Agent', @AgentUserId, 60.0000, 1125.00, 'USD', @Now, @Now);

    -- Producer Split: Admin gets 40% on Acme GL
    INSERT INTO ProducerSplits (Id, StatementId, LineItemId, ProducerName, ProducerId, SplitPercentage, SplitAmount, SplitCurrency, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @Statement1Id, @LineItem1aId, 'Admin User', @AdminUserId, 40.0000, 750.00, 'USD', @Now, @Now);

    PRINT 'Created Commission Statement 1: State Farm - January 2026 (Received)';
END

-- Statement 2: Travelers - December 2025 (Reconciling)
IF NOT EXISTS (SELECT 1 FROM CommissionStatements WHERE TenantId = @TenantId AND StatementNumber = 'STMT-TV-202512')
BEGIN
    SET @Statement2Id = NEWID();
    INSERT INTO CommissionStatements (Id, TenantId, CarrierId, CarrierName, StatementNumber, PeriodMonth, PeriodYear, StatementDate,
                                      Status, TotalPremiumAmount, TotalPremiumCurrency, TotalCommissionAmount, TotalCommissionCurrency,
                                      ReceivedAt, CreatedAt, UpdatedAt)
    VALUES (@Statement2Id, @TenantId, @Carrier3Id, 'Travelers', 'STMT-TV-202512', 12, 2025, '2026-01-15',
            'Reconciling', 8500.00, 'USD', 850.00, 'USD',
            DATEADD(DAY, -15, @Now), @Now, @Now);

    -- Line Item 2a: TechStart Cyber - New Business (reconciled)
    SET @LineItem2aId = NEWID();
    INSERT INTO CommissionLineItems (Id, StatementId, PolicyId, PolicyNumber, InsuredName, LineOfBusiness, EffectiveDate,
                                     TransactionType, GrossPremiumAmount, GrossPremiumCurrency, CommissionRate, CommissionAmount, CommissionCurrency,
                                     IsReconciled, ReconciledAt, DisputeReason, CreatedAt, UpdatedAt)
    VALUES (@LineItem2aId, @Statement2Id, @Policy3Id, 'POL-2024-0003', 'TechStart Inc', 'CyberLiability', '2025-02-15',
            'NewBusiness', 8500.00, 'USD', 10.0000, 850.00, 'USD',
            1, DATEADD(DAY, -5, @Now), NULL, @Now, @Now);

    -- Line Item 2b: Endorsement - pending reconciliation
    SET @LineItem2bId = NEWID();
    INSERT INTO CommissionLineItems (Id, StatementId, PolicyId, PolicyNumber, InsuredName, LineOfBusiness, EffectiveDate,
                                     TransactionType, GrossPremiumAmount, GrossPremiumCurrency, CommissionRate, CommissionAmount, CommissionCurrency,
                                     IsReconciled, ReconciledAt, DisputeReason, CreatedAt, UpdatedAt)
    VALUES (@LineItem2bId, @Statement2Id, NULL, 'POL-2024-0003-END1', 'TechStart Inc', 'CyberLiability', '2025-08-01',
            'Endorsement', 0.00, 'USD', 10.0000, 0.00, 'USD',
            0, NULL, NULL, @Now, @Now);

    -- Producer Split: Agent gets 100% on TechStart Cyber
    INSERT INTO ProducerSplits (Id, StatementId, LineItemId, ProducerName, ProducerId, SplitPercentage, SplitAmount, SplitCurrency, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @Statement2Id, @LineItem2aId, 'John Agent', @AgentUserId, 100.0000, 850.00, 'USD', @Now, @Now);

    PRINT 'Created Commission Statement 2: Travelers - December 2025 (Reconciling)';
END

-- =====================================================
-- DOCUMENTS & TEMPLATES
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM DocumentTemplates WHERE TenantId = @TenantId AND Name = 'Standard COI Template')
BEGIN
    DECLARE @COITemplateId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO DocumentTemplates (Id, TenantId, Name, Description, TemplateType, Content, IsActive, Version, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @COITemplateId,
        @TenantId,
        'Standard COI Template',
        'Standard Certificate of Insurance template with policy details',
        'CertificateOfInsurance',
        N'<!DOCTYPE html><html><head><meta charset="UTF-8"><title>Certificate of Liability Insurance</title><style>*{box-sizing:border-box}body{font-family:Arial,sans-serif;font-size:10pt;margin:40px;color:#333}h1{font-size:16pt;font-weight:bold;color:#000;margin:0 0 4px 0}.subtitle{font-size:8pt;color:#666;margin-bottom:16px}.section-header{background:#e8e8e8;padding:6px 8px;font-weight:bold;font-size:10pt;margin:12px 0 4px 0}table{width:100%;border-collapse:collapse}td{padding:6px 8px;font-size:10pt;vertical-align:top}td.label{font-weight:bold;width:20%}td.value{width:30%}ul{margin:4px 0;padding-left:20px}li{font-size:10pt;margin:2px 0}.disclaimer{font-size:8pt;color:#666;font-style:italic;margin-top:16px;border-top:1px solid #ccc;padding-top:8px}.footer{text-align:center;font-size:8pt;color:#999;margin-top:16px}</style></head><body><h1>CERTIFICATE OF LIABILITY INSURANCE</h1><p class="subtitle">THIS CERTIFICATE IS ISSUED AS A MATTER OF INFORMATION ONLY AND CONFERS NO RIGHTS UPON THE CERTIFICATE HOLDER.</p><div class="section-header">POLICY INFORMATION</div><table><tr><td class="label">Policy Number:</td><td class="value">{{PolicyNumber}}</td><td class="label">Carrier:</td><td class="value">{{CarrierName}}</td></tr><tr><td class="label">Insured:</td><td class="value">{{ClientName}}</td><td class="label">Line of Business:</td><td class="value">{{LineOfBusiness}}</td></tr><tr><td class="label">Effective Date:</td><td class="value">{{EffectiveDate}}</td><td class="label">Expiration Date:</td><td class="value">{{ExpirationDate}}</td></tr></table><div class="section-header">COVERAGE SUMMARY</div><ul>{{#each CoverageSummary}}<li>{{this}}</li>{{/each}}</ul><div class="section-header">BROKER INFORMATION</div><table><tr><td class="label">Broker:</td><td class="value">{{BrokerName}}</td><td class="label">Issue Date:</td><td class="value">{{IssuedDate}}</td></tr></table><p class="disclaimer">This certificate is issued as a matter of information only and confers no rights upon the certificate holder. This certificate does not amend, extend or alter the coverage afforded by the policies listed herein.</p><div class="footer">CERTIFICATE OF INSURANCE -- FOR INFORMATION ONLY</div></body></html>',
        1,
        1,
        'admin@test.com',
        @Now,
        @Now
    );
    PRINT 'Created COI Template: Standard COI Template';

    -- Sample documents (metadata only; actual blobs would be in Azurite)
    INSERT INTO Documents (Id, TenantId, EntityType, EntityId, FileName, ContentType, FileSizeBytes, BlobKey, Category, Version, IsArchived, UploadedBy, UploadedAt, Description, CreatedAt, UpdatedAt)
    VALUES
    (
        NEWID(),
        @TenantId,
        'Policy',
        NULL,
        'policy-document.pdf',
        'application/pdf',
        204800,
        'policy-document-2024.pdf',
        'Policy',
        1,
        0,
        'admin@test.com',
        @Now,
        'Sample policy document',
        @Now,
        @Now
    ),
    (
        NEWID(),
        @TenantId,
        'Claim',
        NULL,
        'claim-report.pdf',
        'application/pdf',
        102400,
        'claim-report-2024.pdf',
        'ClaimReport',
        1,
        0,
        'agent@test.com',
        @Now,
        'Sample claim report',
        @Now,
        @Now
    );
    PRINT 'Created 2 sample Documents';
END

-- =====================================================
-- SUMMARY
-- =====================================================
PRINT '';
PRINT '========================================';
PRINT 'SEED DATA SUMMARY';
PRINT '========================================';
PRINT 'Tenant ID: ' + CAST(@TenantId AS VARCHAR(50));
PRINT '';
PRINT 'Users:';
PRINT '  - admin@test.com / Admin123!';
PRINT '  - agent@test.com / Agent123!';
PRINT '  - user@test.com  / User123!';
PRINT '';
PRINT 'Permissions: 24 (across 7 modules)';
PRINT 'Roles: 3 system (Admin, Agent, User)';
PRINT 'User-Role: Admin=Admin, Agent=Agent, User=User';
PRINT 'Carriers: 4 (3 Active, 1 Inactive)';
PRINT 'Tenant-Carrier Links: 3 (State Farm, Liberty Mutual, Travelers)';
PRINT 'Clients: 5 (4 Active, 1 Inactive)';
PRINT 'Communications: 3 (for Acme Corporation)';
PRINT 'Policies: 5 (2 Active, 1 Expiring, 1 Draft, 1 Cancelled)';
PRINT 'Quotes: 3 (1 Quoted, 1 Draft, 1 Submitted)';
PRINT 'Quote Carriers: 6 (2 Quoted, 2 Pending, 1 Pending, 1 Declined)';
PRINT 'Claims: 3 (1 FNOL, 1 Under Investigation, 1 Approved)';
PRINT 'Claim Notes: 3';
PRINT 'Claim Reserves: 3';
PRINT 'Claim Payments: 1 (Authorized)';
PRINT 'Commission Schedules: 3 (State Farm GL, Liberty Mutual CP, Travelers Cyber)';
PRINT 'Commission Statements: 2 (1 Received, 1 Reconciling)';
PRINT 'Commission Line Items: 5';
PRINT 'Producer Splits: 3';
PRINT 'Document Templates: 1 (Standard COI)';
PRINT 'Documents: 2 (Policy, ClaimReport)';
PRINT '========================================';

GO
