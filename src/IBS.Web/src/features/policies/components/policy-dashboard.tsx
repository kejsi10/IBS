import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { AlertTriangle, Calendar, DollarSign, FileText, Clock } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { StatCard, StatCardGrid } from '@/components/common/stat-card';
import { usePolicies, usePoliciesDueForRenewal } from '@/hooks/use-policies';

/**
 * Dashboard section for the policies page.
 * Shows key metrics and alerts.
 */
export function PolicyDashboard() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { data: policies } = usePolicies();
  const { data: renewalPolicies } = usePoliciesDueForRenewal(30);

  // Calculate metrics
  const activePolicies = policies?.items?.filter((p) => p.status === 'Active').length ?? 0;
  const expiringCount = renewalPolicies?.items?.length ?? 0;
  const totalPremium = policies?.items?.reduce((sum, p) => sum + (p.totalPremium || 0), 0) ?? 0;

  // Get alerts (policies needing attention)
  const alerts = [
    ...(renewalPolicies?.items?.slice(0, 3).map((p) => ({
      id: p.id,
      type: 'renewal' as const,
      message: t('policies.dashboard.expiresMessage', { date: new Date(p.expirationDate).toLocaleDateString() }),
      policyNumber: p.policyNumber,
    })) ?? []),
    ...(policies?.items
      ?.filter((p) => p.status === 'Draft')
      .slice(0, 2)
      .map((p) => ({
        id: p.id,
        type: 'draft' as const,
        message: t('policies.dashboard.draftMessage'),
        policyNumber: p.policyNumber,
      })) ?? []),
  ];

  return (
    <div className="space-y-6">
      {/* Metrics */}
      <StatCardGrid>
        <StatCard
          title={t('policies.dashboard.activePolicies')}
          value={activePolicies}
          icon={<FileText className="h-5 w-5" />}
          description={t('policies.dashboard.currentlyInForce')}
        />
        <StatCard
          title={t('policies.dashboard.expiringSoon')}
          value={expiringCount}
          icon={<Calendar className="h-5 w-5" />}
          description={t('policies.dashboard.within30Days')}
          trend={expiringCount > 0 ? 'down' : 'neutral'}
          trendValue={expiringCount > 0 ? t('policies.dashboard.needsAttention') : undefined}
          onClick={() => navigate('/policies?status=Active&expiring=30')}
        />
        <StatCard
          title={t('policies.dashboard.renewalsDue')}
          value={renewalPolicies?.items?.length ?? 0}
          icon={<Clock className="h-5 w-5" />}
          description={t('policies.dashboard.pendingRenewal')}
        />
        <StatCard
          title={t('policies.dashboard.monthlyPremium')}
          value={new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
            maximumFractionDigits: 0,
          }).format(totalPremium / 12)}
          icon={<DollarSign className="h-5 w-5" />}
          description={t('policies.dashboard.annualPremium', { amount: totalPremium.toLocaleString() })}
        />
      </StatCardGrid>

      {/* Alerts Panel */}
      {alerts.length > 0 && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between py-3">
            <CardTitle className="flex items-center gap-2 text-base">
              <AlertTriangle className="h-5 w-5 text-yellow-500" />
              {t('policies.dashboard.needsAttentionTitle')}
              <Badge variant="secondary">{alerts.length}</Badge>
            </CardTitle>
            <Button variant="ghost" size="sm">
              {t('policies.dashboard.viewAll')}
            </Button>
          </CardHeader>
          <CardContent className="pt-0">
            <div className="space-y-2">
              {alerts.map((alert) => (
                <button
                  key={alert.id}
                  type="button"
                  onClick={() => navigate(`/policies/${alert.id}`)}
                  className="flex w-full items-center justify-between rounded-lg border p-3 text-left transition-colors hover:bg-muted/50"
                >
                  <div className="flex items-center gap-3">
                    {alert.type === 'renewal' ? (
                      <Calendar className="h-4 w-4 text-yellow-500" />
                    ) : (
                      <FileText className="h-4 w-4 text-muted-foreground" />
                    )}
                    <div>
                      <p className="text-sm font-medium">{alert.policyNumber}</p>
                      <p className="text-xs text-muted-foreground">{alert.message}</p>
                    </div>
                  </div>
                  <Badge variant={alert.type === 'renewal' ? 'error' : 'secondary'}>
                    {alert.type === 'renewal' ? t('policies.dashboard.expiring') : t('policies.status.draft')}
                  </Badge>
                </button>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
