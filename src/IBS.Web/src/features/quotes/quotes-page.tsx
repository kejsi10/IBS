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
import { formatCurrency, formatDate } from '@/lib/format';
import { useQuotes } from '@/hooks';
import { useDebounce } from '@/hooks';
import type { QuoteStatus, QuoteFilters, QuoteSummary } from '@/types/api';
import type { BadgeVariant } from '@/components/ui/badge';

/**
 * Maps quote status values to badge variants for display.
 */
const statusVariants: Record<string, BadgeVariant> = {
  Draft: 'secondary',
  Submitted: 'default',
  Quoted: 'warning',
  Accepted: 'success',
  Expired: 'error',
  Cancelled: 'outline',
};

/**
 * Quote list page with search, status filter, table, and pagination.
 */
export function QuotesPage() {
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const [search, setSearch] = React.useState('');
  const [status, setStatus] = React.useState<string>('');
  const [page, setPage] = React.useState(1);
  const [pageSize] = React.useState(20);

  const debouncedSearch = useDebounce(search, 300);

  /** Status filter options — depends on t so must live inside component. */
  const STATUS_OPTIONS = React.useMemo(() => [
    { value: '', label: t('quotes.status.all') },
    { value: 'Draft', label: t('quotes.status.draft') },
    { value: 'Submitted', label: t('quotes.status.submitted') },
    { value: 'Quoted', label: t('quotes.status.quoted') },
    { value: 'Accepted', label: t('quotes.status.bound') },
    { value: 'Expired', label: t('quotes.status.expired') },
    { value: 'Cancelled', label: t('quotes.status.declined') },
  ], [t]);

  const filters: QuoteFilters = React.useMemo(() => ({
    search: debouncedSearch || undefined,
    status: (status || undefined) as QuoteStatus | undefined,
    page,
    pageSize,
  }), [debouncedSearch, status, page, pageSize]);

  const { data, isLoading, error } = useQuotes(filters);

  /** Reset to page 1 when filters change. */
  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, status]);

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{t('quotes.title')}</h1>
          <p className="text-muted-foreground">
            {t('quotes.searchPlaceholder')}
          </p>
        </div>
        <Button onClick={() => navigate('/quotes/new')}>
          <Plus className="mr-2 h-4 w-4" />
          {t('quotes.newQuote')}
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder={t('quotes.searchPlaceholder')}
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
              aria-label={t('quotes.status.all')}
            />
          </div>
        </CardContent>
      </Card>

      {/* Error State */}
      {error && (
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              Failed to load quotes: {error instanceof Error ? error.message : 'Unknown error'}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Quotes Table */}
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b bg-muted/50">
                  <th className="px-4 py-3 text-left font-medium">{t('quotes.columns.client')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('quotes.columns.lineOfBusiness')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('quotes.columns.status')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('quotes.columns.effectiveDate')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('quotes.detail.quoteExpires')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('quotes.detail.carriers')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('quotes.detail.premium')}</th>
                  <th className="px-4 py-3 text-left font-medium">{t('quotes.columns.createdAt')}</th>
                </tr>
              </thead>
              <tbody>
                {isLoading && (
                  <QuotesTableSkeleton />
                )}
                {!isLoading && data?.items.length === 0 && (
                  <tr>
                    <td colSpan={8} className="px-4 py-12 text-center text-muted-foreground">
                      {t('quotes.noQuotes')}
                    </td>
                  </tr>
                )}
                {!isLoading && data?.items.map((quote) => (
                  <QuoteRow key={quote.id} quote={quote} lng={i18n.language} />
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
 * Props for the QuoteRow component.
 */
interface QuoteRowProps {
  /** The quote summary to display. */
  quote: QuoteSummary;
  /** The current i18n language code for locale-aware formatting. */
  lng: string;
}

/**
 * A single row in the quotes table.
 */
function QuoteRow({ quote, lng }: QuoteRowProps) {
  return (
    <tr className="border-b transition-colors hover:bg-muted/50">
      <td className="px-4 py-3">
        <Link
          to={`/quotes/${quote.id}`}
          className="font-medium text-primary hover:underline"
        >
          {quote.clientName || 'Unknown Client'}
        </Link>
      </td>
      <td className="px-4 py-3">
        {quote.lineOfBusiness.replace(/([A-Z])/g, ' $1').trim()}
      </td>
      <td className="px-4 py-3">
        <Badge variant={statusVariants[quote.status] || 'secondary'}>
          {quote.status}
        </Badge>
      </td>
      <td className="px-4 py-3">{formatDate(quote.effectiveDate, lng)}</td>
      <td className="px-4 py-3">{formatDate(quote.expiresAt, lng)}</td>
      <td className="px-4 py-3">
        <span className="text-muted-foreground">
          {quote.responseCount}/{quote.carrierCount}
        </span>
      </td>
      <td className="px-4 py-3">
        {quote.lowestPremium != null
          ? formatCurrency(quote.lowestPremium, 'USD', lng)
          : <span className="text-muted-foreground">--</span>
        }
      </td>
      <td className="px-4 py-3 text-muted-foreground">
        {formatDate(quote.createdAt, lng)}
      </td>
    </tr>
  );
}

/**
 * Skeleton rows displayed while the quotes table is loading.
 */
function QuotesTableSkeleton() {
  return (
    <>
      {Array.from({ length: 5 }).map((_, i) => (
        <tr key={i} className="border-b">
          <td className="px-4 py-3"><Skeleton className="h-4 w-32" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-28" /></td>
          <td className="px-4 py-3"><Skeleton className="h-5 w-20 rounded-full" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-24" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-24" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-12" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-20" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-24" /></td>
        </tr>
      ))}
    </>
  );
}
