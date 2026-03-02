import { http, HttpResponse } from 'msw';

// Use a relative path pattern so handlers match any origin.
// Axios sends requests with a relative baseURL (/api/v1) in the test environment
// because VITE_API_URL is not set. MSW v2 relative URL patterns match any origin.
const API_URL = '/api/v1';

// Mock data
const mockUser = {
  id: '123',
  tenantId: 'tenant-1',
  email: 'test@test.com',
  firstName: 'Test',
  lastName: 'User',
  fullName: 'Test User',
  isActive: true,
  roles: ['Admin'],
};

const mockCarriers = [
  {
    id: 'carrier-1',
    code: 'ACME',
    name: 'Acme Insurance',
    status: 'Active',
    amBestRating: 'A+',
  },
  {
    id: 'carrier-2',
    code: 'GLOB',
    name: 'Global Underwriters',
    status: 'Active',
    amBestRating: 'A',
  },
];

const mockClients = [
  {
    id: 'client-1',
    displayName: 'John Doe',
    type: 'Individual',
    email: 'john@example.com',
    phone: '555-1234',
    status: 'Active',
    createdAt: '2024-01-15T10:00:00Z',
  },
  {
    id: 'client-2',
    displayName: 'Acme Corp',
    type: 'Business',
    email: 'contact@acme.com',
    phone: '555-5678',
    status: 'Active',
    createdAt: '2024-01-20T10:00:00Z',
  },
];

const mockUsers = [
  {
    id: 'user-1',
    tenantId: 'tenant-1',
    email: 'admin@test.com',
    firstName: 'Admin',
    lastName: 'User',
    fullName: 'Admin User',
    isActive: true,
    lastLoginAt: '2024-02-01T10:00:00Z',
    roles: ['Admin'],
    title: 'System Administrator',
    phoneNumber: '555-0001',
    createdAt: '2024-01-01T10:00:00Z',
  },
  {
    id: 'user-2',
    tenantId: 'tenant-1',
    email: 'agent@test.com',
    firstName: 'Agent',
    lastName: 'Smith',
    fullName: 'Agent Smith',
    isActive: true,
    lastLoginAt: '2024-02-10T08:30:00Z',
    roles: ['Agent'],
    title: 'Insurance Agent',
    phoneNumber: '555-0002',
    createdAt: '2024-01-05T10:00:00Z',
  },
  {
    id: 'user-3',
    tenantId: 'tenant-1',
    email: 'user@test.com',
    firstName: 'Regular',
    lastName: 'User',
    fullName: 'Regular User',
    isActive: false,
    roles: ['User'],
    createdAt: '2024-01-10T10:00:00Z',
  },
];

const mockRoles = [
  { id: 'role-1', name: 'Admin', description: 'Full system access', isSystemRole: true, userCount: 1 },
  { id: 'role-2', name: 'Agent', description: 'Insurance agent access', isSystemRole: true, userCount: 1 },
  { id: 'role-3', name: 'User', description: 'Basic user access', isSystemRole: true, userCount: 1 },
  { id: 'role-4', name: 'Claims Manager', description: 'Manages claims workflows', isSystemRole: false, userCount: 0 },
];

const mockPermissions = [
  { id: 'perm-1', name: 'View Clients', description: 'View client records', module: 'Clients' },
  { id: 'perm-2', name: 'Edit Clients', description: 'Create and edit clients', module: 'Clients' },
  { id: 'perm-3', name: 'View Policies', description: 'View policy records', module: 'Policies' },
  { id: 'perm-4', name: 'Edit Policies', description: 'Create and edit policies', module: 'Policies' },
  { id: 'perm-5', name: 'View Reports', description: 'Access reports', module: 'Reports' },
  { id: 'perm-6', name: 'Manage Users', description: 'Manage user accounts', module: 'Administration' },
];

const mockQuotes = [
  {
    id: 'quote-1',
    clientId: 'client-1',
    clientName: 'John Doe',
    lineOfBusiness: 'PersonalAuto',
    effectiveDate: '2024-06-01',
    expirationDate: '2025-06-01',
    status: 'Quoted',
    expiresAt: '2024-07-01',
    carrierCount: 2,
    responseCount: 2,
    lowestPremium: 1200,
    createdAt: '2024-05-01T10:00:00Z',
  },
  {
    id: 'quote-2',
    clientId: 'client-2',
    clientName: 'Acme Corp',
    lineOfBusiness: 'GeneralLiability',
    effectiveDate: '2024-07-01',
    expirationDate: '2025-07-01',
    status: 'Draft',
    expiresAt: '2024-08-01',
    carrierCount: 1,
    responseCount: 0,
    lowestPremium: null,
    createdAt: '2024-06-01T10:00:00Z',
  },
];

