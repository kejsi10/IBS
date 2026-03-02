import { api } from '@/lib/api';
import type {
  CommissionScheduleDetail,
  CommissionScheduleListItem,
  CommissionStatementDetail,
  CommissionStatementListItem,
  CommissionStatistics,
  CommissionSummaryEntry,
  ProducerReportEntry,
  CreateScheduleRequest,
  UpdateScheduleRequest,
  CreateStatementRequest,
  AddLineItemRequest,
  DisputeLineItemRequest,
  AddProducerSplitRequest,
  UpdateStatementStatusRequest,
  ScheduleFilters,
  StatementFilters,
  PaginatedResult,
  CreateResponse,
} from '@/types/api';

/** Backend response shape for paginated schedule lists. */
interface BackendScheduleListResult {
  schedules: CommissionScheduleListItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/** Backend response shape for paginated statement lists. */
interface BackendStatementListResult {
  statements: CommissionStatementListItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/** Maps backend schedule list to frontend PaginatedResult. */
const mapScheduleListResult = (dto: BackendScheduleListResult): PaginatedResult<CommissionScheduleListItem> => ({
  items: dto.schedules,
  totalCount: dto.totalCount,
  page: dto.pageNumber,
  pageSize: dto.pageSize,
  totalPages: dto.totalPages,
});

/** Maps backend statement list to frontend PaginatedResult. */
const mapStatementListResult = (dto: BackendStatementListResult): PaginatedResult<CommissionStatementListItem> => ({
  items: dto.statements,
  totalCount: dto.totalCount,
  page: dto.pageNumber,
  pageSize: dto.pageSize,
  totalPages: dto.totalPages,
});

/**
 * Service for commissions management operations.
 */
export const commissionsService = {
  // ── Schedules ──

  /** Gets a paginated list of commission schedules. */
  getSchedules: async (filters?: ScheduleFilters): Promise<PaginatedResult<CommissionScheduleListItem>> => {
    const response = await api.get<BackendScheduleListResult>('/commissions/schedules', { params: filters });
    return mapScheduleListResult(response.data);
  },

  /** Gets a schedule by ID. */
  getScheduleById: async (id: string): Promise<CommissionScheduleDetail> => {
    const response = await api.get<CommissionScheduleDetail>(`/commissions/schedules/${id}`);
    return response.data;
  },

  /** Creates a new commission schedule. */
  createSchedule: async (data: CreateScheduleRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/commissions/schedules', data);
    return response.data;
  },

  /** Updates an existing commission schedule. */
  updateSchedule: async (id: string, data: UpdateScheduleRequest): Promise<void> => {
    await api.put(`/commissions/schedules/${id}`, data);
  },

  /** Deactivates a commission schedule. */
  deactivateSchedule: async (id: string): Promise<void> => {
    await api.post(`/commissions/schedules/${id}/deactivate`);
  },

  // ── Statements ──

  /** Gets a paginated list of commission statements. */
  getStatements: async (filters?: StatementFilters): Promise<PaginatedResult<CommissionStatementListItem>> => {
    const response = await api.get<BackendStatementListResult>('/commissions/statements', { params: filters });
    return mapStatementListResult(response.data);
  },

  /** Gets a statement by ID with line items and splits. */
  getStatementById: async (id: string): Promise<CommissionStatementDetail> => {
    const response = await api.get<CommissionStatementDetail>(`/commissions/statements/${id}`);
    return response.data;
  },

  /** Creates a new commission statement. */
  createStatement: async (data: CreateStatementRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/commissions/statements', data);
    return response.data;
  },

  /** Adds a line item to a statement. */
  addLineItem: async (statementId: string, data: AddLineItemRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/commissions/statements/${statementId}/line-items`, data);
    return response.data;
  },

  /** Reconciles a line item. */
  reconcileLineItem: async (statementId: string, lineItemId: string): Promise<void> => {
    await api.put(`/commissions/statements/${statementId}/line-items/${lineItemId}/reconcile`);
  },

  /** Disputes a line item. */
  disputeLineItem: async (statementId: string, lineItemId: string, data: DisputeLineItemRequest): Promise<void> => {
    await api.post(`/commissions/statements/${statementId}/line-items/${lineItemId}/dispute`, data);
  },

  /** Adds a producer split. */
  addProducerSplit: async (statementId: string, lineItemId: string, data: AddProducerSplitRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/commissions/statements/${statementId}/line-items/${lineItemId}/splits`, data);
    return response.data;
  },

  /** Updates statement status. */
  updateStatementStatus: async (statementId: string, data: UpdateStatementStatusRequest): Promise<void> => {
    await api.put(`/commissions/statements/${statementId}/status`, data);
  },

  // ── Statistics & Reports ──

  /** Gets commission statistics for the dashboard. */
  getStatistics: async (): Promise<CommissionStatistics> => {
    const response = await api.get<CommissionStatistics>('/commissions/statistics');
    return response.data;
  },

  /** Gets commission summary report. */
  getSummaryReport: async (carrierId?: string, periodMonth?: number, periodYear?: number): Promise<CommissionSummaryEntry[]> => {
    const response = await api.get<CommissionSummaryEntry[]>('/commissions/reports/summary', {
      params: { carrierId, periodMonth, periodYear },
    });
    return response.data;
  },

  /** Gets producer report. */
  getProducerReport: async (producerId?: string, periodMonth?: number, periodYear?: number): Promise<ProducerReportEntry[]> => {
    const response = await api.get<ProducerReportEntry[]>('/commissions/reports/producer', {
      params: { producerId, periodMonth, periodYear },
    });
    return response.data;
  },
};
