import { api } from '@/lib/api';
import type {
  Policy,
  PolicySummary,
  PolicyStatus,
  CreatePolicyRequest,
  CancelPolicyRequest,
  ReinstatePolicyRequest,
  AddCoverageRequest,
  UpdateCoverageRequest,
  AddEndorsementRequest,
  RejectEndorsementRequest,
  PolicyFilters,
  PaginatedResult,
  PolicyHistoryResult,
  RenewalComparisonDto,
  CreateResponse,
} from '@/types/api';

// ========================================
// Backend → Frontend Mapping Functions
// ========================================

/**
 * Backend response shape for paginated policy lists.
 * Uses different field names than frontend PaginatedResult.
 */
interface BackendPolicyListResult {
  policies: PolicySummary[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Maps backend display-name status strings to frontend PolicyStatus enum names.
 * Backend uses GetDisplayName() which adds spaces to multi-word statuses.
 */
const mapPolicyStatus = (displayName: string): PolicyStatus => {
  const statusMap: Record<string, PolicyStatus> = {
    'Pending Cancellation': 'PendingCancellation',
    'Pending Renewal': 'PendingRenewal',
    'Non-Renewed': 'NonRenewed',
  };
  return (statusMap[displayName] ?? displayName) as PolicyStatus;
};

/**
 * Maps backend PolicyListResult to frontend PaginatedResult.
 * Converts field names and maps status display names on each item.
 */
const mapPolicyListResult = (dto: BackendPolicyListResult): PaginatedResult<PolicySummary> => ({
  items: dto.policies.map((item) => ({
    ...item,
    status: mapPolicyStatus(item.status),
  })),
  totalCount: dto.totalCount,
  page: dto.pageNumber,
  pageSize: dto.pageSize,
  totalPages: dto.totalPages,
});

/**
 * Maps status display name on a single policy detail response.
 */
const mapPolicyDetail = (dto: Policy): Policy => ({
  ...dto,
  status: mapPolicyStatus(dto.status),
});

/**
 * Service for policy management operations.
 */
export const policiesService = {
  // ========================================
  // Policy CRUD Operations
  // ========================================

  /**
   * Gets a paginated list of policies with optional filtering.
   */
  getAll: async (filters?: PolicyFilters): Promise<PaginatedResult<PolicySummary>> => {
    const response = await api.get<BackendPolicyListResult>('/policies', {
      params: filters,
    });
    return mapPolicyListResult(response.data);
  },

  /**
   * Gets a policy by ID.
   */
  getById: async (id: string): Promise<Policy> => {
    const response = await api.get<Policy>(`/policies/${id}`);
    return mapPolicyDetail(response.data);
  },

  /**
   * Creates a new policy.
   */
  create: async (data: CreatePolicyRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/policies', data);
    return response.data;
  },

  // ========================================
  // Policy Lifecycle Operations
  // ========================================

  /**
   * Binds a draft policy.
   */
  bind: async (id: string): Promise<void> => {
    await api.post(`/policies/${id}/bind`);
  },

  /**
   * Activates (issues) a bound policy.
   */
  activate: async (id: string): Promise<void> => {
    await api.post(`/policies/${id}/activate`);
  },

  /**
   * Cancels an active policy.
   */
  cancel: async (id: string, data: CancelPolicyRequest): Promise<void> => {
    await api.post(`/policies/${id}/cancel`, data);
  },

  /**
   * Reinstates a cancelled policy.
   */
  reinstate: async (id: string, data: ReinstatePolicyRequest): Promise<void> => {
    await api.post(`/policies/${id}/reinstate`, data);
  },

  /**
   * Creates a renewal policy from an active policy.
   */
  renew: async (id: string): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/policies/${id}/renew`);
    return response.data;
  },

  // ========================================
  // Coverage Management
  // ========================================

  /**
   * Adds a coverage to a policy.
   */
  addCoverage: async (policyId: string, data: AddCoverageRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/policies/${policyId}/coverages`, data);
    return response.data;
  },

  /**
   * Updates an existing coverage.
   */
  updateCoverage: async (policyId: string, coverageId: string, data: UpdateCoverageRequest): Promise<void> => {
    await api.put(`/policies/${policyId}/coverages/${coverageId}`, data);
  },

  /**
   * Removes a coverage from a policy.
   */
  removeCoverage: async (policyId: string, coverageId: string): Promise<void> => {
    await api.delete(`/policies/${policyId}/coverages/${coverageId}`);
  },

  // ========================================
  // Endorsement Management
  // ========================================

  /**
   * Adds an endorsement to a policy.
   */
  addEndorsement: async (policyId: string, data: AddEndorsementRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/policies/${policyId}/endorsements`, data);
    return response.data;
  },

  /**
   * Approves a pending endorsement.
   */
  approveEndorsement: async (policyId: string, endorsementId: string): Promise<void> => {
    await api.post(`/policies/${policyId}/endorsements/${endorsementId}/approve`);
  },

  /**
   * Issues an approved endorsement.
   */
  issueEndorsement: async (policyId: string, endorsementId: string): Promise<void> => {
    await api.post(`/policies/${policyId}/endorsements/${endorsementId}/issue`);
  },

  /**
   * Rejects a pending endorsement.
   */
  rejectEndorsement: async (policyId: string, endorsementId: string, data: RejectEndorsementRequest): Promise<void> => {
    await api.post(`/policies/${policyId}/endorsements/${endorsementId}/reject`, data);
  },

  // ========================================
  // Policy Queries
  // ========================================

  /**
   * Gets policies expiring within a date range.
   */
  getExpiring: async (
    startDate: string,
    endDate: string,
    page = 1,
    pageSize = 20
  ): Promise<PaginatedResult<PolicySummary>> => {
    const response = await api.get<BackendPolicyListResult>('/policies/expiring', {
      params: { startDate, endDate, page, pageSize },
    });
    return mapPolicyListResult(response.data);
  },

  /**
   * Gets policies due for renewal within N days.
   */
  getDueForRenewal: async (
    daysUntilExpiration = 60,
    page = 1,
    pageSize = 20
  ): Promise<PaginatedResult<PolicySummary>> => {
    const response = await api.get<BackendPolicyListResult>('/policies/due-for-renewal', {
      params: { daysUntilExpiration, page, pageSize },
    });
    return mapPolicyListResult(response.data);
  },

  /**
   * Gets the renewal offer comparison for a policy.
   */
  getRenewalComparison: async (id: string): Promise<RenewalComparisonDto> => {
    const response = await api.get<RenewalComparisonDto>(`/policies/${id}/renewal-comparison`);
    return response.data;
  },

  /**
   * Creates a renewal quote linked to this policy.
   */
  createRenewalQuote: async (id: string): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/policies/${id}/renewal-quote`);
    return response.data;
  },

  /**
   * Gets paginated history entries for a policy.
   */
  getHistory: async (id: string, page = 1, pageSize = 50): Promise<PolicyHistoryResult> => {
    const response = await api.get<PolicyHistoryResult>(`/policies/${id}/history`, {
      params: { page, pageSize },
    });
    return response.data;
  },

  /**
   * Gets all policies for a specific client.
   */
  getByClient: async (
    clientId: string,
    params?: { page?: number; pageSize?: number }
  ): Promise<PaginatedResult<PolicySummary>> => {
    const response = await api.get<BackendPolicyListResult>(`/clients/${clientId}/policies`, {
      params,
    });
    return mapPolicyListResult(response.data);
  },
};
