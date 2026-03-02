import { api } from '@/lib/api';
import type {
  QuoteDetail,
  QuoteSummary,
  QuoteSummaryStats,
  CreateQuoteRequest,
  AddCarrierToQuoteRequest,
  RecordQuoteResponseRequest,
  QuoteFilters,
  PaginatedResult,
  CreateResponse,
} from '@/types/api';

// ========================================
// Backend → Frontend Mapping Functions
// ========================================

/**
 * Backend response shape for paginated quote lists.
 */
interface BackendQuoteListResult {
  quotes: QuoteSummary[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Maps backend QuoteSearchResult to frontend PaginatedResult.
 */
const mapQuoteListResult = (dto: BackendQuoteListResult): PaginatedResult<QuoteSummary> => ({
  items: dto.quotes,
  totalCount: dto.totalCount,
  page: dto.pageNumber,
  pageSize: dto.pageSize,
  totalPages: dto.totalPages,
});

/**
 * Service for quote management operations.
 */
export const quotesService = {
  // ========================================
  // Quote CRUD Operations
  // ========================================

  /**
   * Gets a paginated list of quotes with optional filtering.
   */
  getAll: async (filters?: QuoteFilters): Promise<PaginatedResult<QuoteSummary>> => {
    const response = await api.get<BackendQuoteListResult>('/quotes', {
      params: filters,
    });
    return mapQuoteListResult(response.data);
  },

  /**
   * Gets a quote by ID.
   */
  getById: async (id: string): Promise<QuoteDetail> => {
    const response = await api.get<QuoteDetail>(`/quotes/${id}`);
    return response.data;
  },

  /**
   * Gets quote summary statistics.
   */
  getSummary: async (): Promise<QuoteSummaryStats> => {
    const response = await api.get<QuoteSummaryStats>('/quotes/summary');
    return response.data;
  },

  /**
   * Gets quotes for a specific client.
   */
  getByClient: async (
    clientId: string,
    params?: { page?: number; pageSize?: number }
  ): Promise<PaginatedResult<QuoteSummary>> => {
    const response = await api.get<BackendQuoteListResult>(`/clients/${clientId}/quotes`, {
      params,
    });
    return mapQuoteListResult(response.data);
  },

  /**
   * Creates a new quote.
   */
  create: async (data: CreateQuoteRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/quotes', data);
    return response.data;
  },

  // ========================================
  // Carrier Management
  // ========================================

  /**
   * Adds a carrier to a quote.
   */
  addCarrier: async (quoteId: string, data: AddCarrierToQuoteRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/quotes/${quoteId}/carriers`, data);
    return response.data;
  },

  /**
   * Removes a carrier from a quote.
   */
  removeCarrier: async (quoteId: string, quoteCarrierId: string): Promise<void> => {
    await api.delete(`/quotes/${quoteId}/carriers/${quoteCarrierId}`);
  },

  // ========================================
  // Quote Lifecycle Operations
  // ========================================

  /**
   * Submits a quote to carriers.
   */
  submit: async (quoteId: string): Promise<void> => {
    await api.post(`/quotes/${quoteId}/submit`);
  },

  /**
   * Records a carrier's response to a quote.
   */
  recordResponse: async (
    quoteId: string,
    quoteCarrierId: string,
    data: RecordQuoteResponseRequest
  ): Promise<void> => {
    await api.post(`/quotes/${quoteId}/carriers/${quoteCarrierId}/respond`, data);
  },

  /**
   * Accepts a carrier's quote, creating a draft policy.
   */
  acceptCarrier: async (
    quoteId: string,
    quoteCarrierId: string
  ): Promise<{ policyId: string }> => {
    const response = await api.post<{ policyId: string }>(
      `/quotes/${quoteId}/carriers/${quoteCarrierId}/accept`
    );
    return response.data;
  },

  /**
   * Cancels a quote.
   */
  cancel: async (quoteId: string): Promise<void> => {
    await api.post(`/quotes/${quoteId}/cancel`);
  },
};
