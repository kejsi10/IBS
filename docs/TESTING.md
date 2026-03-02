# IBS - Testing Guide

This guide covers all testing aspects of the Insurance Broker System.

## Test Data Overview

After seeding the database, the following test data is available:

### Users

| Email | Password | Role | Description |
|-------|----------|------|-------------|
| admin@test.com | Admin123! | Admin | Full system access |
| agent@test.com | Agent123! | Agent | Agent-level access |
| user@test.com | User123! | User | Basic user access |

**Tenant ID**: `00000000-0000-0000-0000-000000000001`

### Carriers

| Name | Code | Status | Products |
|------|------|--------|----------|
| State Farm | STATEFARM | Active | GL, Commercial Auto, Workers Comp |
| Liberty Mutual | LIBERTY | Active | Property, GL, Umbrella |
| Travelers | TRAVELERS | Active | Cyber, D&O, EPL |
| The Hartford | HARTFORD | Inactive | (none) |

### Clients

| Name | Type | Status | Notes |
|------|------|--------|-------|
| Acme Corporation | Business | Active | 2 contacts, 2 addresses |
| TechStart Inc | Business | Active | Tech startup |
| Green Construction LLC | Business | Active | Construction company |
| John Smith | Individual | Active | Personal client |
| Maria Garcia | Individual | Inactive | For testing inactive state |

### Policies

| Policy Number | Client | Status | Notes |
|---------------|--------|--------|-------|
| POL-2024-0001 | Acme Corp | Active | General Liability |
| POL-2024-0002 | Acme Corp | Active | Workers Comp |
| POL-2024-0003 | TechStart | Active | Expires in 15 days |
| POL-2024-0004 | Green Construction | Draft | Pending bind |
| POL-2023-0099 | TechStart | Cancelled | Historical policy |

### Document Templates

| Name | Type | Status | Notes |
|------|------|--------|-------|
| Standard COI Template | CertificateOfInsurance | Active | Handlebars template with policy placeholders |

### Documents

| File Name | Category | Entity | Notes |
|-----------|----------|--------|-------|
| policy-acme-001.pdf | Policy | POL-2024-0001 | Sample policy document |
| claim-acme-2024.pdf | ClaimReport | Acme Corp claim | Sample claim document |

## Manual Testing Workflows

### 1. Authentication Flow

1. Navigate to http://localhost:5173
2. Log in with `admin@test.com` / `Admin123!`
3. Verify dashboard loads with:
   - Stats cards showing client/policy counts
   - Alerts panel with expiring policies
   - Activity feed
   - Upcoming renewals list

### 2. Client Management

**Create Individual Client:**
1. Go to Clients page
2. Click "New Client"
3. Select "Individual"
4. Fill in: First Name, Last Name, Email, Phone
5. Click "Create Client"
6. Verify redirect to client detail page

**Create Business Client:**
1. Go to Clients page
2. Click "New Client"
3. Select "Business"
4. Fill in: Legal Name, Email, Phone
5. Optionally add DBA, Tax ID
6. Click "Create Client"

**Add Contact to Client:**
1. Go to any client detail page
2. Click "Contacts" tab
3. Click "Add Contact"
4. Fill in contact details
5. Toggle "Primary" if needed
6. Save

**Add Address to Client:**
1. Go to any client detail page
2. Click "Addresses" tab
3. Click "Add Address"
4. Select address type
5. Fill in address fields
6. Save

### 3. Carrier Management

**View Carriers:**
1. Go to Carriers page
2. Use status filters (All, Active, Inactive, Suspended)
3. Search by name or code
4. Click row to view details

**Edit Carrier:**
1. Go to carrier detail page
2. Click any editable field
3. Make changes
4. Press Enter or click away to save

**Manage Products:**
1. Go to carrier detail page
2. Scroll to Products section
3. Click "Add Product"
4. Fill in product details
5. Save

### 4. Policy Management

**Create New Policy (Wizard):**
1. Go to Policies page
2. Click "New Policy"
3. **Step 1 - Client**: Search and select client
4. **Step 2 - Carrier**: Select carrier, then product
5. **Step 3 - Details**: Set dates, billing, payment plan
6. **Step 4 - Coverages**: Add coverages with limits/premiums
7. **Step 5 - Review**: Verify all details, click "Create"

**Bind a Draft Policy:**
1. Go to Policies page
2. Filter by "Draft" status
3. Click on POL-2024-0004
4. Click "Bind" button
5. Confirm the action

**Activate a Bound Policy:**
1. Go to a Bound policy
2. Click "Activate" button
3. Confirm the action

**Renew an Active Policy:**
1. Go to an Active policy
2. Click "Renew" button
3. Follow renewal wizard

### 5. Dashboard Features

**Test Expiring Policies Alert:**
- POL-2024-0003 should appear in alerts (expiring in 15 days)

**Test Quick Actions:**
- Click "New Client" button
- Click "New Policy" button
- Verify navigation works

**Test Global Search (Cmd+K):**
1. Press Cmd+K (Mac) or Ctrl+K (Windows)
2. Type a search term
3. Verify results from clients, carriers, policies
4. Click a result to navigate

### 6. Filter and Search Testing

**Client Filters:**
- Click "All", "Active", "Inactive" badges
- Verify counts update correctly
- Use search to find "Acme"

**Policy Filters:**
- Test status filters (All, Draft, Bound, Active, etc.)
- Use date range filters
- Combine search with filters

## API Testing with HTTP Files

The `tests/http/` folder contains HTTP request files for testing the API directly.

### Prerequisites

- VS Code with REST Client extension, or
- JetBrains Rider/IntelliJ with HTTP Client

### Running API Tests

