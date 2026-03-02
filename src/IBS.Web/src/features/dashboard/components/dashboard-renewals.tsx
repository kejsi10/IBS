import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Calendar, ChevronRight } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { usePoliciesDueForRenewal } from '@/hooks/use-policies';
import { formatDate } from '@/lib/format';

/**
 * Calculates days until expiration.
 */
function getDaysUntil(dateString: string): number {
  const now = new Date();
  const date = new Date(dateString);
  return Math.ceil((date.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
}

/**
 * Dashboard upcoming renewals panel.
 */
export function DashboardRenewals() {
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const { data: policies, isLoading } = usePoliciesDueForRenewal(60);

  const upcomingRenewals = policies?.items?.slice(0, 5) ?? [];

  return (
    <Card className="h-full">
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>{t('dashboard.renewals.title')}</CardTitle>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => navigate('/policies?status=Active')}
        >
          {t('dashboard.renewals.viewAll')}
          <ChevronRight className="ml-1 h-4 w-4" />
        </Button>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="space-y-3">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-12 animate-pulse rounded bg-muted" />
            ))}
          </div>
        ) : upcomingRenewals.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-8 text-muted-foreground">
            <Calendar className="h-8 w-8 mb-2" />
            <p className="text-sm">{t('dashboard.renewals.noRenewals')}</p>
          </div>
        ) : (
          <div className="space-y-3">
            {upcomingRenewals.map((policy) => {
              const daysUntil = getDaysUntil(policy.expirationDate);
              const isUrgent = daysUntil <= 14;

              return (
                <button
                  key={policy.id}
                  type="button"
                  onClick={() => navigate(`/policies/${policy.id}`)}
                  className="flex w-full items-center justify-between rounded-lg border p-3 text-left transition-colors hover:bg-muted/50"
                >
                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium truncate">
                      {policy.policyNumber}
                    </p>
                    <p className="text-xs text-muted-foreground truncate">
                      {policy.clientName}
                    </p>
                  </div>
                  <div className="flex items-center gap-2 ml-2">
                    <Badge variant={isUrgent ? 'error' : 'secondary'}>
                      {daysUntil}d
                    </Badge>
                    <span className="text-xs text-muted-foreground whitespace-nowrap">
                      {formatDate(policy.expirationDate, i18n.language)}
                    </span>
                  </div>
                </button>
              );
            })}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
