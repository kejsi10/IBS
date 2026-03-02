import { useTranslation } from 'react-i18next';
import { SearchInput } from '@/components/common/search-input';
import { QuickFilters, type FilterOption } from '@/components/common/quick-filters';
import type { ClientStatus } from '@/types/api';

/** Filter value type including 'all' option */
export type ClientStatusFilter = ClientStatus | 'all';

/**
 * Props for the ClientFilters component.
 */
export interface ClientFiltersProps {
  /** Current search query */
  search: string;
  /** Callback when search changes */
  onSearchChange: (value: string) => void;
  /** Current status filter */
  statusFilter: ClientStatusFilter;
  /** Callback when status filter changes */
  onStatusFilterChange: (value: ClientStatusFilter) => void;
  /** Counts for each status */
  counts?: {
    all: number;
    active: number;
    inactive: number;
  };
}

/**
 * Client list filters with search and quick status filters.
 */
export function ClientFilters({
  search,
  onSearchChange,
  statusFilter,
  onStatusFilterChange,
  counts,
}: ClientFiltersProps) {
  const { t } = useTranslation();

  const filterOptions: FilterOption<ClientStatusFilter>[] = [
    { value: 'all', label: t('clients.status.all'), count: counts?.all },
    { value: 'Active', label: t('clients.status.active'), count: counts?.active },
    { value: 'Inactive', label: t('clients.status.inactive'), count: counts?.inactive },
  ];

  return (
    <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <QuickFilters
        options={filterOptions}
        value={statusFilter}
        onChange={onStatusFilterChange}
      />
      <SearchInput
        value={search}
        onSearch={onSearchChange}
        placeholder={t('clients.searchPlaceholder')}
        className="w-full sm:w-80"
      />
    </div>
  );
}
