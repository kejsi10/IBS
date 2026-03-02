import { api } from '@/lib/api';
import type {
  ClaimDetail,
  ClaimListItem,
  ClaimStatistics,
  CreateClaimRequest,
  UpdateClaimStatusRequest,
  AddClaimNoteRequest,
  SetReserveRequest,
  AuthorizePaymentRequest,
  VoidPaymentRequest,
  ClaimFilters,
  PaginatedResult,
  CreateResponse,
} from '@/types/api';

/**
 * Backend response shape for paginated claim lists.
 */
interface BackendClaimListResult {
  claims: ClaimListItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Maps backend ClaimSearchResult to frontend PaginatedResult.
 */
const mapClaimListResult = (dto: BackendClaimListResult): PaginatedResult<ClaimListItem> => ({
  items: dto.claims,
  totalCount: dto.totalCount,
  page: dto.pageNumber,
  pageSize: dto.pageSize,
  totalPages: dto.totalPages,
});

/**
 * Service for claims management operations.
 */
export const claimsService = {
  /**
   * Gets a paginated list of claims with optional filtering.
   */
  getAll: async (filters?: ClaimFilters): Promise<PaginatedResult<ClaimListItem>> => {
    const response = await api.get<BackendClaimListResult>('/claims', {
      params: filters,
    });
    return mapClaimListResult(response.data);
  },

  /**
   * Gets a claim by ID.
   */
  getById: async (id: string): Promise<ClaimDetail> => {
    const response = await api.get<ClaimDetail>(`/claims/${id}`);
    return response.data;
  },

  /**
   * Gets claim statistics for the dashboard.
   */
  getStatistics: async (): Promise<ClaimStatistics> => {
    const response = await api.get<ClaimStatistics>('/claims/statistics');
    return response.data;
  },

  /**
   * Creates a new claim (FNOL).
   */
  create: async (data: CreateClaimRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/claims', data);
    return response.data;
  },

  /**
   * Updates the status of a claim.
   */
  updateStatus: async (claimId: string, data: UpdateClaimStatusRequest): Promise<void> => {
    await api.put(`/claims/${claimId}/status`, data);
  },

  /**
   * Adds a note to a claim.
   */
  addNote: async (claimId: string, data: AddClaimNoteRequest): Promise<void> => {
    await api.post(`/claims/${claimId}/notes`, data);
  },

  /**
   * Sets a reserve on a claim.
   */
  setReserve: async (claimId: string, data: SetReserveRequest): Promise<void> => {
    await api.post(`/claims/${claimId}/reserves`, data);
  },

  /**
   * Authorizes a payment on a claim.
   */
  authorizePayment: async (claimId: string, data: AuthorizePaymentRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/claims/${claimId}/payments`, data);
    return response.data;
  },

  /**
   * Issues an authorized payment.
   */
  issuePayment: async (claimId: string, paymentId: string): Promise<void> => {
    await api.put(`/claims/${claimId}/payments/${paymentId}/issue`);
  },

  /**
   * Voids a payment.
   */
  voidPayment: async (claimId: string, paymentId: string, data: VoidPaymentRequest): Promise<void> => {
    await api.put(`/claims/${claimId}/payments/${paymentId}/void`, data);
  },
};
