import * as React from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, FileText, Play, CheckCircle, RefreshCw, PlusCircle } from 'lucide-react';
import { getErrorMessage } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/toast';
import { PolicyOverviewTab } from './components/tabs/policy-overview';
import { PolicyCoveragesTab } from './components/tabs/policy-coverages';
import { PolicyEndorsementsTab } from './components/tabs/policy-endorsements';
import {
  usePolicy,
  useBindPolicy,
  useActivatePolicy,
  useRenewPolicy,
} from '@/hooks/use-policies';
import type { PolicyStatus } from '@/types/api';
import type { BadgeVariant } from '@/components/ui/badge';

/**
 * Status badge variants.
 */
const statusVariants: Record<string, BadgeVariant> = {
  Active: 'success',
  Draft: 'secondary',
  Bound: 'secondary',
  Cancelled: 'error',
  Expired: 'outline',
  Renewed: 'outline',
  PendingRenewal: 'warning',
  NonRenewed: 'outline',
};

/**
 * Policy detail page with tabbed interface.
 */
export function PolicyDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const { data: policy, isLoading, error } = usePolicy(id!);
  const [activeTab, setActiveTab] = React.useState('overview');

  const bindPolicy = useBindPolicy();
  const activatePolicy = useActivatePolicy();
  const renewPolicy = useRenewPolicy();

  const handleBind = async () => {
    try {
      await bindPolicy.mutateAsync(id!);
      addToast({
        title: t('policies.actions.bind'),
        description: t('policies.toast.bound', { policyNumber: policy?.policyNumber }),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: getErrorMessage(err),
        variant: 'error',
      });
    }
  };

  const handleActivate = async () => {
    try {
      await activatePolicy.mutateAsync(id!);
      addToast({
        title: t('policies.actions.activate'),
        description: t('policies.toast.activated', { policyNumber: policy?.policyNumber }),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: getErrorMessage(err),
        variant: 'error',
      });
    }
  };

  const handleRenew = async () => {
    try {
      await renewPolicy.mutateAsync(id!);
      addToast({
        title: t('policies.actions.renew'),
        description: t('policies.toast.renewed', { policyNumber: policy?.policyNumber }),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: getErrorMessage(err),
        variant: 'error',
      });
    }
  };

  if (isLoading) {
    return <PolicyDetailSkeleton />;
  }

  if (error || !policy) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate('/policies')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('policies.detail.backToPolicies')}
        </Button>
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {error instanceof Error ? error.message : t('policies.detail.notFound')}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const status = policy.status as PolicyStatus;
  const hasActiveCoverages = policy.coverages.some((c) => c.isActive);

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <Button variant="ghost" onClick={() => navigate('/policies')}>
        <ArrowLeft className="mr-2 h-4 w-4" />
        {t('policies.detail.backToPolicies')}
      </Button>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary/10">
            <FileText className="h-6 w-6 text-primary" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold font-mono">{policy.policyNumber}</h1>
              <Badge variant={statusVariants[status] || 'secondary'}>
                {t(`policies.detail.status.${status.charAt(0).toLowerCase() + status.slice(1)}`, status)}
              </Badge>
            </div>
            <div className="flex items-center gap-2 text-muted-foreground">
              <Link to={`/clients/${policy.clientId}`} className="hover:underline">
                {policy.clientName}
              </Link>
              <span>•</span>
              <Link to={`/carriers/${policy.carrierId}`} className="hover:underline">
                {policy.carrierName}
              </Link>
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="flex gap-2">
          {status === 'Draft' && (
            <div className="flex flex-col items-end gap-1">
              <Button onClick={handleBind} disabled={bindPolicy.isPending || !hasActiveCoverages}>
                <CheckCircle className="mr-2 h-4 w-4" />
                {t('policies.detail.bind')}
              </Button>
              {!hasActiveCoverages && (
                <p className="text-xs text-muted-foreground">
                  {t('policies.detail.bindHint')}{' '}
                  <button
                    type="button"
                    className="underline hover:text-foreground"
                    onClick={() => setActiveTab('coverages')}
                  >
                    {t('policies.detail.bindHintAction')}
                  </button>
                </p>
              )}
            </div>
          )}
          {status === 'Bound' && (
            <Button onClick={handleActivate} disabled={activatePolicy.isPending}>
              <Play className="mr-2 h-4 w-4" />
              {t('policies.detail.activate')}
            </Button>
          )}
          {status === 'Active' && (
            <>
              <Button variant="outline" onClick={handleRenew} disabled={renewPolicy.isPending}>
                <RefreshCw className="mr-2 h-4 w-4" />
                {t('policies.detail.renew')}
              </Button>
              <Button
                variant="outline"
                onClick={() => navigate(`/policies/${id}?tab=endorsements&add=true`)}
              >
                <PlusCircle className="mr-2 h-4 w-4" />
                {t('policies.detail.addEndorsement')}
              </Button>
            </>
          )}
        </div>
      </div>

      {/* Quick Info Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('policies.detail.totalPremium')}</p>
            <p className="text-xl font-bold">
              {new Intl.NumberFormat('en-US', {
                style: 'currency',
                currency: policy.currency || 'USD',
              }).format(policy.totalPremium)}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('policies.detail.effectiveDate')}</p>
            <p className="font-medium">
              {new Date(policy.effectiveDate).toLocaleDateString()}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('policies.detail.expirationDate')}</p>
            <p className="font-medium">
              {new Date(policy.expirationDate).toLocaleDateString()}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('policies.detail.lineOfBusiness')}</p>
            <p className="font-medium">
              {policy.lineOfBusiness.replace(/([A-Z])/g, ' $1').trim()}
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="overview">{t('policies.detail.tabs.overview')}</TabsTrigger>
          <TabsTrigger value="coverages">
            {t('policies.detail.tabs.coverages')} ({policy.coverages.length})
          </TabsTrigger>
          <TabsTrigger value="endorsements">
            {t('policies.detail.tabs.endorsements')} ({policy.endorsements.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="mt-6">
          <PolicyOverviewTab policy={policy} />
        </TabsContent>

        <TabsContent value="coverages" className="mt-6">
          <PolicyCoveragesTab policy={policy} />
        </TabsContent>

        <TabsContent value="endorsements" className="mt-6">
          <PolicyEndorsementsTab policy={policy} />
        </TabsContent>
      </Tabs>
    </div>
  );
}

/**
 * Loading skeleton for policy detail page.
 */
function PolicyDetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-9 w-32" />
      <div className="flex items-center gap-4">
        <Skeleton className="h-12 w-12 rounded-full" />
        <div>
          <Skeleton className="h-8 w-48" />
          <Skeleton className="mt-1 h-4 w-32" />
        </div>
      </div>
      <div className="grid gap-4 md:grid-cols-4">
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
      </div>
      <Skeleton className="h-10 w-96" />
      <Skeleton className="h-64" />
    </div>
  );
}
