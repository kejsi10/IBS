import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { ChevronDown, X } from 'lucide-react';
import { SearchInput } from '@/components/common/search-input';
import { QuickFilters, type FilterOption } from '@/components/common/quick-filters';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import type { PolicyStatus } from '@/types/api';

/** Filter value type including 'all' option */
export type PolicyStatusFilter = PolicyStatus | 'all';

/**
 * Advanced filter values.
 */
export interface AdvancedFilters {
  effectiveDateFrom?: string;
  effectiveDateTo?: string;
  expirationDateFrom?: string;
  expirationDateTo?: string;
  minPremium?: number;
  maxPremium?: number;
}

/**
 * Props for the PolicyFilters component.
 */
export interface PolicyFiltersProps {
  /** Current search query */
  search: string;
  /** Callback when search changes */
  onSearchChange: (value: string) => void;
  /** Current status filter */
  statusFilter: PolicyStatusFilter;
  /** Callback when status filter changes */
  onStatusFilterChange: (value: PolicyStatusFilter) => void;
  /** Advanced filters */
  advancedFilters: AdvancedFilters;
  /** Callback when advanced filters change */
  onAdvancedFiltersChange: (filters: AdvancedFilters) => void;
  /** Counts for each status */
  counts?: {
    all: number;
    draft: number;
    bound: number;
    active: number;
    cancelled: number;
    expired: number;
  };
}

/**
 * Policy list filters with search, quick status filters, and advanced filters.
 */
export function PolicyFilters({
  search,
  onSearchChange,
  statusFilter,
  onStatusFilterChange,
  advancedFilters,
  onAdvancedFiltersChange,
  counts,
}: PolicyFiltersProps) {
  const { t } = useTranslation();
  const [showAdvanced, setShowAdvanced] = React.useState(false);

  const filterOptions: FilterOption<PolicyStatusFilter>[] = React.useMemo(() => [
    { value: 'all', label: t('policies.status.all'), count: counts?.all },
    { value: 'Draft', label: t('policies.status.draft'), count: counts?.draft },
    { value: 'Bound', label: t('policies.status.bound'), count: counts?.bound },
    { value: 'Active', label: t('policies.status.active'), count: counts?.active },
    { value: 'Cancelled', label: t('policies.status.cancelled'), count: counts?.cancelled },
    { value: 'Expired', label: t('policies.status.expired'), count: counts?.expired },
  ], [t, counts]);

  const activeAdvancedCount = Object.values(advancedFilters).filter(Boolean).length;

  const handleClearAdvanced = () => {
    onAdvancedFiltersChange({});
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <QuickFilters
          options={filterOptions}
          value={statusFilter}
          onChange={onStatusFilterChange}
        />
        <div className="flex items-center gap-2">
          <SearchInput
            value={search}
            onSearch={onSearchChange}
            placeholder={t('policies.searchPlaceholder')}
            className="w-full sm:w-64"
          />
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="whitespace-nowrap"
          >
            {t('policies.filters.advanced')}
            {activeAdvancedCount > 0 && (
              <Badge variant="secondary" className="ml-2">
                {activeAdvancedCount}
              </Badge>
            )}
            <ChevronDown
              className={`ml-1 h-4 w-4 transition-transform ${
                showAdvanced ? 'rotate-180' : ''
              }`}
            />
          </Button>
        </div>
      </div>

      {showAdvanced && (
        <div className="rounded-lg border bg-muted/30 p-4">
          <div className="flex items-center justify-between mb-4">
            <h4 className="font-medium">{t('policies.filters.advanced')}</h4>
            {activeAdvancedCount > 0 && (
              <Button variant="ghost" size="sm" onClick={handleClearAdvanced}>
                <X className="mr-1 h-4 w-4" />
                Clear All
              </Button>
            )}
          </div>

          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div className="space-y-2">
              <Label htmlFor="effectiveFrom">{t('policies.filters.effectiveFrom')}</Label>
              <Input
                id="effectiveFrom"
                type="date"
                value={advancedFilters.effectiveDateFrom || ''}
                onChange={(e) =>
                  onAdvancedFiltersChange({
                    ...advancedFilters,
                    effectiveDateFrom: e.target.value || undefined,
                  })
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="effectiveTo">{t('policies.filters.effectiveTo')}</Label>
              <Input
                id="effectiveTo"
                type="date"
                value={advancedFilters.effectiveDateTo || ''}
                onChange={(e) =>
                  onAdvancedFiltersChange({
                    ...advancedFilters,
                    effectiveDateTo: e.target.value || undefined,
                  })
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="expirationFrom">Expiration From</Label>
              <Input
                id="expirationFrom"
                type="date"
                value={advancedFilters.expirationDateFrom || ''}
                onChange={(e) =>
                  onAdvancedFiltersChange({
                    ...advancedFilters,
                    expirationDateFrom: e.target.value || undefined,
                  })
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="expirationTo">Expiration To</Label>
              <Input
                id="expirationTo"
                type="date"
                value={advancedFilters.expirationDateTo || ''}
                onChange={(e) =>
                  onAdvancedFiltersChange({
                    ...advancedFilters,
                    expirationDateTo: e.target.value || undefined,
                  })
                }
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
