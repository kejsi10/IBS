# IBS Web UI Implementation Plan

## Executive Summary

This document outlines the implementation steps for completing the IBS Web frontend, including proper testing at each layer. The frontend uses React 19, TypeScript, Vite, Zustand, TanStack Query, and Tailwind CSS.

**Current State:**
- ✅ Authentication (Login, token refresh, protected routes)
- ✅ Layout (Sidebar, Header, responsive shell)
- ✅ Dashboard (stat cards, alerts, renewals, activity feed)
- ✅ UI components (Select, Dialog, Table, Pagination, DatePicker, Badge, Skeleton, etc.)
- ✅ Testing infrastructure (Vitest, RTL, MSW)
- ✅ API type definitions (`src/types/api.ts`)
- ✅ API service modules (`src/services/`)
- ✅ React Query hooks (`src/hooks/`)
- ✅ Feature modules (Clients, Carriers, Policies, Quotes, Claims, Commissions, Reports, Settings, Documents)
- ✅ Frontend tests (hooks, pages for all feature modules)

---

## Phase 1: Testing Infrastructure Setup

### 1.1 Install Testing Dependencies

```bash
cd src/IBS.Web
npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event jsdom @vitest/coverage-v8 msw
```

**Package Purposes:**
- `vitest` - Test runner compatible with Vite
- `@testing-library/react` - React component testing utilities
- `@testing-library/jest-dom` - Custom DOM matchers
- `@testing-library/user-event` - User interaction simulation
- `jsdom` - DOM environment for Node.js
- `@vitest/coverage-v8` - Code coverage reporting
- `msw` - Mock Service Worker for API mocking

### 1.2 Configure Vitest

Create `vitest.config.ts`:

```typescript
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: ['node_modules/', 'src/test/'],
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
```

### 1.3 Create Test Setup File

Create `src/test/setup.ts`:

```typescript
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, beforeAll, afterAll } from 'vitest';
import { server } from './mocks/server';

// Cleanup after each test
afterEach(() => {
  cleanup();
});

// Start MSW server before all tests
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

### 1.4 Create MSW Mock Server

Create `src/test/mocks/handlers.ts`:

```typescript
import { http, HttpResponse } from 'msw';

const API_URL = 'https://localhost:7001/api/v1';

export const handlers = [
  // Auth handlers
  http.post(`${API_URL}/auth/login`, async ({ request }) => {
    const body = await request.json();
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

  http.get(`${API_URL}/auth/me`, () => {
    return HttpResponse.json({
      id: '123',
      tenantId: 'tenant-1',
      email: 'test@test.com',
      firstName: 'Test',
      lastName: 'User',
      roles: ['Admin'],
    });
  }),

  // Add more handlers as features are implemented
];
```

Create `src/test/mocks/server.ts`:

```typescript
import { setupServer } from 'msw/node';
import { handlers } from './handlers';

export const server = setupServer(...handlers);
```

### 1.5 Create Test Utilities

Create `src/test/utils.tsx`:

```typescript
import { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
      },
    },
  });

interface WrapperProps {
  children: React.ReactNode;
}

function AllProviders({ children }: WrapperProps) {
  const queryClient = createTestQueryClient();
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{children}</BrowserRouter>
    </QueryClientProvider>
  );
}

const customRender = (ui: ReactElement, options?: Omit<RenderOptions, 'wrapper'>) =>
  render(ui, { wrapper: AllProviders, ...options });

export * from '@testing-library/react';
export { customRender as render };
```

### 1.6 Add Test Scripts to package.json

```json
{
  "scripts": {
    "test": "vitest",
    "test:ui": "vitest --ui",
    "test:coverage": "vitest run --coverage",
    "test:ci": "vitest run"
  }
}
```

---

## Phase 2: API Types and Services

### 2.1 Define API Types

Create `src/types/api.ts`:

```typescript
// ============================================
// Common Types
// ============================================
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  error: string;
}

