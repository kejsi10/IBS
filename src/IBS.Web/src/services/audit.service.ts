import { api } from '@/lib/api';
import type { AuditPagedResult, AuditFilters } from '@/types/audit';

/**
 * Service for audit log operations.
 */
export const auditService = {
  /**
   * Gets a paginated list of audit log entries.
   */
  getAll: async (filters?: AuditFilters): Promise<AuditPagedResult> => {
    const response = await api.get<AuditPagedResult>('/audit', { params: filters });
    return response.data;
  },
};
