/**
 * Audit log entry from the API.
 */
export interface AuditLogEntry {
  id: string;
  userId: string | null;
  userEmail: string | null;
  action: 'Create' | 'Update' | 'Delete';
  entityType: string;
  entityId: string;
  changes: string | null;
  timestamp: string;
}

/**
 * Paginated result for audit log entries.
 */
export interface AuditPagedResult {
  items: AuditLogEntry[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

/**
 * Filters for audit log queries.
 */
export interface AuditFilters {
  page?: number;
  pageSize?: number;
  entityType?: string;
  entityId?: string;
  action?: string;
  from?: string;
  to?: string;
}
