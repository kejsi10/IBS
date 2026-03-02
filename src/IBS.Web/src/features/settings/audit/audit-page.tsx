import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { ChevronDown, ChevronRight, Filter } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableEmpty,
  TableHead,
  TableHeader,
  TableLoading,
  TableRow,
} from '@/components/ui/table';
import { Pagination, PaginationInfo } from '@/components/ui/pagination';
import { useAuditLogs } from '@/hooks/use-audit';
import type { AuditFilters, AuditLogEntry } from '@/types/audit';
import { formatDate } from '@/lib/format';
import { cn } from '@/lib/utils';

/** Action filter values available in the dropdown */
type ActionFilter = 'all' | 'Create' | 'Update' | 'Delete';

const PAGE_SIZE = 20;

/**
 * Maps an audit action to its badge variant.
 */
function actionVariant(action: AuditLogEntry['action']) {
  switch (action) {
    case 'Create':
      return 'success' as const;
    case 'Update':
      return 'warning' as const;
    case 'Delete':
      return 'error' as const;
    default:
      return 'secondary' as const;
  }
}

/**
 * Collapsible JSON viewer for the audit log changes field.
 */
function ChangesCell({ changes }: { changes: string | null }) {
  const [expanded, setExpanded] = React.useState(false);
  const { t } = useTranslation();

  if (!changes) {
    return <span className="text-muted-foreground text-xs">{t('common.none', '—')}</span>;
  }

  let formatted: string;
  try {
    formatted = JSON.stringify(JSON.parse(changes), null, 2);
  } catch {
    formatted = changes;
  }

  return (
    <div className="max-w-xs">
      <button
        type="button"
        onClick={(e) => {
          e.stopPropagation();
          setExpanded((prev) => !prev);
        }}
        className="inline-flex items-center gap-1 text-xs text-primary hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        aria-expanded={expanded}
      >
        {expanded ? (
          <ChevronDown className="h-3 w-3 shrink-0" />
        ) : (
          <ChevronRight className="h-3 w-3 shrink-0" />
        )}
        {expanded
          ? t('settings.audit.changes.hide', 'Hide')
          : t('settings.audit.changes.show', 'Show')}
      </button>
      {expanded && (
        <pre className="mt-1 max-h-48 overflow-auto rounded-md border bg-muted/50 p-2 text-[11px] leading-relaxed">
          {formatted}
        </pre>
      )}
    </div>
  );
}

/**
 * Audit log page displaying a filterable, paginated table of audit events.
 */