// ============================================
// Auth Types
// ============================================
export interface LoginRequest {
  email: string;
  password: string;
  tenantId?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  userId: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface User {
  id: string;
  tenantId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  title?: string;
  phoneNumber?: string;
  isActive: boolean;
  lastLoginAt?: string;
  roles: string[];
}

// ============================================
// Carrier Types
// ============================================
export type CarrierStatus = 'Active' | 'Inactive' | 'Suspended';

export interface CarrierSummary {
  id: string;
  code: string;
  name: string;
  status: CarrierStatus;
  amBestRating?: string;
}

export interface Carrier extends CarrierSummary {
  legalName?: string;
  naicCode?: string;
  websiteUrl?: string;
  apiEndpoint?: string;
  notes?: string;
  products: Product[];
  appetites: Appetite[];
  createdAt: string;
  updatedAt?: string;
}

export interface Product {
  id: string;
  code: string;
  name: string;
  lineOfBusiness: LineOfBusiness;
  description?: string;
  minimumPremium?: number;
  effectiveDate?: string;
  expirationDate?: string;
  isActive: boolean;
}

export interface Appetite {
  id: string;
  lineOfBusiness: LineOfBusiness;
  states: string;
  minYearsInBusiness?: number;
  maxYearsInBusiness?: number;
  minAnnualRevenue?: number;
  maxAnnualRevenue?: number;
  acceptedIndustries?: string;
  excludedIndustries?: string;
  isActive: boolean;
}

export interface CreateCarrierRequest {
  name: string;
  code: string;
  legalName?: string;
  amBestRating?: string;
  naicCode?: string;
  websiteUrl?: string;
  notes?: string;
}

export interface UpdateCarrierRequest {
  name: string;
  legalName?: string;
  amBestRating?: string;
  naicCode?: string;
  websiteUrl?: string;
  apiEndpoint?: string;
  notes?: string;
}

// ============================================
// Client Types
// ============================================
export type ClientType = 'Individual' | 'Business';
export type ClientStatus = 'Active' | 'Inactive';
export type AddressType = 'Mailing' | 'Physical' | 'Billing';

export interface ClientSummary {
  id: string;
  displayName: string;
  type: ClientType;
  email?: string;
  phone?: string;
  status: ClientStatus;
  createdAt: string;
}

export interface Client {
  id: string;
  type: ClientType;
  status: ClientStatus;
  // Individual fields
  firstName?: string;
  lastName?: string;
  middleName?: string;
  suffix?: string;
  dateOfBirth?: string;
  // Business fields
  businessName?: string;
  businessType?: string;
  dbaName?: string;
  industry?: string;
  yearEstablished?: number;
  numberOfEmployees?: number;
  annualRevenue?: number;
  website?: string;
  // Relations
  contacts: Contact[];
  addresses: Address[];
  createdAt: string;
  updatedAt?: string;
}

export interface Contact {
  id: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  title?: string;
  isPrimary: boolean;
}

export interface Address {
  id: string;
  type: AddressType;
  line1: string;
  line2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  isPrimary: boolean;
}

export interface CreateIndividualClientRequest {
  firstName: string;
  lastName: string;
  middleName?: string;
  suffix?: string;
  dateOfBirth?: string;
  email?: string;
  phone?: string;
}

export interface CreateBusinessClientRequest {
  businessName: string;
  businessType: string;
  dbaName?: string;
  industry?: string;
  yearEstablished?: number;
  numberOfEmployees?: number;
  annualRevenue?: number;
  website?: string;
  email?: string;
  phone?: string;
}

// ============================================
// Policy Types
// ============================================
export type LineOfBusiness =
  | 'PersonalAuto'
  | 'CommercialAuto'
  | 'Homeowners'
  | 'Renters'
  | 'GeneralLiability'
  | 'ProfessionalLiability'
  | 'WorkersCompensation'
  | 'CommercialProperty'
  | 'BusinessOwners'
  | 'Umbrella'
  | 'CyberLiability'
  | 'DirectorsOfficers'
  | 'EmploymentPractices'
  | 'InlandMarine'
  | 'Crime'
  | 'Surety'
  | 'Other';

export type PolicyStatus =
  | 'Draft'
  | 'Bound'
  | 'Active'
  | 'Cancelled'
  | 'Expired'
  | 'Renewed'
  | 'PendingRenewal'
  | 'NonRenewed';

export type BillingType = 'DirectBill' | 'AgencyBill';
export type PaymentPlan = 'Annual' | 'SemiAnnual' | 'Quarterly' | 'Monthly';
export type CancellationType = 'FlatCancel' | 'InsuredRequest' | 'NonPayment' | 'Underwriting' | 'CarrierRequest';
export type EndorsementStatus = 'Pending' | 'Approved' | 'Issued' | 'Rejected' | 'Cancelled';

export interface PolicySummary {
  id: string;
  policyNumber: string;
  clientId: string;
  clientName?: string;
  carrierId: string;
  carrierName?: string;
  lineOfBusiness: string;
  policyType: string;
  status: string;
  effectiveDate: string;
  expirationDate: string;
  totalPremium: number;
  currency: string;
  createdAt: string;
}

export interface Policy extends PolicySummary {
  billingType: string;
  paymentPlan: string;
  carrierPolicyNumber?: string;
  notes?: string;
  boundAt?: string;
  cancellationDate?: string;
  cancellationReason?: string;
  coverages: Coverage[];
  endorsements: Endorsement[];
  updatedAt?: string;
}

export interface Coverage {
  id: string;
  code: string;
  name: string;
  description?: string;
  limitAmount?: number;
  deductibleAmount?: number;
  premiumAmount: number;
  isOptional: boolean;
  isActive: boolean;
}

export interface Endorsement {
  id: string;
  endorsementNumber: string;
  effectiveDate: string;
  type: string;
  description: string;
  premiumChange: number;
  status: string;
  processedAt?: string;
  notes?: string;
}

export interface CreatePolicyRequest {
  clientId: string;
  carrierId: string;
  lineOfBusiness: LineOfBusiness;
  policyType: string;
  effectiveDate: string;
  expirationDate: string;
  billingType?: BillingType;
  paymentPlan?: PaymentPlan;
  quoteId?: string;
  notes?: string;
}

export interface CancelPolicyRequest {
  cancellationDate: string;
  reason: string;
  cancellationType: CancellationType;
}

export interface AddCoverageRequest {
  code: string;
  name: string;
  premiumAmount: number;
  description?: string;
  limitAmount?: number;
  deductibleAmount?: number;
  isOptional?: boolean;
}

export interface UpdateCoverageRequest {
  name: string;
  premiumAmount: number;
  description?: string;
  limitAmount?: number;
  perOccurrenceLimit?: number;
  aggregateLimit?: number;
  deductibleAmount?: number;
}

export interface AddEndorsementRequest {
  effectiveDate: string;
  type: string;
  description: string;
  premiumChange: number;
  notes?: string;
}

export interface RejectEndorsementRequest {
  reason: string;
}

// ============================================
// Query Parameters
// ============================================
export interface PaginationParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface PolicyFilters extends PaginationParams {
  search?: string;
  clientId?: string;
  carrierId?: string;
  status?: PolicyStatus;
  lineOfBusiness?: LineOfBusiness;
  effectiveDateFrom?: string;
  effectiveDateTo?: string;
  expirationDateFrom?: string;
  expirationDateTo?: string;
}

export interface ClientFilters extends PaginationParams {
  search?: string;
}
```

### 2.2 Create API Service Modules

Create `src/services/auth.service.ts`:

```typescript
import api from '@/lib/api';
import type { LoginRequest, LoginResponse, User } from '@/types/api';

export const authService = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/login', data);
    return response.data;
  },

  logout: async (): Promise<void> => {
    await api.post('/auth/logout');
  },

  refreshToken: async (refreshToken: string): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/refresh', { refreshToken });
    return response.data;
  },

  getCurrentUser: async (): Promise<User> => {
    const response = await api.get<User>('/auth/me');
    return response.data;
  },

  forgotPassword: async (email: string, tenantId?: string): Promise<void> => {
    await api.post('/auth/forgot-password', { email, tenantId });
  },

  resetPassword: async (data: {
    token: string;
    email: string;
    newPassword: string;
    confirmPassword: string;
    tenantId?: string;
  }): Promise<void> => {
    await api.post('/auth/reset-password', data);
  },
};
```

Create `src/services/carriers.service.ts`:

```typescript
import api from '@/lib/api';
import type {
  Carrier,
  CarrierSummary,
  CreateCarrierRequest,
  UpdateCarrierRequest,
  CarrierStatus,
} from '@/types/api';