const mockQuoteDetail = {
  id: 'quote-1',
  clientId: 'client-1',
  clientName: 'John Doe',
  lineOfBusiness: 'PersonalAuto',
  effectiveDate: '2024-06-01',
  expirationDate: '2025-06-01',
  status: 'Quoted',
  expiresAt: '2024-07-01',
  acceptedCarrierId: null,
  policyId: null,
  notes: 'Looking for best rate on personal auto.',
  createdBy: 'agent@test.com',
  createdAt: '2024-05-01T10:00:00Z',
  updatedAt: '2024-05-15T10:00:00Z',
  carriers: [
    {
      id: 'qc-1',
      carrierId: 'carrier-1',
      carrierName: 'Acme Insurance',
      status: 'Quoted',
      premiumAmount: 1200,
      premiumCurrency: 'USD',
      declinationReason: null,
      conditions: 'Clean driving record required',
      proposedCoverages: null,
      respondedAt: '2024-05-10T10:00:00Z',
      expiresAt: '2024-06-15',
    },
    {
      id: 'qc-2',
      carrierId: 'carrier-2',
      carrierName: 'Global Underwriters',
      status: 'Declined',
      premiumAmount: null,
      premiumCurrency: null,
      declinationReason: 'Outside of appetite',
      conditions: null,
      proposedCoverages: null,
      respondedAt: '2024-05-12T10:00:00Z',
      expiresAt: null,
    },
  ],
};

const mockPolicies = [
  {
    id: 'policy-1',
    policyNumber: 'POL-2024-001',
    clientId: 'client-1',
    clientName: 'John Doe',
    carrierId: 'carrier-1',
    carrierName: 'Acme Insurance',
    lineOfBusiness: 'PersonalAuto',
    policyType: 'Auto Insurance',
    status: 'Active',
    effectiveDate: '2024-01-01',
    expirationDate: '2025-01-01',
    totalPremium: 1500,
    currency: 'USD',
    createdAt: '2024-01-01T10:00:00Z',
  },
  {
    id: 'policy-2',
    policyNumber: 'POL-2024-002',
    clientId: 'client-2',
    clientName: 'Acme Corp',
    carrierId: 'carrier-2',
    carrierName: 'Global Underwriters',
    lineOfBusiness: 'GeneralLiability',
    policyType: 'Commercial General Liability',
    status: 'Active',
    effectiveDate: '2024-02-01',
    expirationDate: '2025-02-01',
    totalPremium: 5000,
    currency: 'USD',
    createdAt: '2024-02-01T10:00:00Z',
  },
];

const mockClaims = [
  {
    id: 'claim-1',
    claimNumber: 'CLM-20240315-0001',
    policyId: 'policy-1',
    clientId: 'client-1',
    clientName: 'John Doe',
    policyNumber: 'POL-2024-001',
    status: 'UnderInvestigation',
    lossDate: '2024-03-15T10:30:00Z',
    reportedDate: '2024-03-16T09:00:00Z',
    lossType: 'PropertyDamage',
    lossDescription: 'Water damage from burst pipe in main office area.',
    lossAmount: 50000,
    lossCurrency: 'USD',
    claimAmount: null,
    assignedAdjusterId: 'ADJ-001',
    createdAt: '2024-03-16T09:00:00Z',
  },
  {
    id: 'claim-2',
    claimNumber: 'CLM-20240320-0002',
    policyId: 'policy-2',
    clientId: 'client-2',
    clientName: 'Acme Corp',
    policyNumber: 'POL-2024-002',
    status: 'Approved',
    lossDate: '2024-03-20T14:00:00Z',
    reportedDate: '2024-03-20T16:00:00Z',
    lossType: 'Auto',
    lossDescription: 'Company vehicle rear-ended at intersection.',
    lossAmount: 15000,
    lossCurrency: 'USD',
    claimAmount: 12000,
    claimAmountCurrency: 'USD',
    assignedAdjusterId: 'ADJ-002',
    createdAt: '2024-03-20T16:00:00Z',
  },
  {
    id: 'claim-3',
    claimNumber: 'CLM-20240401-0003',
    policyId: 'policy-1',
    clientId: 'client-1',
    clientName: 'John Doe',
    policyNumber: 'POL-2024-001',
    status: 'FNOL',
    lossDate: '2024-04-01T08:00:00Z',
    reportedDate: '2024-04-01T10:00:00Z',
    lossType: 'TheftFraud',
    lossDescription: 'Equipment stolen from warehouse overnight.',
    lossAmount: 25000,
    lossCurrency: 'USD',
    claimAmount: null,
    assignedAdjusterId: null,
    createdAt: '2024-04-01T10:00:00Z',
  },
];