export function AuditPage() {
  const { t, i18n } = useTranslation();

  // Filter state
  const [from, setFrom] = React.useState('');
  const [to, setTo] = React.useState('');
  const [entityType, setEntityType] = React.useState('');
  const [action, setAction] = React.useState<ActionFilter>('all');
  const [page, setPage] = React.useState(1);

  // Build filter params; reset to page 1 when filters change
  const filterParams: AuditFilters = React.useMemo(() => {
    return {
      page,
      pageSize: PAGE_SIZE,
      from: from || undefined,
      to: to || undefined,
      entityType: entityType || undefined,
      action: action === 'all' ? undefined : action,
    };
  }, [page, from, to, entityType, action]);

  const { data, isLoading, error } = useAuditLogs(filterParams);

  // Reset page when filters change (excluding page itself)
  const resetPage = React.useCallback(() => setPage(1), []);
  React.useEffect(resetPage, [from, to, entityType, action, resetPage]);

  const handleActionChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setAction(e.target.value as ActionFilter);
  };

  const totalPages = data?.totalPages ?? 1;
  const totalCount = data?.totalCount ?? 0;
  const items = data?.items ?? [];
  const currentPage = data?.page ?? page;

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div>
        <h2 className="text-xl font-semibold">{t('settings.audit.title')}</h2>
        <p className="text-sm text-muted-foreground">
          {t('settings.audit.description', 'Review all recorded system actions.')}
        </p>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-wrap items-end gap-4">
            <div className="flex items-center gap-1.5 text-sm font-medium text-muted-foreground">
              <Filter className="h-4 w-4" />
              {t('settings.audit.filters.label', 'Filters')}
            </div>

            {/* Date from */}
            <div className="flex flex-col gap-1">
              <label htmlFor="audit-from" className="text-xs font-medium text-muted-foreground">
                {t('settings.audit.filters.from', 'From')}
              </label>
              <Input
                id="audit-from"
                type="date"
                value={from}
                onChange={(e) => setFrom(e.target.value)}
                className="h-9 w-40 text-sm"
              />
            </div>

            {/* Date to */}
            <div className="flex flex-col gap-1">
              <label htmlFor="audit-to" className="text-xs font-medium text-muted-foreground">
                {t('settings.audit.filters.to', 'To')}
              </label>
              <Input
                id="audit-to"
                type="date"
                value={to}
                onChange={(e) => setTo(e.target.value)}
                className="h-9 w-40 text-sm"
              />
            </div>

            {/* Entity type */}
            <div className="flex flex-col gap-1">
              <label htmlFor="audit-entity-type" className="text-xs font-medium text-muted-foreground">
                {t('settings.audit.filters.entityType', 'Entity Type')}
              </label>
              <Input
                id="audit-entity-type"
                type="text"
                value={entityType}
                onChange={(e) => setEntityType(e.target.value)}
                placeholder={t('settings.audit.filters.entityTypePlaceholder', 'e.g. Policy')}
                className="h-9 w-48 text-sm"
              />
            </div>

            {/* Action */}
            <div className="flex flex-col gap-1">
              <label htmlFor="audit-action" className="text-xs font-medium text-muted-foreground">
                {t('settings.audit.filters.action', 'Action')}
              </label>
              <select
                id="audit-action"
                value={action}
                onChange={handleActionChange}
                className={cn(
                  'h-9 w-36 rounded-md border border-input bg-background px-3 text-sm ring-offset-background',
                  'focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2'
                )}
              >
                <option value="all">{t('settings.audit.actions.all', 'All')}</option>
                <option value="Create">{t('settings.audit.actions.create', 'Create')}</option>
                <option value="Update">{t('settings.audit.actions.update', 'Update')}</option>
                <option value="Delete">{t('settings.audit.actions.delete', 'Delete')}</option>
              </select>
            </div>

            {/* Clear button */}
            {(from || to || entityType || action !== 'all') && (
              <Button
                variant="ghost"
                size="sm"
                className="self-end"
                onClick={() => {
                  setFrom('');
                  setTo('');
                  setEntityType('');
                  setAction('all');
                }}
              >
                {t('common.clearFilters', 'Clear filters')}
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Error state */}
      {error && (
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {t('settings.audit.loadError', 'Failed to load audit log')}
              {error instanceof Error ? `: ${error.message}` : ''}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Table */}
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t('settings.audit.columns.action', 'Action')}</TableHead>
              <TableHead>{t('settings.audit.columns.entityType', 'Entity Type')}</TableHead>
              <TableHead>{t('settings.audit.columns.entityId', 'Entity ID')}</TableHead>
              <TableHead>{t('settings.audit.columns.userEmail', 'User')}</TableHead>
              <TableHead>{t('settings.audit.columns.timestamp', 'Timestamp')}</TableHead>
              <TableHead>{t('settings.audit.columns.changes', 'Changes')}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableLoading colSpan={6} rows={PAGE_SIZE} />
            ) : items.length === 0 ? (
              <TableEmpty message={t('settings.audit.noEntries', 'No audit log entries found.')} colSpan={6} />
            ) : (
              items.map((entry) => (
                <TableRow key={entry.id}>
                  {/* Action */}
                  <TableCell>
                    <Badge variant={actionVariant(entry.action)}>
                      {t(`settings.audit.actions.${entry.action.toLowerCase()}`, entry.action)}
                    </Badge>
                  </TableCell>

                  {/* Entity type */}
                  <TableCell className="font-medium">{entry.entityType}</TableCell>

                  {/* Entity ID */}
                  <TableCell>
                    <span className="font-mono text-xs text-muted-foreground">{entry.entityId}</span>
                  </TableCell>

                  {/* User email */}
                  <TableCell>
                    {entry.userEmail ? (
                      entry.userEmail
                    ) : (
                      <span className="text-muted-foreground text-xs">
                        {t('settings.audit.systemUser', 'System')}
                      </span>
                    )}
                  </TableCell>

                  {/* Timestamp */}
                  <TableCell className="whitespace-nowrap text-sm text-muted-foreground">
                    {formatDate(entry.timestamp, i18n.language, {
                      year: 'numeric',
                      month: 'short',
                      day: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </TableCell>

                  {/* Changes — collapsible JSON */}
                  <TableCell>
                    <ChangesCell changes={entry.changes} />
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {!isLoading && totalCount > 0 && (
        <div className="flex flex-col items-center gap-4 sm:flex-row sm:justify-between">
          <PaginationInfo
            currentPage={currentPage}
            pageSize={PAGE_SIZE}
            totalCount={totalCount}
          />
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={setPage}
          />
        </div>
      )}
    </div>
  );
}