export const carriersService = {
  getAll: async (): Promise<CarrierSummary[]> => {
    const response = await api.get<CarrierSummary[]>('/carriers');
    return response.data;
  },

  getById: async (id: string): Promise<Carrier> => {
    const response = await api.get<Carrier>(`/carriers/${id}`);
    return response.data;
  },

  getByStatus: async (status: CarrierStatus): Promise<CarrierSummary[]> => {
    const response = await api.get<CarrierSummary[]>(`/carriers/status/${status}`);
    return response.data;
  },

  search: async (searchTerm: string): Promise<CarrierSummary[]> => {
    const response = await api.get<CarrierSummary[]>('/carriers/search', {
      params: { searchTerm },
    });
    return response.data;
  },

  create: async (data: CreateCarrierRequest): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>('/carriers', data);
    return response.data;
  },

  update: async (id: string, data: UpdateCarrierRequest): Promise<void> => {
    await api.put(`/carriers/${id}`, data);
  },

  deactivate: async (id: string, reason?: string): Promise<void> => {
    await api.post(`/carriers/${id}/deactivate`, { reason });
  },

  addProduct: async (carrierId: string, data: any): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>(`/carriers/${carrierId}/products`, data);
    return response.data;
  },

  addAppetite: async (carrierId: string, data: any): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>(`/carriers/${carrierId}/appetites`, data);
    return response.data;
  },
};
```

Create `src/services/clients.service.ts`:

```typescript
import api from '@/lib/api';
import type {
  Client,
  ClientSummary,
  CreateIndividualClientRequest,
  CreateBusinessClientRequest,
  ClientFilters,
  PaginatedResult,
} from '@/types/api';

export const clientsService = {
  getAll: async (filters?: ClientFilters): Promise<PaginatedResult<ClientSummary>> => {
    const response = await api.get<PaginatedResult<ClientSummary>>('/clients', {
      params: filters,
    });
    return response.data;
  },

  getById: async (id: string): Promise<Client> => {
    const response = await api.get<Client>(`/clients/${id}`);
    return response.data;
  },

  createIndividual: async (data: CreateIndividualClientRequest): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>('/clients/individual', data);
    return response.data;
  },

  createBusiness: async (data: CreateBusinessClientRequest): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>('/clients/business', data);
    return response.data;
  },

  update: async (id: string, data: Partial<Client>): Promise<void> => {
    await api.put(`/clients/${id}`, data);
  },

  deactivate: async (id: string): Promise<void> => {
    await api.post(`/clients/${id}/deactivate`);
  },
};
```

Create `src/services/policies.service.ts`:

```typescript
import api from '@/lib/api';
import type {
  Policy,
  PolicySummary,
  CreatePolicyRequest,
  CancelPolicyRequest,
  AddCoverageRequest,
  UpdateCoverageRequest,
  AddEndorsementRequest,
  RejectEndorsementRequest,
  PolicyFilters,
  PaginatedResult,
} from '@/types/api';

