import * as React from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Plus, Search } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { NativeSelect } from '@/components/ui/select';
import { Pagination, PaginationInfo } from '@/components/ui/pagination';
import { Skeleton } from '@/components/ui/skeleton';
import { useClaims } from '@/hooks';
import { useDebounce } from '@/hooks';
import { formatDate, formatCurrency } from '@/lib/format';
import type { ClaimStatus, ClaimFilters, ClaimListItem } from '@/types/api';
import type { BadgeVariant } from '@/components/ui/badge';

/**
 * Maps claim status values to badge variants for display.
 */
const statusVariants: Record<string, BadgeVariant> = {
  FNOL: 'default',
  Acknowledged: 'secondary',
  Assigned: 'secondary',
  UnderInvestigation: 'warning',
  Evaluation: 'warning',
  Approved: 'success',
  Denied: 'error',
  Settlement: 'default',
  Closed: 'outline',
};

/**
 * Formats a status string for display.
 */
function formatStatus(status: string): string {
  return status.replace(/([A-Z])/g, ' $1').trim();
}

/**
 * Claims list page with search, status filter, table, and pagination.
 */
export function ClaimsPage() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const [search, setSearch] = React.useState('');
  const [status, setStatus] = React.useState<string>('');
  const [page, setPage] = React.useState(1);
  const [pageSize] = React.useState(20);

  const debouncedSearch = useDebounce(search, 300);

  /**
   * Status filter options for the claims list.
   */
  const STATUS_OPTIONS = React.useMemo(() => [
    { value: '', label: t('claims.status.all') },
    { value: 'FNOL', label: t('claims.status.fnol') },
    { value: 'Acknowledged', label: t('claims.status.acknowledged') },
    { value: 'Assigned', label: t('claims.status.assigned') },
    { value: 'UnderInvestigation', label: t('claims.status.underInvestigation') },
    { value: 'Evaluation', label: t('claims.status.evaluation') },
    { value: 'Approved', label: t('claims.status.approved') },
    { value: 'Denied', label: t('claims.status.denied') },
    { value: 'Settlement', label: t('claims.status.settlement') },
    { value: 'Closed', label: t('claims.status.closed') },
  ], [t]);

  const filters: ClaimFilters = React.useMemo(() => ({
    search: debouncedSearch || undefined,
    status: (status || undefined) as ClaimStatus | undefined,
    page,
    pageSize,
  }), [debouncedSearch, status, page, pageSize]);

  const { data, isLoading, error } = useClaims(filters);

  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, status]);

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{t('claims.title')}</h1>
          <p className="text-muted-foreground">
            {t('claims.subtitle')}
          </p>
        </div>
        <Button onClick={() => navigate('/claims/new')}>
          <Plus className="mr-2 h-4 w-4" />
          {t('claims.newClaim')}
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder={t('claims.searchPlaceholder')}
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="pl-9"
              />
            </div>
            <NativeSelect
              options={STATUS_OPTIONS}
              value={status}
              onChange={(e) => setStatus(e.target.value)}
              className="w-full sm:w-48"
              aria-label="Filter by status"
            />
          </div>
        </CardContent>
      </Card>

      {/* Error State */}
      {error && (
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {t('claims.loadError')}{error instanceof Error ? `: ${error.message}` : ''}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Claims Table */}
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b bg-muted/50">
                  <th className="px-4 py-3 text-left font-medium">{t('claims.columns.claimNumber')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('claims.columns.client')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('claims.columns.status')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('claims.columns.lossType')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('claims.columns.lossDate')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('claims.columns.estimatedLoss')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('claims.columns.approvedAmount')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('claims.columns.filed')}</th>
                </tr>
              </thead>
              <tbody>
                {isLoading && <ClaimsTableSkeleton />}
                {!isLoading && data?.items.length === 0 && (
                  <tr>
                    <td colSpan={8} className="px-4 py-12 text-center text-muted-foreground">
                      {t('claims.noClaims')}
                    </td>
                  </tr>
                )}
                {!isLoading && data?.items.map((claim) => (
                  <ClaimRow key={claim.id} claim={claim} lng={i18n.language} />
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <PaginationInfo
            currentPage={data.page}
            pageSize={data.pageSize}
            totalCount={data.totalCount}
          />
          <Pagination
            currentPage={data.page}
            totalPages={data.totalPages}
            onPageChange={setPage}
          />
        </div>
      )}
    </div>
  );
}

/**
 * Props for the ClaimRow component.
 */
interface ClaimRowProps {
  claim: ClaimListItem;
  lng: string;
}

/** Maps API status strings to i18n sub-keys under claims.status. */
const STATUS_KEYS: Record<string, string> = {
  FNOL: 'fnol',
  Acknowledged: 'acknowledged',
  Assigned: 'assigned',
  UnderInvestigation: 'underInvestigation',
  Evaluation: 'evaluation',
  Approved: 'approved',
  Denied: 'denied',
  Settlement: 'settlement',
  Closed: 'closed',
};

/** Maps API loss type strings to i18n sub-keys under claims.lossType. */
const LOSS_TYPE_KEYS: Record<string, string> = {
  PropertyDamage: 'propertyDamage',
  Auto: 'auto',
  TheftFraud: 'theftFraud',
  Liability: 'liability',
  Workers: 'workers',
  Other: 'other',
};

/**
 * A single row in the claims table.
 */
function ClaimRow({ claim, lng }: ClaimRowProps) {
  const { t } = useTranslation();

  return (
    <tr className="border-b transition-colors hover:bg-muted/50">
      <td className="px-4 py-3">
        <Link
          to={`/claims/${claim.id}`}
          className="font-medium text-primary hover:underline"
        >
          {claim.claimNumber}
        </Link>
      </td>
      <td className="px-4 py-3">{claim.clientName || t('claims.unknownClient')}</td>
      <td className="px-4 py-3">
        <Badge variant={statusVariants[claim.status] || 'secondary'}>
          {t(`claims.status.${STATUS_KEYS[claim.status] ?? claim.status.toLowerCase()}`, { defaultValue: formatStatus(claim.status) })}
        </Badge>
      </td>
      <td className="px-4 py-3">{t(`claims.lossType.${LOSS_TYPE_KEYS[claim.lossType] ?? claim.lossType.toLowerCase()}`, { defaultValue: formatStatus(claim.lossType) })}</td>
      <td className="px-4 py-3">{formatDate(claim.lossDate, lng)}</td>
      <td className="px-4 py-3">
        {claim.lossAmount != null
          ? formatCurrency(claim.lossAmount, 'USD', lng)
          : <span className="text-muted-foreground">--</span>
        }
      </td>
      <td className="px-4 py-3">
        {claim.claimAmount != null
          ? formatCurrency(claim.claimAmount, 'USD', lng)
          : <span className="text-muted-foreground">--</span>
        }
      </td>
      <td className="px-4 py-3 text-muted-foreground">
        {formatDate(claim.createdAt, lng)}
      </td>
    </tr>
  );
}

/**
 * Skeleton rows displayed while the claims table is loading.
 */
function ClaimsTableSkeleton() {
  return (
    <>
      {Array.from({ length: 5 }).map((_, i) => (
        <tr key={i} className="border-b">
          <td className="px-4 py-3"><Skeleton className="h-4 w-28" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-32" /></td>
          <td className="px-4 py-3"><Skeleton className="h-5 w-24 rounded-full" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-24" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-24" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-20" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-20" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-24" /></td>
        </tr>
      ))}
    </>
  );
}
