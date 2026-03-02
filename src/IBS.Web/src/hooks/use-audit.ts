import { useQuery } from '@tanstack/react-query';
import { auditService } from '@/services/audit.service';
import type { AuditFilters } from '@/types/audit';

/**
 * Query keys for audit-related queries.
 */
export const auditKeys = {
  all: ['audit'] as const,
  lists: () => [...auditKeys.all, 'list'] as const,
  list: (filters: AuditFilters) => [...auditKeys.lists(), filters] as const,
};

/**
 * Hook to fetch a paginated list of audit log entries.
 */
export function useAuditLogs(filters?: AuditFilters) {
  return useQuery({
    queryKey: auditKeys.list(filters || {}),
    queryFn: () => auditService.getAll(filters),
  });
}