export const policiesService = {
  // Policy CRUD
  getAll: async (filters?: PolicyFilters): Promise<PaginatedResult<PolicySummary>> => {
    const response = await api.get<PaginatedResult<PolicySummary>>('/policies', {
      params: filters,
    });
    return response.data;
  },

  getById: async (id: string): Promise<Policy> => {
    const response = await api.get<Policy>(`/policies/${id}`);
    return response.data;
  },

  create: async (data: CreatePolicyRequest): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>('/policies', data);
    return response.data;
  },

  // Policy Lifecycle
  bind: async (id: string): Promise<void> => {
    await api.post(`/policies/${id}/bind`);
  },

  activate: async (id: string): Promise<void> => {
    await api.post(`/policies/${id}/activate`);
  },

  cancel: async (id: string, data: CancelPolicyRequest): Promise<void> => {
    await api.post(`/policies/${id}/cancel`, data);
  },

  renew: async (id: string): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>(`/policies/${id}/renew`);
    return response.data;
  },

  // Coverage Management
  addCoverage: async (policyId: string, data: AddCoverageRequest): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>(`/policies/${policyId}/coverages`, data);
    return response.data;
  },

  updateCoverage: async (policyId: string, coverageId: string, data: UpdateCoverageRequest): Promise<void> => {
    await api.put(`/policies/${policyId}/coverages/${coverageId}`, data);
  },

  removeCoverage: async (policyId: string, coverageId: string): Promise<void> => {
    await api.delete(`/policies/${policyId}/coverages/${coverageId}`);
  },

  // Endorsement Management
  addEndorsement: async (policyId: string, data: AddEndorsementRequest): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>(`/policies/${policyId}/endorsements`, data);
    return response.data;
  },

  approveEndorsement: async (policyId: string, endorsementId: string): Promise<void> => {
    await api.post(`/policies/${policyId}/endorsements/${endorsementId}/approve`);
  },

  issueEndorsement: async (policyId: string, endorsementId: string): Promise<void> => {
    await api.post(`/policies/${policyId}/endorsements/${endorsementId}/issue`);
  },

  rejectEndorsement: async (policyId: string, endorsementId: string, data: RejectEndorsementRequest): Promise<void> => {
    await api.post(`/policies/${policyId}/endorsements/${endorsementId}/reject`, data);
  },

  // Queries
  getExpiring: async (startDate: string, endDate: string, page = 1, pageSize = 20): Promise<PaginatedResult<PolicySummary>> => {
    const response = await api.get<PaginatedResult<PolicySummary>>('/policies/expiring', {
      params: { startDate, endDate, page, pageSize },
    });
    return response.data;
  },

  getDueForRenewal: async (daysUntilExpiration = 60, page = 1, pageSize = 20): Promise<PaginatedResult<PolicySummary>> => {
    const response = await api.get<PaginatedResult<PolicySummary>>('/policies/due-for-renewal', {
      params: { daysUntilExpiration, page, pageSize },
    });
    return response.data;
  },

  getByClient: async (clientId: string, params?: { page?: number; pageSize?: number }): Promise<PaginatedResult<PolicySummary>> => {
    const response = await api.get<PaginatedResult<PolicySummary>>(`/clients/${clientId}/policies`, {
      params,
    });
    return response.data;
  },
};
```

Create `src/services/index.ts`:

```typescript
export { authService } from './auth.service';
export { carriersService } from './carriers.service';
export { clientsService } from './clients.service';
export { policiesService } from './policies.service';
```

---

## Phase 3: React Query Hooks

### 3.1 Create Query Hooks

Create `src/hooks/use-carriers.ts`:

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { carriersService } from '@/services';
import type { CreateCarrierRequest, UpdateCarrierRequest, CarrierStatus } from '@/types/api';

export const carrierKeys = {
  all: ['carriers'] as const,
  lists: () => [...carrierKeys.all, 'list'] as const,
  list: (filters: string) => [...carrierKeys.lists(), filters] as const,
  details: () => [...carrierKeys.all, 'detail'] as const,
  detail: (id: string) => [...carrierKeys.details(), id] as const,
};

export function useCarriers() {
  return useQuery({
    queryKey: carrierKeys.lists(),
    queryFn: () => carriersService.getAll(),
  });
}

export function useCarrier(id: string) {
  return useQuery({
    queryKey: carrierKeys.detail(id),
    queryFn: () => carriersService.getById(id),
    enabled: !!id,
  });
}

export function useCarriersByStatus(status: CarrierStatus) {
  return useQuery({
    queryKey: carrierKeys.list(status),
    queryFn: () => carriersService.getByStatus(status),
  });
}

export function useCreateCarrier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCarrierRequest) => carriersService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.lists() });
    },
  });
}

export function useUpdateCarrier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCarrierRequest }) =>
      carriersService.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: carrierKeys.lists() });
    },
  });
}

export function useDeactivateCarrier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string }) =>
      carriersService.deactivate(id, reason),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: carrierKeys.lists() });
    },
  });
}
```

