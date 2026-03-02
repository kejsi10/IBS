import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { PolicyStatusFlow } from '../policy-status-flow';
import type { Policy, PolicyStatus } from '@/types/api';

/**
 * Props for the PolicyOverviewTab component.
 */
export interface PolicyOverviewTabProps {
  policy: Policy;
}

/**
 * Formats line of business for display.
 */
function formatLob(lob: string): string {
  return lob.replace(/([A-Z])/g, ' $1').trim();
}

/**
 * Calculates days until expiration.
 */
function getDaysUntilExpiration(expirationDate: string): number {
  const now = new Date();
  const expiry = new Date(expirationDate);
  return Math.ceil((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
}

/**
 * Overview tab for policy detail page.
 */
export function PolicyOverviewTab({ policy }: PolicyOverviewTabProps) {
  const { t } = useTranslation();
  const daysUntilExpiration = getDaysUntilExpiration(policy.expirationDate);
  const totalPremium = policy.coverages.reduce((sum, c) => sum + c.premiumAmount, 0);

  return (
    <div className="space-y-6">
      {/* Status Flow */}
      <Card>
        <CardHeader>
          <CardTitle>{t('policies.detail.overview.policyStatus')}</CardTitle>
        </CardHeader>
        <CardContent>
          <PolicyStatusFlow status={policy.status as PolicyStatus} />
        </CardContent>
      </Card>

      {/* Key Metrics */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('policies.detail.overview.totalPremium')}</p>
            <p className="text-2xl font-bold">
              {new Intl.NumberFormat('en-US', {
                style: 'currency',
                currency: policy.currency || 'USD',
              }).format(policy.totalPremium)}
            </p>
            <p className="text-xs text-muted-foreground">
              {t('policies.detail.overview.paymentPlanPayments', { plan: policy.paymentPlan })}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('policies.detail.overview.policyTerm')}</p>
            <p className="text-lg font-medium">
              {new Date(policy.effectiveDate).toLocaleDateString()} -{' '}
              {new Date(policy.expirationDate).toLocaleDateString()}
            </p>
            {policy.status === 'Active' && (
              <Badge
                variant={daysUntilExpiration <= 30 ? 'error' : 'secondary'}
                className="mt-1"
              >
                {daysUntilExpiration > 0
                  ? t('policies.detail.overview.daysRemaining', { days: daysUntilExpiration })
                  : t('policies.detail.overview.expired')}
              </Badge>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('policies.detail.overview.coveragesTitle')}</p>
            <p className="text-2xl font-bold">{policy.coverages.length}</p>
            <p className="text-xs text-muted-foreground">
              {t('policies.detail.overview.activeCoverageItems')}
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Policy Details */}
      <Card>
        <CardHeader>
          <CardTitle>{t('policies.detail.overview.policyDetails')}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            <div>
              <p className="text-sm text-muted-foreground">{t('policies.detail.overview.policyNumber')}</p>
              <p className="font-mono font-medium">{policy.policyNumber}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('policies.detail.overview.carrierPolicyNumber')}</p>
              <p className="font-mono">{policy.carrierPolicyNumber || '—'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('policies.detail.lineOfBusiness')}</p>
              <Badge variant="outline">{formatLob(policy.lineOfBusiness)}</Badge>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('policies.detail.overview.policyType')}</p>
              <p className="font-medium">{policy.policyType}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('policies.detail.overview.billingType')}</p>
              <p className="font-medium">
                {policy.billingType === 'DirectBill' ? t('policies.detail.overview.directBill') : t('policies.detail.overview.agencyBill')}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('policies.detail.overview.paymentPlan')}</p>
              <p className="font-medium">{policy.paymentPlan}</p>
            </div>
          </div>

          {policy.notes && (
            <div className="mt-6 border-t pt-6">
              <p className="text-sm text-muted-foreground">{t('policies.detail.overview.notes')}</p>
              <p className="mt-1">{policy.notes}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Premium Breakdown */}
      <Card>
        <CardHeader>
          <CardTitle>{t('policies.detail.overview.premiumBreakdown')}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            {policy.coverages.map((coverage) => (
              <div
                key={coverage.id}
                className="flex items-center justify-between rounded-lg border p-3"
              >
                <div>
                  <span className="font-medium">{coverage.name}</span>
                  <span className="ml-2 font-mono text-xs text-muted-foreground">
                    {coverage.code}
                  </span>
                </div>
                <span>
                  {new Intl.NumberFormat('en-US', {
                    style: 'currency',
                    currency: policy.currency || 'USD',
                  }).format(coverage.premiumAmount)}
                </span>
              </div>
            ))}
            <div className="flex items-center justify-between border-t pt-3">
              <span className="font-bold">{t('policies.detail.overview.totalPremiumLabel')}</span>
              <span className="text-xl font-bold">
                {new Intl.NumberFormat('en-US', {
                  style: 'currency',
                  currency: policy.currency || 'USD',
                }).format(totalPremium)}
              </span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