1. Start the backend: `dotnet run --project src/IBS.Api`
2. Open any `.http` file in `tests/http/`
3. Execute requests individually

### auth.http - Authentication

```http
### Login
POST {{baseUrl}}/auth/login
Content-Type: application/json
X-Tenant-Id: 00000000-0000-0000-0000-000000000001

{
  "email": "admin@test.com",
  "password": "Admin123!"
}
```

### clients.http - Client CRUD

```http
### List Clients
GET {{baseUrl}}/clients
Authorization: Bearer {{token}}

### Create Client
POST {{baseUrl}}/clients
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "clientType": "Business",
  "legalName": "Test Company",
  "email": "test@company.com"
}
```

### carriers.http - Carrier Management

```http
### List Carriers
GET {{baseUrl}}/carriers
Authorization: Bearer {{token}}

### Get Carrier with Products
GET {{baseUrl}}/carriers/{{carrierId}}
Authorization: Bearer {{token}}
```

### policies.http - Policy Operations

```http
### List Policies
GET {{baseUrl}}/policies
Authorization: Bearer {{token}}

### Create Policy
POST {{baseUrl}}/policies
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "clientId": "{{clientId}}",
  "carrierId": "{{carrierId}}",
  "lineOfBusiness": "GeneralLiability",
  ...
}
```

### quotes.http - Quote Management

See `tests/http/quotes.http` for full quote workflow (create, submit to carriers, record responses, accept quote).

### claims.http - Claims Management

See `tests/http/claims.http` for FNOL intake, status transitions, reserve management, and payment workflows.

### commissions.http - Commission Management

See `tests/http/commissions.http` for schedule management, statement reconciliation, and reports.

### documents.http - Document Management

See `tests/http/documents.http` for document upload, download, template management, and COI generation.

## Automated Tests

### Unit Tests

```bash
# Run all unit tests
cd tests/IBS.UnitTests
dotnet test

# Run with verbose output
dotnet test -v n

# Run specific test class
dotnet test --filter "FullyQualifiedName~ClientTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Integration Tests

```bash
# Run integration tests
cd tests/IBS.IntegrationTests
dotnet test

# Note: Integration tests use Testcontainers — SQL Server spins up automatically via Docker.
# No external database setup required; Docker Desktop must be running.
```

### Frontend Tests

```bash
cd src/IBS.Web

# Run tests in watch mode
npm test

# Run once
npm run test:ci

# Run with UI
npm run test:ui

# Run with coverage
npm run test:coverage
```

## Test Scenarios Checklist

### Authentication
- [ ] Login with valid credentials
- [ ] Login with invalid password (should fail)
- [ ] Login with non-existent email (should fail)
- [ ] Token refresh
- [ ] Logout
- [ ] Access protected route without token (should redirect)

### Clients
- [ ] List clients with pagination
- [ ] Filter by status
- [ ] Search by name/email
- [ ] Create individual client
- [ ] Create business client
- [ ] Edit client details
- [ ] Add/edit/remove contacts
- [ ] Add/edit/remove addresses
- [ ] Deactivate client
- [ ] View client's policies

### Carriers
- [ ] List carriers
- [ ] Filter by status
- [ ] Search by name/code
- [ ] View carrier details
- [ ] Edit carrier info (inline)
- [ ] Add product
- [ ] Deactivate product
- [ ] Add appetite
- [ ] Remove appetite

### Policies
- [ ] List policies
- [ ] Filter by status
- [ ] Advanced date filters
- [ ] Create policy (wizard)
- [ ] Bind draft policy
- [ ] Activate bound policy
- [ ] Renew active policy
- [ ] Cancel policy
- [ ] Add coverage
- [ ] Edit coverage
- [ ] Remove coverage
- [ ] Add endorsement
- [ ] Approve endorsement
- [ ] Issue endorsement

### Dashboard
- [ ] Stats load correctly
- [ ] Expiring policies show in alerts
- [ ] Recent activity displays
- [ ] Upcoming renewals list
- [ ] Quick action buttons work
- [ ] Global search (Cmd+K)

### Documents
- [ ] Upload document (PDF, with category and optional entity link)
- [ ] List documents with category filter
- [ ] Search documents by filename
- [ ] Download document (redirects to blob SAS URL)
- [ ] Archive document
- [ ] Archived documents excluded from list by default
- [ ] Create Handlebars template
- [ ] Edit template (update name, description, content)
- [ ] Activate / deactivate template
- [ ] Generate COI from active COI template using a Policy ID
- [ ] Generated COI PDF opens for download automatically

## Performance Testing

### Load Testing with k6

```javascript
// k6-load-test.js
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  vus: 10,
  duration: '30s',
};

export default function() {
  let res = http.get('https://localhost:7001/api/v1/clients', {
    headers: { 'Authorization': 'Bearer ' + __ENV.TOKEN }
  });
  check(res, { 'status is 200': (r) => r.status === 200 });
}
```

Run with:
```bash
k6 run --env TOKEN=your-jwt-token k6-load-test.js
```

## Debugging Tips

### Backend Logging

Check Serilog output in console for:
- Request/response details
- MediatR pipeline execution
- EF Core SQL queries
- Authentication events

### Frontend DevTools

1. Open browser DevTools (F12)
2. Network tab: Check API calls
3. Console: Check for errors
4. React DevTools: Inspect component state
5. TanStack Query DevTools: Inspect cache

### Database Inspection

```sql
-- Check recent clients
SELECT TOP 10 * FROM Clients ORDER BY CreatedAt DESC;

-- Check policies with status
SELECT PolicyNumber, Status, EffectiveDate, ExpirationDate
FROM Policies
ORDER BY CreatedAt DESC;

-- Check audit trail
SELECT * FROM AuditLogs ORDER BY Timestamp DESC;
```