Create `src/hooks/use-clients.ts`:

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { clientsService } from '@/services';
import type { ClientFilters, CreateIndividualClientRequest, CreateBusinessClientRequest } from '@/types/api';

export const clientKeys = {
  all: ['clients'] as const,
  lists: () => [...clientKeys.all, 'list'] as const,
  list: (filters: ClientFilters) => [...clientKeys.lists(), filters] as const,
  details: () => [...clientKeys.all, 'detail'] as const,
  detail: (id: string) => [...clientKeys.details(), id] as const,
};

export function useClients(filters?: ClientFilters) {
  return useQuery({
    queryKey: clientKeys.list(filters || {}),
    queryFn: () => clientsService.getAll(filters),
  });
}

export function useClient(id: string) {
  return useQuery({
    queryKey: clientKeys.detail(id),
    queryFn: () => clientsService.getById(id),
    enabled: !!id,
  });
}

export function useCreateIndividualClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateIndividualClientRequest) => clientsService.createIndividual(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clientKeys.lists() });
    },
  });
}

export function useCreateBusinessClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateBusinessClientRequest) => clientsService.createBusiness(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clientKeys.lists() });
    },
  });
}

export function useDeactivateClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => clientsService.deactivate(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: clientKeys.lists() });
    },
  });
}
```

Create `src/hooks/use-policies.ts`:

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { policiesService } from '@/services';
import type {
  PolicyFilters,
  CreatePolicyRequest,
  CancelPolicyRequest,
  AddCoverageRequest,
  UpdateCoverageRequest,
  AddEndorsementRequest,
  RejectEndorsementRequest,
} from '@/types/api';

export const policyKeys = {
  all: ['policies'] as const,
  lists: () => [...policyKeys.all, 'list'] as const,
  list: (filters: PolicyFilters) => [...policyKeys.lists(), filters] as const,
  details: () => [...policyKeys.all, 'detail'] as const,
  detail: (id: string) => [...policyKeys.details(), id] as const,
  expiring: (startDate: string, endDate: string) => [...policyKeys.all, 'expiring', startDate, endDate] as const,
  dueForRenewal: (days: number) => [...policyKeys.all, 'dueForRenewal', days] as const,
  byClient: (clientId: string) => [...policyKeys.all, 'byClient', clientId] as const,
};

export function usePolicies(filters?: PolicyFilters) {
  return useQuery({
    queryKey: policyKeys.list(filters || {}),
    queryFn: () => policiesService.getAll(filters),
  });
}

export function usePolicy(id: string) {
  return useQuery({
    queryKey: policyKeys.detail(id),
    queryFn: () => policiesService.getById(id),
    enabled: !!id,
  });
}

export function useExpiringPolicies(startDate: string, endDate: string) {
  return useQuery({
    queryKey: policyKeys.expiring(startDate, endDate),
    queryFn: () => policiesService.getExpiring(startDate, endDate),
    enabled: !!startDate && !!endDate,
  });
}

export function usePoliciesDueForRenewal(daysUntilExpiration = 60) {
  return useQuery({
    queryKey: policyKeys.dueForRenewal(daysUntilExpiration),
    queryFn: () => policiesService.getDueForRenewal(daysUntilExpiration),
  });
}

export function useClientPolicies(clientId: string) {
  return useQuery({
    queryKey: policyKeys.byClient(clientId),
    queryFn: () => policiesService.getByClient(clientId),
    enabled: !!clientId,
  });
}

export function useCreatePolicy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreatePolicyRequest) => policiesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: policyKeys.lists() });
    },
  });
}

export function useBindPolicy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => policiesService.bind(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: policyKeys.lists() });
    },
  });
}

export function useActivatePolicy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => policiesService.activate(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: policyKeys.lists() });
    },
  });
}

export function useCancelPolicy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CancelPolicyRequest }) =>
      policiesService.cancel(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: policyKeys.lists() });
    },
  });
}

export function useRenewPolicy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => policiesService.renew(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: policyKeys.lists() });
    },
  });
}

// Coverage mutations
export function useAddCoverage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, data }: { policyId: string; data: AddCoverageRequest }) =>
      policiesService.addCoverage(policyId, data),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

export function useUpdateCoverage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, coverageId, data }: { policyId: string; coverageId: string; data: UpdateCoverageRequest }) =>
      policiesService.updateCoverage(policyId, coverageId, data),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

export function useRemoveCoverage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, coverageId }: { policyId: string; coverageId: string }) =>
      policiesService.removeCoverage(policyId, coverageId),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

// Endorsement mutations
export function useAddEndorsement() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, data }: { policyId: string; data: AddEndorsementRequest }) =>
      policiesService.addEndorsement(policyId, data),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

export function useApproveEndorsement() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, endorsementId }: { policyId: string; endorsementId: string }) =>
      policiesService.approveEndorsement(policyId, endorsementId),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

export function useIssueEndorsement() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, endorsementId }: { policyId: string; endorsementId: string }) =>
      policiesService.issueEndorsement(policyId, endorsementId),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

export function useRejectEndorsement() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, endorsementId, data }: { policyId: string; endorsementId: string; data: RejectEndorsementRequest }) =>
      policiesService.rejectEndorsement(policyId, endorsementId, data),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}
```

