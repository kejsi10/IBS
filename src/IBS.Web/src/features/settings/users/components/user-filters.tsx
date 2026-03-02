import { useTranslation } from 'react-i18next';
import { SearchInput } from '@/components/common/search-input';
import { QuickFilters, type FilterOption } from '@/components/common/quick-filters';

/** Filter value type for user active status */
export type UserStatusFilter = 'all' | 'active' | 'inactive';

/**
 * Props for the UserFilters component.
 */
export interface UserFiltersProps {
  /** Current search query */
  search: string;
  /** Callback when search changes */
  onSearchChange: (value: string) => void;
  /** Current status filter */
  statusFilter: UserStatusFilter;
  /** Callback when status filter changes */
  onStatusFilterChange: (value: UserStatusFilter) => void;
  /** Counts for each status */
  counts?: {
    all: number;
    active: number;
    inactive: number;
  };
}

/**
 * User list filters with search and quick status filters.
 */
export function UserFilters({
  search,
  onSearchChange,
  statusFilter,
  onStatusFilterChange,
  counts,
}: UserFiltersProps) {
  const { t } = useTranslation();

  const filterOptions: FilterOption<UserStatusFilter>[] = [
    { value: 'all', label: t('settings.users.status.all'), count: counts?.all },
    { value: 'active', label: t('settings.users.status.active'), count: counts?.active },
    { value: 'inactive', label: t('settings.users.status.inactive'), count: counts?.inactive },
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
        placeholder={t('settings.users.searchPlaceholder')}
        className="w-full sm:w-80"
      />
    </div>
  );
}
