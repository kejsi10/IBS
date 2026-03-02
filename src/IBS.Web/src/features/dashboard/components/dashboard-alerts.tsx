import { useNavigate } from 'react-router-dom';
import { AlertTriangle, Calendar, FileText, ChevronRight } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { formatDate } from '@/lib/format';
import { usePoliciesDueForRenewal, usePolicies } from '@/hooks/use-policies';

/**
 * Dashboard alerts panel showing items needing attention.
 */
export function DashboardAlerts() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const { data: renewalPolicies } = usePoliciesDueForRenewal(30);
  const { data: policies } = usePolicies();

  // Build alerts list
  const alerts = [
    ...(renewalPolicies?.items?.slice(0, 3).map((p) => ({
      id: p.id,
      type: 'renewal' as const,
      title: p.policyNumber,
      message: `Expires ${formatDate(p.expirationDate, i18n.language)}`,
      path: `/policies/${p.id}`,
    })) ?? []),
    ...(policies?.items
      ?.filter((p) => p.status === 'Draft')
      .slice(0, 2)
      .map((p) => ({
        id: p.id,
        type: 'draft' as const,
        title: p.policyNumber,
        message: t('dashboard.alerts.stillInDraft'),
        path: `/policies/${p.id}`,
      })) ?? []),
  ];

  if (alerts.length === 0) {
    return null;
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between py-3">
        <CardTitle className="flex items-center gap-2 text-base">
          <AlertTriangle className="h-5 w-5 text-yellow-500" />
          {t('dashboard.alerts.title')}
          <Badge variant="secondary">{alerts.length}</Badge>
        </CardTitle>
        <Button variant="ghost" size="sm" onClick={() => navigate('/policies')}>
          {t('dashboard.alerts.viewAll')}
          <ChevronRight className="ml-1 h-4 w-4" />
        </Button>
      </CardHeader>
      <CardContent className="pt-0">
        <div className="space-y-2">
          {alerts.map((alert) => (
            <button
              key={alert.id}
              type="button"
              onClick={() => navigate(alert.path)}
              className="flex w-full items-center justify-between rounded-lg border p-3 text-left transition-colors hover:bg-muted/50"
            >
              <div className="flex items-center gap-3">
                {alert.type === 'renewal' ? (
                  <Calendar className="h-4 w-4 text-yellow-500" />
                ) : (
                  <FileText className="h-4 w-4 text-muted-foreground" />
                )}
                <div>
                  <p className="text-sm font-medium">{alert.title}</p>
                  <p className="text-xs text-muted-foreground">{alert.message}</p>
                </div>
              </div>
              <Badge variant={alert.type === 'renewal' ? 'error' : 'secondary'}>
                {alert.type === 'renewal' ? t('dashboard.alerts.expiring') : t('dashboard.alerts.draft')}
              </Badge>
            </button>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