Create `src/hooks/index.ts`:

```typescript
export * from './use-carriers';
export * from './use-clients';
export * from './use-policies';
```

---

## Phase 4: UI Components

### 4.1 Additional UI Primitives Needed

| Component | Purpose | Priority |
|-----------|---------|----------|
| Select | Dropdown selection | High |
| Textarea | Multi-line input | High |
| Badge | Status indicators | High |
| Dialog/Modal | Confirmation dialogs, forms | High |
| Table | Data display with sorting | High |
| Pagination | Page navigation | High |
| DatePicker | Date selection | High |
| Tabs | Content organization | Medium |
| Toast/Notification | Success/error messages | High |
| Skeleton | Loading states | Medium |
| DropdownMenu | Action menus | Medium |
| Alert | Information banners | Medium |
| Checkbox | Boolean inputs | Medium |
| RadioGroup | Single selection | Medium |
| Switch | Toggle inputs | Low |
| Tooltip | Hover information | Low |

### 4.2 Component Implementation Order

1. **Toast/Notification System** - Critical for user feedback
2. **Dialog/Modal** - Needed for confirmations and forms
3. **Select** - Used in all forms
4. **Badge** - Status display throughout
5. **Table + Pagination** - Core data display
6. **DatePicker** - Policy dates
7. **Tabs** - Detail page organization
8. **Remaining components** as needed

---

## Phase 5: Feature Implementation

### 5.1 Clients Module

**Files to Create:**
```
src/features/clients/
├── components/
│   ├── client-table.tsx
│   ├── client-filters.tsx
│   ├── client-form.tsx
│   ├── client-detail-card.tsx
│   ├── contact-list.tsx
│   ├── address-list.tsx
│   └── create-client-dialog.tsx
├── pages/
│   ├── clients-list-page.tsx
│   └── client-detail-page.tsx
├── hooks/
│   └── use-client-form.ts
└── __tests__/
    ├── client-table.test.tsx
    ├── client-form.test.tsx
    └── clients-list-page.test.tsx
```

**Features:**
- List clients with search and pagination
- Create individual/business client
- View client details with contacts/addresses
- Edit client information
- Deactivate client
- View client policies

### 5.2 Carriers Module

**Files to Create:**
```
src/features/carriers/
├── components/
│   ├── carrier-table.tsx
│   ├── carrier-form.tsx
│   ├── carrier-detail-card.tsx
│   ├── product-list.tsx
│   ├── appetite-list.tsx
│   ├── add-product-dialog.tsx
│   └── add-appetite-dialog.tsx
├── pages/
│   ├── carriers-list-page.tsx
│   └── carrier-detail-page.tsx
└── __tests__/
    ├── carrier-table.test.tsx
    └── carrier-form.test.tsx
```

**Features:**
- List carriers with status filter
- Create/edit carrier
- View carrier with products/appetites
- Add products to carrier
- Add appetite rules
- Deactivate carrier

### 5.3 Policies Module

**Files to Create:**
```
src/features/policies/
├── components/
│   ├── policy-table.tsx
│   ├── policy-filters.tsx
│   ├── policy-form.tsx
│   ├── policy-detail-card.tsx
│   ├── policy-status-badge.tsx
│   ├── policy-actions.tsx
│   ├── coverage-list.tsx
│   ├── coverage-form.tsx
│   ├── endorsement-list.tsx
│   ├── endorsement-form.tsx
│   ├── endorsement-workflow-actions.tsx
│   ├── cancel-policy-dialog.tsx
│   └── renew-policy-dialog.tsx
├── pages/
│   ├── policies-list-page.tsx
│   ├── policy-detail-page.tsx
│   ├── create-policy-page.tsx
│   ├── expiring-policies-page.tsx
│   └── renewal-dashboard-page.tsx
└── __tests__/
    ├── policy-table.test.tsx
    ├── policy-form.test.tsx
    ├── coverage-form.test.tsx
    └── endorsement-workflow.test.tsx
```

**Features:**
- List policies with advanced filtering
- Create new policy (multi-step form)
- View policy details with coverages/endorsements
- Policy lifecycle actions (bind, activate, cancel, renew)
- Coverage management (add, update, remove)
- Endorsement workflow (add, approve, issue, reject)
- Expiring policies view
- Renewal dashboard

### 5.4 Dashboard Enhancement

**Files to Create:**
```
src/features/dashboard/
├── components/
│   ├── stat-card.tsx
│   ├── recent-policies-widget.tsx
│   ├── expiring-policies-widget.tsx
│   ├── renewal-alerts-widget.tsx
│   └── premium-chart.tsx
├── pages/
│   └── dashboard-page.tsx (enhance existing)
└── __tests__/
    └── dashboard-page.test.tsx
```

---

## Phase 6: Testing Strategy

### 6.1 Testing Layers

| Layer | What to Test | Tools |
|-------|--------------|-------|
| Unit | Utility functions, hooks logic | Vitest |
| Component | UI rendering, user interactions | React Testing Library |
| Integration | Page workflows, API integration | RTL + MSW |
| E2E | Critical user journeys | Playwright (optional) |