const mockClaimDetail = {
  id: 'claim-1',
  claimNumber: 'CLM-20240315-0001',
  policyId: 'policy-1',
  clientId: 'client-1',
  clientName: 'John Doe',
  policyNumber: 'POL-2024-001',
  status: 'UnderInvestigation',
  lossDate: '2024-03-15T10:30:00Z',
  reportedDate: '2024-03-16T09:00:00Z',
  lossType: 'PropertyDamage',
  lossDescription: 'Water damage from burst pipe in main office area. Affecting carpet, drywall, and several workstations.',
  lossAmount: 50000,
  lossCurrency: 'USD',
  claimAmount: null,
  claimAmountCurrency: null,
  assignedAdjusterId: 'ADJ-001',
  denialReason: null,
  closedAt: null,
  closureReason: null,
  createdBy: 'agent@test.com',
  createdAt: '2024-03-16T09:00:00Z',
  updatedAt: '2024-03-20T10:00:00Z',
  notes: [
    {
      id: 'note-1',
      content: 'Initial inspection completed. Damage consistent with water intrusion from burst pipe.',
      authorBy: 'ADJ-001',
      isInternal: false,
      createdAt: '2024-03-18T10:00:00Z',
    },
    {
      id: 'note-2',
      content: 'Client may have had pre-existing water damage issue. Need to verify policy terms.',
      authorBy: 'ADJ-001',
      isInternal: true,
      createdAt: '2024-03-19T14:00:00Z',
    },
  ],
  reserves: [
    {
      id: 'reserve-1',
      reserveType: 'Indemnity',
      amount: 50000,
      currency: 'USD',
      setBy: 'ADJ-001',
      setAt: '2024-03-18T10:00:00Z',
      notes: 'Initial reserve based on preliminary damage assessment.',
    },
  ],
  payments: [
    {
      id: 'payment-1',
      paymentType: 'Expense',
      amount: 5000,
      currency: 'USD',
      payeeName: 'Emergency Cleanup Services',
      paymentDate: '2024-03-17',
      checkNumber: 'CHK-5001',
      status: 'Issued',
      authorizedBy: 'agent@test.com',
      authorizedAt: '2024-03-17T10:00:00Z',
      issuedAt: '2024-03-17T14:00:00Z',
      voidedAt: null,
      voidReason: null,
    },
  ],
};

const mockSchedules = [
  {
    id: 'schedule-1',
    carrierId: 'carrier-1',
    carrierName: 'Acme Insurance',
    lineOfBusiness: 'Personal Auto',
    newBusinessRate: 15,
    renewalRate: 12,
    effectiveFrom: '2024-01-01',
    effectiveTo: null,
    isActive: true,
  },
  {
    id: 'schedule-2',
    carrierId: 'carrier-2',
    carrierName: 'Global Underwriters',
    lineOfBusiness: 'General Liability',
    newBusinessRate: 18,
    renewalRate: 14,
    effectiveFrom: '2024-01-01',
    effectiveTo: '2025-12-31',
    isActive: true,
  },
];

const mockStatements = [
  {
    id: 'statement-1',
    carrierId: 'carrier-1',
    carrierName: 'Acme Insurance',
    statementNumber: 'STMT-2024-001',
    periodMonth: 3,
    periodYear: 2024,
    statementDate: '2024-04-15',
    status: 'Reconciling',
    totalPremium: 125000,
    totalPremiumCurrency: 'USD',
    totalCommission: 18750,
    totalCommissionCurrency: 'USD',
    receivedAt: '2024-04-20T10:00:00Z',
    lineItemCount: 3,
    reconciledCount: 1,
    disputedCount: 0,
  },
  {
    id: 'statement-2',
    carrierId: 'carrier-2',
    carrierName: 'Global Underwriters',
    statementNumber: 'STMT-2024-002',
    periodMonth: 3,
    periodYear: 2024,
    statementDate: '2024-04-10',
    status: 'Received',
    totalPremium: 75000,
    totalPremiumCurrency: 'USD',
    totalCommission: 13500,
    totalCommissionCurrency: 'USD',
    receivedAt: '2024-04-12T10:00:00Z',
    lineItemCount: 2,
    reconciledCount: 0,
    disputedCount: 0,
  },
];

