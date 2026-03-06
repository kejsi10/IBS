import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { usePolicyHistory } from '@/hooks/use-policies';
import {
  CheckCircle,
  XCircle,
  RefreshCw,
  FileText,
  Shield,
  AlertCircle,
  TrendingUp,
  Plus,
  Minus,
  ChevronDown,
  ChevronUp,
  RotateCcw,
} from 'lucide-react';
import type { PolicyHistoryEventType } from '@/types/api';

/**
 * Props for PolicyHistoryTab.
 */
interface PolicyHistoryTabProps {
  policyId: string;
}

/**
 * Icon and color config per event type.
 */
const eventConfig: Record<
  string,
  { icon: React.ComponentType<{ className?: string }>; variant: string; label: string }
> = {
  Created: { icon: FileText, variant: 'secondary', label: 'Created' },
  Bound: { icon: Shield, variant: 'secondary', label: 'Bound' },
  Activated: { icon: CheckCircle, variant: 'success', label: 'Activated' },
  Cancelled: { icon: XCircle, variant: 'error', label: 'Cancelled' },
  Reinstated: { icon: RotateCcw, variant: 'success', label: 'Reinstated' },
  Renewed: { icon: RefreshCw, variant: 'secondary', label: 'Renewed' },
  Expired: { icon: AlertCircle, variant: 'outline', label: 'Expired' },
  NonRenewed: { icon: AlertCircle, variant: 'outline', label: 'Non-Renewed' },
  CoverageAdded: { icon: Plus, variant: 'success', label: 'Coverage Added' },
  CoverageModified: { icon: TrendingUp, variant: 'warning', label: 'Coverage Modified' },
  CoverageRemoved: { icon: Minus, variant: 'error', label: 'Coverage Removed' },
  EndorsementAdded: { icon: Plus, variant: 'secondary', label: 'Endorsement Added' },
  EndorsementApproved: { icon: CheckCircle, variant: 'success', label: 'Endorsement Approved' },
  EndorsementIssued: { icon: Shield, variant: 'success', label: 'Endorsement Issued' },
  EndorsementRejected: { icon: XCircle, variant: 'error', label: 'Endorsement Rejected' },
  PremiumChanged: { icon: TrendingUp, variant: 'warning', label: 'Premium Changed' },
};

/**
 * History tab displaying a timeline of all policy events.
 */
export function PolicyHistoryTab({ policyId }: PolicyHistoryTabProps) {
  const { t } = useTranslation();
  const [page, setPage] = React.useState(1);
  const { data, isLoading } = usePolicyHistory(policyId, page);

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-16 w-full" />
        ))}
      </div>
    );
  }

  if (!data || data.items.length === 0) {
    return (
      <Card>
        <CardContent className="py-12 text-center">
          <p className="text-muted-foreground">{t('policies.history.empty')}</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      <div className="relative">
        {/* Vertical line */}
        <div className="absolute left-6 top-0 bottom-0 w-px bg-border" />

        <div className="space-y-4">
          {data.items.map((entry) => (
            <HistoryEntry key={entry.id} entry={entry} />
          ))}
        </div>
      </div>

      {/* Pagination */}
      {data.totalPages > 1 && (
        <div className="flex items-center justify-between pt-4">
          <p className="text-sm text-muted-foreground">
            {t('policies.history.page', { page: data.pageNumber, total: data.totalPages })}
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage((p) => p - 1)}
            >
              {t('common.previous')}
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={page >= data.totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              {t('common.next')}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}

/**
 * A single history entry in the timeline.
 */
function HistoryEntry({
  entry,
}: {
  entry: {
    id: string;
    eventType: PolicyHistoryEventType;
    description: string;
    changesJson?: string;
    timestamp: string;
  };
}) {
  const [expanded, setExpanded] = React.useState(false);
  const config = eventConfig[entry.eventType] ?? {
    icon: FileText,
    variant: 'secondary',
    label: entry.eventType,
  };
  const Icon = config.icon;

  return (
    <div className="flex gap-4 pl-2">
      {/* Icon bubble */}
      <div className="relative flex h-9 w-9 shrink-0 items-center justify-center rounded-full border bg-background z-10">
        <Icon className="h-4 w-4" />
      </div>

      {/* Content */}
      <div className="flex-1 rounded-lg border p-3">
        <div className="flex items-start justify-between gap-2">
          <div className="flex items-center gap-2 flex-wrap">
            <Badge variant={config.variant as 'success' | 'error' | 'warning' | 'secondary' | 'outline'}>
              {config.label}
            </Badge>
            <p className="text-sm">{entry.description}</p>
          </div>
          <div className="flex items-center gap-2 shrink-0">
            <p className="text-xs text-muted-foreground whitespace-nowrap">
              {new Date(entry.timestamp).toLocaleString()}
            </p>
            {entry.changesJson && (
              <button
                type="button"
                className="text-muted-foreground hover:text-foreground"
                onClick={() => setExpanded((e) => !e)}
                aria-label="Toggle change details"
              >
                {expanded ? (
                  <ChevronUp className="h-4 w-4" />
                ) : (
                  <ChevronDown className="h-4 w-4" />
                )}
              </button>
            )}
          </div>
        </div>

        {/* Expandable JSON diff */}
        {expanded && entry.changesJson && (
          <pre className="mt-2 overflow-x-auto rounded bg-muted p-2 text-xs">
            {JSON.stringify(JSON.parse(entry.changesJson), null, 2)}
          </pre>
        )}
      </div>
    </div>
  );
}