### 6.2 Component Testing Guidelines

```typescript
// Example: client-form.test.tsx
import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@/test/utils';
import userEvent from '@testing-library/user-event';
import { ClientForm } from './client-form';

describe('ClientForm', () => {
  it('renders individual client form fields', () => {
    render(<ClientForm type="individual" onSubmit={vi.fn()} />);

    expect(screen.getByLabelText(/first name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/last name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
  });

  it('validates required fields', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    render(<ClientForm type="individual" onSubmit={onSubmit} />);

    await user.click(screen.getByRole('button', { name: /save/i }));

    expect(await screen.findByText(/first name is required/i)).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('submits form with valid data', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    render(<ClientForm type="individual" onSubmit={onSubmit} />);

    await user.type(screen.getByLabelText(/first name/i), 'John');
    await user.type(screen.getByLabelText(/last name/i), 'Doe');
    await user.type(screen.getByLabelText(/email/i), 'john@example.com');
    await user.click(screen.getByRole('button', { name: /save/i }));

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
      });
    });
  });
});
```

### 6.3 Hook Testing Guidelines

```typescript
// Example: use-policies.test.ts
import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { usePolicies, useCreatePolicy } from './use-policies';

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe('usePolicies', () => {
  it('fetches policies successfully', async () => {
    const { result } = renderHook(() => usePolicies(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data?.items).toBeDefined();
  });
});

describe('useCreatePolicy', () => {
  it('creates policy and invalidates cache', async () => {
    const { result } = renderHook(() => useCreatePolicy(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      clientId: 'client-1',
      carrierId: 'carrier-1',
      lineOfBusiness: 'PersonalAuto',
      policyType: 'Auto Insurance',
      effectiveDate: '2024-01-01',
      expirationDate: '2025-01-01',
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.id).toBeDefined();
  });
});
```

### 6.4 MSW Handlers for Testing

Expand `src/test/mocks/handlers.ts` for each feature:

```typescript
// Policy handlers
http.get(`${API_URL}/policies`, ({ request }) => {
  const url = new URL(request.url);
  const page = parseInt(url.searchParams.get('page') || '1');
  const pageSize = parseInt(url.searchParams.get('pageSize') || '20');

  return HttpResponse.json({
    items: mockPolicies.slice((page - 1) * pageSize, page * pageSize),
    totalCount: mockPolicies.length,
    pageNumber: page,
    pageSize: pageSize,
    totalPages: Math.ceil(mockPolicies.length / pageSize),
  });
}),

http.get(`${API_URL}/policies/:id`, ({ params }) => {
  const policy = mockPolicies.find(p => p.id === params.id);
  if (!policy) {
    return HttpResponse.json({ error: 'Policy not found' }, { status: 404 });
  }
  return HttpResponse.json(policy);
}),

http.post(`${API_URL}/policies`, async ({ request }) => {
  const body = await request.json();
  return HttpResponse.json({ id: 'new-policy-id' }, { status: 201 });
}),

http.post(`${API_URL}/policies/:id/bind`, () => {
  return new HttpResponse(null, { status: 204 });
}),

// Add more handlers as needed...
```

### 6.5 Test Coverage Targets

| Category | Target Coverage |
|----------|-----------------|
| UI Components | 80% |
| Hooks | 90% |
| Services | 70% |
| Utils | 95% |
| Pages | 60% |

---

## Phase 7: Implementation Checklist

> **Last audited: 2026-02-20** — All implementation is complete including Documents module and frontend tests.

### Week 1: Foundation ✅ COMPLETE
- [x] Set up testing infrastructure (Vitest, RTL, MSW)
- [x] Create API types (`src/types/api.ts`)
- [x] Create service modules (auth, carriers, clients, policies, quotes, claims, commissions, roles, users)
- [x] Create React Query hooks (use-carriers, use-clients, use-policies, use-quotes, use-claims, use-commissions, use-roles, use-users)
- [ ] Write tests for hooks ⬅ **REMAINING**

### Week 2: Core UI Components ✅ COMPLETE
- [x] Toast/Notification system
- [x] Dialog/Modal component
- [x] Select component
- [x] Badge component
- [x] Table component with sorting
- [x] Pagination component
- [x] DatePicker component
- [x] Tabs component
- [x] Textarea component
- [x] Checkbox component
- [x] Skeleton loading component
- [x] Write component tests (Badge tests)
- [ ] Additional component tests ⬅ **REMAINING**

### Week 3: Clients Module ✅ COMPLETE
- [x] Client list page with table (`clients-page.tsx`, `client-table.tsx`)
- [x] Client filters (`client-filters.tsx`)
- [x] Create client dialog — individual + business (`client-dialog.tsx`, `client-form.tsx`)
- [x] Client detail page (`client-detail-page.tsx`)
- [x] Contact management tab (`tabs/client-contacts.tsx`)
- [x] Address management tab (`tabs/client-addresses.tsx`)
- [x] Client policies tab (`tabs/client-policies.tsx`)
- [ ] Client tests ⬅ **REMAINING**