const mockStatementDetail = {
  id: 'statement-1',
  carrierId: 'carrier-1',
  carrierName: 'Acme Insurance',
  statementNumber: 'STMT-2024-001',
  periodMonth: 3,
  periodYear: 2024,
  statementDate: '2024-04-15',
  status: 'Reconciling',
  totalPremium: 125000,
  totalPremiumCurrency: 'USD',
  totalCommission: 18750,
  totalCommissionCurrency: 'USD',
  receivedAt: '2024-04-20T10:00:00Z',
  createdAt: '2024-04-20T10:00:00Z',
  updatedAt: '2024-04-22T14:00:00Z',
  lineItems: [
    {
      id: 'li-1',
      policyId: 'policy-1',
      policyNumber: 'POL-2024-001',
      insuredName: 'John Doe',
      lineOfBusiness: 'Personal Auto',
      effectiveDate: '2024-01-01',
      transactionType: 'NewBusiness',
      grossPremium: 50000,
      grossPremiumCurrency: 'USD',
      commissionRate: 15,
      commissionAmount: 7500,
      commissionAmountCurrency: 'USD',
      isReconciled: true,
      reconciledAt: '2024-04-21T10:00:00Z',
      disputeReason: null,
    },
    {
      id: 'li-2',
      policyId: 'policy-2',
      policyNumber: 'POL-2024-002',
      insuredName: 'Acme Corp',
      lineOfBusiness: 'General Liability',
      effectiveDate: '2024-02-01',
      transactionType: 'NewBusiness',
      grossPremium: 50000,
      grossPremiumCurrency: 'USD',
      commissionRate: 15,
      commissionAmount: 7500,
      commissionAmountCurrency: 'USD',
      isReconciled: false,
      reconciledAt: null,
      disputeReason: null,
    },
    {
      id: 'li-3',
      policyId: null,
      policyNumber: 'POL-2024-003',
      insuredName: 'Jane Smith',
      lineOfBusiness: 'Homeowners',
      effectiveDate: '2024-03-01',
      transactionType: 'Renewal',
      grossPremium: 25000,
      grossPremiumCurrency: 'USD',
      commissionRate: 12,
      commissionAmount: 3000,
      commissionAmountCurrency: 'USD',
      isReconciled: false,
      reconciledAt: null,
      disputeReason: null,
    },
  ],
  producerSplits: [
    {
      id: 'split-1',
      lineItemId: 'li-1',
      producerName: 'Agent Smith',
      producerId: 'user-2',
      splitPercentage: 60,
      splitAmount: 4500,
      splitAmountCurrency: 'USD',
    },
    {
      id: 'split-2',
      lineItemId: 'li-1',
      producerName: 'Admin User',
      producerId: 'user-1',
      splitPercentage: 40,
      splitAmount: 3000,
      splitAmountCurrency: 'USD',
    },
  ],
};

const mockCommissionStatistics = {
  totalStatements: 2,
  receivedCount: 1,
  reconcilingCount: 1,
  reconciledCount: 0,
  disputedCount: 0,
  paidCount: 0,
  totalPremium: 200000,
  totalCommission: 32250,
  totalPaid: 0,
};

const mockCommissionSummary = [
  {
    carrierName: 'Acme Insurance',
    periodMonth: 3,
    periodYear: 2024,
    statementCount: 1,
    totalPremium: 125000,
    totalCommission: 18750,
    totalPaid: 0,
    currency: 'USD',
  },
  {
    carrierName: 'Global Underwriters',
    periodMonth: 3,
    periodYear: 2024,
    statementCount: 1,
    totalPremium: 75000,
    totalCommission: 13500,
    totalPaid: 0,
    currency: 'USD',
  },
];

const mockProducerReport = [
  {
    producerName: 'Agent Smith',
    producerId: 'user-2',
    periodMonth: 3,
    periodYear: 2024,
    lineItemCount: 1,
    averageSplitPercentage: 60,
    totalSplitAmount: 4500,
    currency: 'USD',
  },
  {
    producerName: 'Admin User',
    producerId: 'user-1',
    periodMonth: 3,
    periodYear: 2024,
    lineItemCount: 1,
    averageSplitPercentage: 40,
    totalSplitAmount: 3000,
    currency: 'USD',
  },
];