### Week 4: Carriers Module ✅ COMPLETE
- [x] Carrier list page (`carriers-page.tsx`, `carrier-table.tsx`)
- [x] Carrier create/edit dialog (`carrier-dialog.tsx`)
- [x] Carrier detail page (`carrier-detail-page.tsx`)
- [x] Product management (`carrier-products.tsx`)
- [x] Appetite management (`carrier-appetites.tsx`)
- [ ] Carrier tests ⬅ **REMAINING**

### Week 5-6: Policies Module ✅ COMPLETE
- [x] Policy list page with filters (`policies-page.tsx`, `policy-table.tsx`, `policy-filters.tsx`)
- [x] Create policy wizard — 5 steps (`policy-wizard.tsx`, step-client/carrier/details/coverages/review)
- [x] Policy detail page (`policy-detail-page.tsx`)
- [x] Coverage management UI (`tabs/policy-coverages.tsx`)
- [x] Endorsement workflow UI (`tabs/policy-endorsements.tsx`)
- [x] Policy lifecycle actions + status flow (`policy-status-flow.tsx`)
- [x] Policy dashboard (`policy-dashboard.tsx`)
- [ ] Policy tests ⬅ **REMAINING**

### Week 7: Dashboard & Polish ✅ COMPLETE
- [x] Enhanced dashboard with widgets (`dashboard-page.tsx`)
- [x] Stats cards widget (`dashboard-stats.tsx`)
- [x] Expiring/alerts widget (`dashboard-alerts.tsx`)
- [x] Renewals widget (`dashboard-renewals.tsx`)
- [x] Activity feed widget (`dashboard-activity.tsx`)
- [x] Loading states (skeletons) — skeleton component available
- [ ] Error boundaries ⬅ **REMAINING**
- [ ] Final testing & bug fixes ⬅ **REMAINING**

### Additional (not originally planned) ✅ COMPLETE
- [x] Quotes module (quotes-page, quote-detail-page, quote-wizard, record-response-dialog)
- [x] Claims module (claims-page, claim-detail-page, fnol-wizard)
- [x] Commissions module (commissions-page, statement-detail-page)
- [x] Reports page (reports-page.tsx)
- [x] Settings — Roles (roles-page, role-detail-page, role-permissions)
- [x] Settings — Users (users-page, user-detail-page, user-role-manager)
- [x] Documents module (documents-page, upload-dialog, document-list, templates-page, template-editor-dialog, generate-coi-dialog)

### Phase 8: Frontend Tests ✅ COMPLETE
- [x] Hook tests: use-clients, use-carriers, use-policies, use-quotes, use-claims, use-commissions, use-roles, use-users, use-documents
- [x] Page integration tests: clients-page, carriers-page, documents-page, documents templates-page
- [x] MSW handlers for all feature modules (clients, carriers, policies, quotes, claims, commissions, documents)
- [x] COI generation dialog tests

---

## Appendix: File Structure Summary

```
src/IBS.Web/src/
├── components/
│   ├── common/
│   │   ├── layout.tsx
│   │   ├── sidebar.tsx
│   │   └── header.tsx
│   └── ui/
│       ├── button.tsx
│       ├── card.tsx
│       ├── input.tsx
│       ├── label.tsx
│       ├── select.tsx          # NEW
│       ├── textarea.tsx        # NEW
│       ├── badge.tsx           # NEW
│       ├── dialog.tsx          # NEW
│       ├── table.tsx           # NEW
│       ├── pagination.tsx      # NEW
│       ├── date-picker.tsx     # NEW
│       ├── tabs.tsx            # NEW
│       ├── toast.tsx           # NEW
│       └── skeleton.tsx        # NEW
├── features/
│   ├── auth/
│   │   └── login-page.tsx
│   ├── dashboard/
│   │   ├── components/         # NEW
│   │   └── dashboard-page.tsx
│   ├── clients/                # NEW
│   │   ├── components/
│   │   ├── pages/
│   │   └── __tests__/
│   ├── carriers/               # NEW
│   │   ├── components/
│   │   ├── pages/
│   │   └── __tests__/
│   └── policies/               # NEW
│       ├── components/
│       ├── pages/
│       └── __tests__/
├── hooks/
│   ├── use-carriers.ts         # NEW
│   ├── use-clients.ts          # NEW
│   ├── use-policies.ts         # NEW
│   └── index.ts                # NEW
├── services/
│   ├── auth.service.ts         # NEW
│   ├── carriers.service.ts     # NEW
│   ├── clients.service.ts      # NEW
│   ├── policies.service.ts     # NEW
│   └── index.ts                # NEW
├── stores/
│   └── auth.ts
├── types/
│   └── api.ts                  # NEW
├── lib/
│   ├── api.ts
│   └── utils.ts
├── test/                       # NEW
│   ├── setup.ts
│   ├── utils.tsx
│   └── mocks/
│       ├── handlers.ts
│       └── server.ts
├── App.tsx
├── main.tsx
└── index.css
```

---

## Running Tests

```bash
# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with UI
npm run test:ui

# Run tests with coverage
npm run test:coverage

# Run specific test file
npm test -- src/features/clients/__tests__/client-form.test.tsx
```