export const handlers = [
  // Auth handlers
  http.post(`${API_URL}/auth/login`, async ({ request }) => {
    const body = (await request.json()) as { email: string; password: string };
    if (body.email === 'test@test.com' && body.password === 'password') {
      return HttpResponse.json({
        accessToken: 'mock-access-token',
        refreshToken: 'mock-refresh-token',
        expiresIn: 3600,
        userId: '123',
        email: 'test@test.com',
        fullName: 'Test User',
        roles: ['Admin'],
      });
    }
    return HttpResponse.json({ error: 'Invalid credentials' }, { status: 401 });
  }),

  http.post(`${API_URL}/auth/logout`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/auth/refresh`, async ({ request }) => {
    const body = (await request.json()) as { refreshToken: string };
    if (body.refreshToken === 'mock-refresh-token') {
      return HttpResponse.json({
        accessToken: 'new-mock-access-token',
        refreshToken: 'new-mock-refresh-token',
        expiresIn: 3600,
        userId: '123',
        email: 'test@test.com',
        fullName: 'Test User',
        roles: ['Admin'],
      });
    }
    return HttpResponse.json({ error: 'Invalid refresh token' }, { status: 401 });
  }),

  http.get(`${API_URL}/auth/me`, () => {
    return HttpResponse.json(mockUser);
  }),

  // Carrier handlers
  http.get(`${API_URL}/carriers`, () => {
    return HttpResponse.json(mockCarriers);
  }),

  http.get(`${API_URL}/carriers/:id`, ({ params }) => {
    const carrier = mockCarriers.find((c) => c.id === params.id);
    if (!carrier) {
      return HttpResponse.json({ error: 'Carrier not found' }, { status: 404 });
    }
    return HttpResponse.json({
      ...carrier,
      products: [
        {
          id: 'product-1',
          carrierId: carrier.id,
          name: 'General Liability Coverage',
          code: 'GL-001',
          lineOfBusiness: 'GeneralLiability',
          lineOfBusinessDisplayName: 'General Liability',
          isActive: true,
          minimumPremium: 5000,
        },
      ],
      appetites: [
        {
          id: 'appetite-1',
          carrierId: carrier.id,
          lineOfBusiness: 'GeneralLiability',
          lineOfBusinessDisplayName: 'General Liability',
          states: 'All',
          isActive: true,
        },
      ],
      createdAt: '2024-01-01T10:00:00Z',
    });
  }),

  http.post(`${API_URL}/carriers`, async () => {
    return HttpResponse.json({ id: 'new-carrier-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/carriers/:id`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/carriers/:id/deactivate`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Client handlers
  http.get(`${API_URL}/clients`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      items: mockClients,
      totalCount: mockClients.length,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(mockClients.length / pageSize),
    });
  }),

  http.get(`${API_URL}/clients/:id`, ({ params }) => {
    const client = mockClients.find((c) => c.id === params.id);
    if (!client) {
      return HttpResponse.json({ error: 'Client not found' }, { status: 404 });
    }
    return HttpResponse.json({
      ...client,
      contacts: [],
      addresses: [],
    });
  }),

  http.post(`${API_URL}/clients/individual`, async () => {
    return HttpResponse.json({ id: 'new-individual-client-id' }, { status: 201 });
  }),

  http.post(`${API_URL}/clients/business`, async () => {
    return HttpResponse.json({ id: 'new-business-client-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/clients/:id`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/clients/:id/deactivate`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/clients/:id/reactivate`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Policy handlers
  http.get(`${API_URL}/policies`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      policies: mockPolicies,
      totalCount: mockPolicies.length,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(mockPolicies.length / pageSize),
    });
  }),

  http.get(`${API_URL}/policies/:id`, ({ params }) => {
    const policy = mockPolicies.find((p) => p.id === params.id);
    if (!policy) {
      return HttpResponse.json({ error: 'Policy not found' }, { status: 404 });
    }
    return HttpResponse.json({
      ...policy,
      billingType: 'DirectBill',
      paymentPlan: 'Annual',
      coverages: [],
      endorsements: [],
    });
  }),

  http.post(`${API_URL}/policies`, async () => {
    return HttpResponse.json({ id: 'new-policy-id' }, { status: 201 });
  }),

  http.post(`${API_URL}/policies/:id/bind`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/policies/:id/activate`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/policies/:id/cancel`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/policies/:id/renew`, () => {
    return HttpResponse.json({ id: 'renewed-policy-id' });
  }),

  // Coverage handlers
  http.post(`${API_URL}/policies/:id/coverages`, async () => {
    return HttpResponse.json({ id: 'new-coverage-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/policies/:policyId/coverages/:coverageId`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.delete(`${API_URL}/policies/:policyId/coverages/:coverageId`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Endorsement handlers
  http.post(`${API_URL}/policies/:id/endorsements`, async () => {
    return HttpResponse.json({ id: 'new-endorsement-id' }, { status: 201 });
  }),

  http.post(`${API_URL}/policies/:policyId/endorsements/:endorsementId/approve`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/policies/:policyId/endorsements/:endorsementId/issue`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/policies/:policyId/endorsements/:endorsementId/reject`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Policy queries
  http.get(`${API_URL}/policies/expiring`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      policies: mockPolicies,
      totalCount: mockPolicies.length,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(mockPolicies.length / pageSize),
    });
  }),

  http.get(`${API_URL}/policies/due-for-renewal`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      policies: mockPolicies,
      totalCount: mockPolicies.length,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(mockPolicies.length / pageSize),
    });
  }),

  http.get(`${API_URL}/clients/:clientId/policies`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      policies: mockPolicies,
      totalCount: mockPolicies.length,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(mockPolicies.length / pageSize),
    });
  }),

  // User handlers
  http.get(`${API_URL}/users`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      items: mockUsers,
      totalCount: mockUsers.length,
      page: page,
      pageSize: pageSize,
      totalPages: Math.ceil(mockUsers.length / pageSize),
    });
  }),

  http.get(`${API_URL}/users/:id`, ({ params }) => {
    const user = mockUsers.find((u) => u.id === params.id);
    if (!user) {
      return HttpResponse.json({ error: 'User not found' }, { status: 404 });
    }
    return HttpResponse.json(user);
  }),

  http.post(`${API_URL}/auth/register`, async () => {
    return HttpResponse.json({ id: 'new-user-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/users/:id/profile`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/users/:id/activate`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/users/:id/deactivate`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/users/:userId/roles`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.delete(`${API_URL}/users/:userId/roles/:roleId`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Role handlers
  http.get(`${API_URL}/roles`, () => {
    return HttpResponse.json(mockRoles);
  }),

  http.get(`${API_URL}/roles/:id`, ({ params }) => {
    const role = mockRoles.find((r) => r.id === params.id);
    if (!role) {
      return HttpResponse.json({ error: 'Role not found' }, { status: 404 });
    }
    return HttpResponse.json({
      ...role,
      permissions: role.name === 'Admin' ? mockPermissions : mockPermissions.slice(0, 2),
      createdAt: '2024-01-01T10:00:00Z',
    });
  }),

  http.post(`${API_URL}/roles`, async () => {
    return HttpResponse.json({ id: 'new-role-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/roles/:id`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/roles/:roleId/permissions`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.delete(`${API_URL}/roles/:roleId/permissions/:permissionId`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Permissions handler
  http.get(`${API_URL}/permissions`, () => {
    return HttpResponse.json(mockPermissions);
  }),

  // Quote handlers
  http.get(`${API_URL}/quotes`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      quotes: mockQuotes,
      totalCount: mockQuotes.length,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(mockQuotes.length / pageSize),
    });
  }),

  http.get(`${API_URL}/quotes/summary`, () => {
    return HttpResponse.json({
      totalQuotes: 2,
      draftCount: 1,
      submittedCount: 0,
      quotedCount: 1,
      acceptedCount: 0,
      expiredCount: 0,
      cancelledCount: 0,
      averageQuotedPremium: 1200,
    });
  }),

  http.get(`${API_URL}/quotes/:id`, ({ params }) => {
    if (params.id === 'quote-1') {
      return HttpResponse.json(mockQuoteDetail);
    }
    return HttpResponse.json({ error: 'Quote not found' }, { status: 404 });
  }),

  http.get(`${API_URL}/clients/:clientId/quotes`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      quotes: mockQuotes,
      totalCount: mockQuotes.length,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(mockQuotes.length / pageSize),
    });
  }),

  http.post(`${API_URL}/quotes`, async () => {
    return HttpResponse.json({ id: 'new-quote-id' }, { status: 201 });
  }),

  http.post(`${API_URL}/quotes/:id/carriers`, async () => {
    return HttpResponse.json({ id: 'new-quote-carrier-id' }, { status: 201 });
  }),

  http.delete(`${API_URL}/quotes/:id/carriers/:carrierId`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/quotes/:id/submit`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/quotes/:id/carriers/:carrierId/respond`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/quotes/:id/carriers/:carrierId/accept`, () => {
    return HttpResponse.json({ policyId: 'new-policy-from-quote-id' });
  }),

  http.post(`${API_URL}/quotes/:id/cancel`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Claims handlers
  http.get(`${API_URL}/claims`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      claims: mockClaims,
      totalCount: mockClaims.length,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(mockClaims.length / pageSize),
    });
  }),

  http.get(`${API_URL}/claims/statistics`, () => {
    return HttpResponse.json({
      totalClaims: 3,
      fnolCount: 1,
      acknowledgedCount: 0,
      assignedCount: 0,
      underInvestigationCount: 1,
      evaluationCount: 0,
      approvedCount: 1,
      deniedCount: 0,
      settlementCount: 0,
      closedCount: 0,
      totalReserves: 75000,
      totalPayments: 15000,
    });
  }),

  http.get(`${API_URL}/claims/:id`, ({ params }) => {
    if (params.id === 'claim-1') {
      return HttpResponse.json(mockClaimDetail);
    }
    const claim = mockClaims.find((c) => c.id === params.id);
    if (!claim) {
      return HttpResponse.json({ error: 'Claim not found' }, { status: 404 });
    }
    return HttpResponse.json({
      ...claim,
      createdBy: 'agent@test.com',
      updatedAt: '2024-04-01T10:00:00Z',
      notes: [],
      reserves: [],
      payments: [],
    });
  }),

  http.post(`${API_URL}/claims`, async () => {
    return HttpResponse.json({ id: 'new-claim-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/claims/:id/status`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/claims/:id/notes`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/claims/:id/reserves`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/claims/:id/payments`, async () => {
    return HttpResponse.json({ id: 'new-payment-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/claims/:claimId/payments/:paymentId/issue`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.put(`${API_URL}/claims/:claimId/payments/:paymentId/void`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Commission schedule handlers
  http.get(`${API_URL}/commissions/schedules`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      schedules: mockSchedules,
      totalCount: mockSchedules.length,
      pageNumber: page,
      pageSize,
      totalPages: Math.ceil(mockSchedules.length / pageSize),
    });
  }),

  http.get(`${API_URL}/commissions/schedules/:id`, ({ params }) => {
    const schedule = mockSchedules.find((s) => s.id === params.id);
    if (!schedule) {
      return HttpResponse.json({ error: 'Schedule not found' }, { status: 404 });
    }
    return HttpResponse.json(schedule);
  }),

  http.post(`${API_URL}/commissions/schedules`, async () => {
    return HttpResponse.json({ id: 'new-schedule-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/commissions/schedules/:id`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/commissions/schedules/:id/deactivate`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Commission statement handlers
  http.get(`${API_URL}/commissions/statements`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      statements: mockStatements,
      totalCount: mockStatements.length,
      pageNumber: page,
      pageSize,
      totalPages: Math.ceil(mockStatements.length / pageSize),
    });
  }),

  http.get(`${API_URL}/commissions/statements/:id`, ({ params }) => {
    if (params.id === 'statement-1') {
      return HttpResponse.json(mockStatementDetail);
    }
    const statement = mockStatements.find((s) => s.id === params.id);
    if (!statement) {
      return HttpResponse.json({ error: 'Statement not found' }, { status: 404 });
    }
    return HttpResponse.json({
      ...statement,
      createdAt: '2024-04-12T10:00:00Z',
      updatedAt: '2024-04-12T10:00:00Z',
      lineItems: [],
      producerSplits: [],
    });
  }),

  http.post(`${API_URL}/commissions/statements`, async () => {
    return HttpResponse.json({ id: 'new-statement-id' }, { status: 201 });
  }),

  http.post(`${API_URL}/commissions/statements/:id/line-items`, async () => {
    return HttpResponse.json({ id: 'new-line-item-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/commissions/statements/:id/line-items/:lineItemId/reconcile`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/commissions/statements/:id/line-items/:lineItemId/dispute`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/commissions/statements/:id/line-items/:lineItemId/splits`, async () => {
    return HttpResponse.json({ id: 'new-split-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/commissions/statements/:id/status`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Commission statistics & reports
  http.get(`${API_URL}/commissions/statistics`, () => {
    return HttpResponse.json(mockCommissionStatistics);
  }),

  http.get(`${API_URL}/commissions/reports/summary`, () => {
    return HttpResponse.json(mockCommissionSummary);
  }),

  http.get(`${API_URL}/commissions/reports/producer`, () => {
    return HttpResponse.json(mockProducerReport);
  }),

  // ── Documents ──────────────────────────────────────────────────────────────
  http.get(`${API_URL}/documents`, () => {
    return HttpResponse.json({
      items: [
        {
          id: '00000000-0000-0000-0000-000000000010',
          entityType: 'Policy',
          entityId: '00000000-0000-0000-0000-000000000001',
          fileName: 'policy-document.pdf',
          contentType: 'application/pdf',
          fileSizeBytes: 204800,
          category: 'Policy',
          version: 1,
          isArchived: false,
          uploadedBy: 'admin',
          uploadedAt: '2026-01-15T10:00:00Z',
        },
        {
          id: '00000000-0000-0000-0000-000000000011',
          entityType: 'Claim',
          entityId: '00000000-0000-0000-0000-000000000002',
          fileName: 'claim-report.pdf',
          contentType: 'application/pdf',
          fileSizeBytes: 102400,
          category: 'ClaimReport',
          version: 1,
          isArchived: false,
          uploadedBy: 'agent',
          uploadedAt: '2026-01-20T14:30:00Z',
        },
      ],
      totalCount: 2,
      page: 1,
      pageSize: 20,
      totalPages: 1,
    });
  }),

  // ── Document Templates (must be before /documents/:id to avoid param match) ──
  http.get(`${API_URL}/documents/templates`, () => {
    return HttpResponse.json({
      items: [
        {
          id: '00000000-0000-0000-0000-000000000020',
          name: 'Standard COI Template',
          description: 'Default certificate of insurance template',
          templateType: 'CertificateOfInsurance',
          isActive: true,
          version: 1,
          createdBy: 'admin',
        },
      ],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
    });
  }),

  http.get(`${API_URL}/documents/templates/:id`, () => {
    return HttpResponse.json({
      id: '00000000-0000-0000-0000-000000000020',
      tenantId: '00000000-0000-0000-0000-000000000001',
      name: 'Standard COI Template',
      description: 'Default certificate of insurance template',
      templateType: 'CertificateOfInsurance',
      content: '<h1>COI - {{PolicyNumber}}</h1><p>Insured: {{ClientName}}</p>',
      isActive: true,
      version: 1,
      createdBy: 'admin',
    });
  }),

  http.post(`${API_URL}/documents/templates`, () => {
    return HttpResponse.json({ id: '00000000-0000-0000-0000-000000000021' }, { status: 201 });
  }),

  http.put(`${API_URL}/documents/templates/:id`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/documents/templates/:id/activate`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/documents/templates/:id/deactivate`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // ── Document by ID and other document endpoints ────────────────────────────
  http.get(`${API_URL}/documents/:id`, () => {
    return HttpResponse.json({
      id: '00000000-0000-0000-0000-000000000010',
      tenantId: '00000000-0000-0000-0000-000000000001',
      entityType: 'Policy',
      entityId: '00000000-0000-0000-0000-000000000001',
      fileName: 'policy-document.pdf',
      contentType: 'application/pdf',
      fileSizeBytes: 204800,
      blobKey: '00000000-0000-0000-0000-000000000001/Policy/uuid/policy-document.pdf',
      category: 'Policy',
      version: 1,
      isArchived: false,
      uploadedBy: 'admin',
      description: 'Main policy document',
      uploadedAt: '2026-01-15T10:00:00Z',
    });
  }),

  http.post(`${API_URL}/documents/upload`, () => {
    return HttpResponse.json({ id: '00000000-0000-0000-0000-000000000012' }, { status: 201 });
  }),

  http.delete(`${API_URL}/documents/:id`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/documents/generate-coi`, () => {
    return HttpResponse.json({ id: '00000000-0000-0000-0000-000000000013' }, { status: 201 });
  }),

  // ── Audit Log ─────────────────────────────────────────────────────────────
  http.get(`${API_URL}/audit`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      items: [
        {
          id: 'audit-1',
          tenantId: 'tenant-1',
          userId: 'user-1',
          userEmail: 'admin@test.com',
          action: 'Create',
          entityType: 'Client',
          entityId: 'client-1',
          changes: '{"displayName":"John Doe","email":"john@example.com"}',
          timestamp: '2024-03-15T10:30:00Z',
        },
        {
          id: 'audit-2',
          tenantId: 'tenant-1',
          userId: 'user-2',
          userEmail: 'agent@test.com',
          action: 'Update',
          entityType: 'Policy',
          entityId: 'policy-1',
          changes: '{"status":{"old":"Draft","new":"Active"}}',
          timestamp: '2024-03-16T09:00:00Z',
        },
        {
          id: 'audit-3',
          tenantId: 'tenant-1',
          userId: 'user-1',
          userEmail: 'admin@test.com',
          action: 'Delete',
          entityType: 'Document',
          entityId: 'doc-1',
          changes: null,
          timestamp: '2024-03-17T14:00:00Z',
        },
      ],
      totalCount: 3,
      page,
      pageSize,
      totalPages: 1,
    });
  }),

  // ── Tenants ───────────────────────────────────────────────────────────────
  http.get(`${API_URL}/tenants`, ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

    return HttpResponse.json({
      items: [
        {
          id: 'tenant-1',
          name: 'Acme Insurance Brokerage',
          subdomain: 'acme',
          status: 'Active',
          subscriptionTier: 'Professional',
          createdAt: '2024-01-01T10:00:00Z',
        },
        {
          id: 'tenant-2',
          name: 'Global Brokers Ltd',
          subdomain: 'global-brokers',
          status: 'Active',
          subscriptionTier: 'Enterprise',
          createdAt: '2024-02-15T10:00:00Z',
        },
        {
          id: 'tenant-3',
          name: 'Suspended Corp',
          subdomain: 'suspended',
          status: 'Suspended',
          subscriptionTier: 'Basic',
          createdAt: '2024-03-01T10:00:00Z',
        },
      ],
      totalCount: 3,
      page,
      pageSize,
      totalPages: 1,
    });
  }),

  http.get(`${API_URL}/tenants/:id`, ({ params }) => {
    const tenants: Record<string, object> = {
      'tenant-1': {
        id: 'tenant-1',
        name: 'Acme Insurance Brokerage',
        subdomain: 'acme',
        status: 'Active',
        subscriptionTier: 'Professional',
        defaultCurrency: 'USD',
        settings: '{}',
        createdAt: '2024-01-01T10:00:00Z',
        updatedAt: '2024-06-01T10:00:00Z',
        carriers: [
          {
            carrierId: 'carrier-1',
            carrierName: 'Acme Insurance',
            agencyCode: 'AG-001',
            commissionRate: 15,
            isActive: true,
          },
        ],
      },
    };
    const tenant = tenants[params.id as string];
    if (!tenant) {
      return HttpResponse.json({ error: 'Tenant not found' }, { status: 404 });
    }
    return HttpResponse.json(tenant);
  }),

  http.post(`${API_URL}/tenants`, async () => {
    return HttpResponse.json({ id: 'new-tenant-id' }, { status: 201 });
  }),

  http.put(`${API_URL}/tenants/:id`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/tenants/:id/suspend`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/tenants/:id/activate`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/tenants/:id/cancel`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.put(`${API_URL}/tenants/:id/subscription`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post(`${API_URL}/tenants/:id/carriers`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.delete(`${API_URL}/tenants/:id/carriers/:carrierId`, () => {
    return new HttpResponse(null, { status: 204 });
  }),
];
