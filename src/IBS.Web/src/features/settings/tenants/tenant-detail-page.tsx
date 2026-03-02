import * as React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Building2, Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Skeleton } from '@/components/ui/skeleton';
import { InlineEdit } from '@/components/common/inline-edit';
import {
  Table,
  TableBody,
  TableCell,
  TableEmpty,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { useToast } from '@/components/ui/toast';
import {
  useTenant,
  useUpdateTenant,
  useSuspendTenant,
  useActivateTenant,
  useCancelTenant,
  useUpdateSubscription,
  useAddTenantCarrier,
  useRemoveTenantCarrier,
} from '@/hooks/use-tenants';
import { formatDate } from '@/lib/format';
import { cn } from '@/lib/utils';
import type { TenantStatus, SubscriptionTier, TenantCarrier } from '@/types/tenant';
import type { BadgeVariant } from '@/components/ui/badge';

/** Maps tenant status to badge variant. */
const statusVariantMap: Record<TenantStatus, BadgeVariant> = {
  Active: 'success',
  Suspended: 'warning',
  Cancelled: 'error',
};

/** Maps subscription tier to badge variant. */
const tierVariantMap: Record<SubscriptionTier, BadgeVariant> = {
  Basic: 'secondary',
  Professional: 'default',
  Enterprise: 'outline',
};

/** All available subscription tiers. */
const TIER_OPTIONS: SubscriptionTier[] = ['Basic', 'Professional', 'Enterprise'];

/**
 * Tenant detail page.
 * Shows tenant info, status management, subscription tier, and carrier associations.
 */
export function TenantDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();

  const { data: tenant, isLoading, error } = useTenant(id!);
  const updateTenant = useUpdateTenant();
  const suspendTenant = useSuspendTenant();
  const activateTenant = useActivateTenant();
  const cancelTenant = useCancelTenant();
  const updateSubscription = useUpdateSubscription();
  const addCarrier = useAddTenantCarrier();
  const removeCarrier = useRemoveTenantCarrier();

  // Add carrier dialog state
  const [addCarrierOpen, setAddCarrierOpen] = React.useState(false);
  const [carrierIdInput, setCarrierIdInput] = React.useState('');
  const [agencyCodeInput, setAgencyCodeInput] = React.useState('');
  const [commissionRateInput, setCommissionRateInput] = React.useState('');
  const [addCarrierError, setAddCarrierError] = React.useState('');

  const handleNameSave = async (value: string) => {
    if (!tenant) return;
    try {
      await updateTenant.mutateAsync({ id: id!, data: { name: value } });
      addToast({
        title: t('common.toast.updated'),
        description: t('settings.tenants.detail.nameUpdated'),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : t('settings.tenants.detail.updateError'),
        variant: 'error',
      });
      throw err;
    }
  };

  const handleSuspend = async () => {
    try {
      await suspendTenant.mutateAsync(id!);
      addToast({
        title: t('common.toast.success'),
        description: t('settings.tenants.detail.suspended'),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : t('settings.tenants.detail.statusError'),
        variant: 'error',
      });
    }
  };

  const handleActivate = async () => {
    try {
      await activateTenant.mutateAsync(id!);
      addToast({
        title: t('common.toast.success'),
        description: t('settings.tenants.detail.activated'),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : t('settings.tenants.detail.statusError'),
        variant: 'error',
      });
    }
  };

  const handleCancel = async () => {
    try {
      await cancelTenant.mutateAsync(id!);
      addToast({
        title: t('common.toast.success'),
        description: t('settings.tenants.detail.cancelled'),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : t('settings.tenants.detail.statusError'),
        variant: 'error',
      });
    }
  };

  const handleTierChange = async (e: React.ChangeEvent<HTMLSelectElement>) => {
    const tier = e.target.value as SubscriptionTier;
    try {
      await updateSubscription.mutateAsync({ id: id!, data: { subscriptionTier: tier } });
      addToast({
        title: t('common.toast.updated'),
        description: t('settings.tenants.detail.tierUpdated'),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : t('settings.tenants.detail.tierError'),
        variant: 'error',
      });
    }
  };

  const handleOpenAddCarrier = () => {
    setCarrierIdInput('');
    setAgencyCodeInput('');
    setCommissionRateInput('');
    setAddCarrierError('');
    setAddCarrierOpen(true);
  };

  const handleAddCarrier = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!carrierIdInput.trim()) {
      setAddCarrierError(t('common.validation.required'));
      return;
    }
    setAddCarrierError('');

    try {
      await addCarrier.mutateAsync({
        id: id!,
        data: {
          carrierId: carrierIdInput.trim(),
          agencyCode: agencyCodeInput.trim() || undefined,
          commissionRate: commissionRateInput ? parseFloat(commissionRateInput) : undefined,
        },
      });
      addToast({
        title: t('common.toast.success'),
        description: t('settings.tenants.detail.carriers.added'),
        variant: 'success',
      });
      setAddCarrierOpen(false);
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : t('settings.tenants.detail.carriers.addError'),
        variant: 'error',
      });
    }
  };

  const handleRemoveCarrier = async (carrier: TenantCarrier) => {
    try {
      await removeCarrier.mutateAsync({ id: id!, carrierId: carrier.carrierId });
      addToast({
        title: t('common.toast.success'),
        description: t('settings.tenants.detail.carriers.removed'),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : t('settings.tenants.detail.carriers.removeError'),
        variant: 'error',
      });
    }
  };

  if (isLoading) {
    return <TenantDetailSkeleton />;
  }

  if (error || !tenant) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate('/settings/tenants')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('settings.tenants.detail.backToTenants')}
        </Button>
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {error instanceof Error ? error.message : t('settings.tenants.detail.notFound')}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const isStatusPending =
    suspendTenant.isPending || activateTenant.isPending || cancelTenant.isPending;

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <Button variant="ghost" onClick={() => navigate('/settings/tenants')}>
        <ArrowLeft className="mr-2 h-4 w-4" />
        {t('settings.tenants.detail.backToTenants')}
      </Button>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary/10">
            <Building2 className="h-6 w-6 text-primary" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <InlineEdit
                value={tenant.name}
                onSave={handleNameSave}
                displayClassName="text-2xl font-bold"
                placeholder={t('settings.tenants.detail.namePlaceholder')}
              />
              <Badge variant={statusVariantMap[tenant.status]}>
                {t(`settings.tenants.status.${tenant.status.toLowerCase()}`)}
              </Badge>
              <Badge
                variant={tierVariantMap[tenant.subscriptionTier]}
                className={cn(
                  tenant.subscriptionTier === 'Enterprise' && 'border-purple-300 text-purple-700 dark:text-purple-300',
                  tenant.subscriptionTier === 'Professional' && 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-100',
                )}
              >
                {tenant.subscriptionTier}
              </Badge>
            </div>
            <p className="font-mono text-sm text-muted-foreground">{tenant.subdomain}</p>
          </div>
        </div>
      </div>

      {/* Info Section */}
      <Card>
        <CardHeader>
          <CardTitle>{t('settings.tenants.detail.infoTitle')}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            <div>
              <p className="text-sm text-muted-foreground">{t('settings.tenants.columns.subdomain')}</p>
              <p className="font-mono text-sm font-medium">{tenant.subdomain}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('settings.tenants.detail.defaultCurrency')}</p>
              <p className="font-medium">{tenant.defaultCurrency}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('settings.tenants.detail.createdAt')}</p>
              <p className="font-medium">{formatDate(tenant.createdAt, 'en')}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('settings.tenants.detail.updatedAt')}</p>
              <p className="font-medium">
                {tenant.updatedAt ? formatDate(tenant.updatedAt, 'en') : '—'}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Status Actions */}
      <Card>
        <CardHeader>
          <CardTitle>{t('settings.tenants.detail.statusActions')}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-3">
            {tenant.status !== 'Active' && (
              <Button
                variant="outline"
                onClick={handleActivate}
                disabled={isStatusPending}
                isLoading={activateTenant.isPending}
                className="border-green-300 text-green-700 hover:bg-green-50 dark:border-green-700 dark:text-green-400 dark:hover:bg-green-950"
              >
                {t('settings.tenants.detail.activate')}
              </Button>
            )}
            {tenant.status === 'Active' && (
              <Button
                variant="outline"
                onClick={handleSuspend}
                disabled={isStatusPending}
                isLoading={suspendTenant.isPending}
                className="border-yellow-300 text-yellow-700 hover:bg-yellow-50 dark:border-yellow-700 dark:text-yellow-400 dark:hover:bg-yellow-950"
              >
                {t('settings.tenants.detail.suspend')}
              </Button>
            )}
            {tenant.status !== 'Cancelled' && (
              <Button
                variant="outline"
                onClick={handleCancel}
                disabled={isStatusPending}
                isLoading={cancelTenant.isPending}
                className="border-destructive text-destructive hover:bg-destructive/10"
              >
                {t('settings.tenants.detail.cancel')}
              </Button>
            )}
            {tenant.status === 'Cancelled' && (
              <p className="text-sm text-muted-foreground">
                {t('settings.tenants.detail.cancelledNote')}
              </p>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Subscription Tier */}
      <Card>
        <CardHeader>
          <CardTitle>{t('settings.tenants.detail.subscriptionTitle')}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="max-w-xs space-y-2">
            <Label htmlFor="subscription-tier">
              {t('settings.tenants.dialog.subscriptionTier')}
            </Label>
            <select
              id="subscription-tier"
              value={tenant.subscriptionTier}
              onChange={handleTierChange}
              disabled={updateSubscription.isPending}
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {TIER_OPTIONS.map((tier) => (
                <option key={tier} value={tier}>
                  {tier}
                </option>
              ))}
            </select>
          </div>
        </CardContent>
      </Card>

      {/* Carriers Section */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>{t('settings.tenants.detail.carriers.title')}</CardTitle>
          <Button variant="outline" size="sm" onClick={handleOpenAddCarrier}>
            <Plus className="mr-2 h-4 w-4" />
            {t('settings.tenants.detail.carriers.addCarrier')}
          </Button>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t('settings.tenants.detail.carriers.carrierId')}</TableHead>
                  <TableHead>{t('settings.tenants.detail.carriers.agencyCode')}</TableHead>
                  <TableHead>{t('settings.tenants.detail.carriers.commissionRate')}</TableHead>
                  <TableHead>{t('common.table.status')}</TableHead>
                  <TableHead className="w-16" />
                </TableRow>
              </TableHeader>
              <TableBody>
                {!tenant.carriers?.length ? (
                  <TableEmpty
                    message={t('settings.tenants.detail.carriers.noCarriers')}
                    colSpan={5}
                  />
                ) : (
                  tenant.carriers.map((carrier) => (
                    <TableRow key={carrier.carrierId}>
                      <TableCell>
                        <span className="font-mono text-sm">{carrier.carrierId}</span>
                      </TableCell>
                      <TableCell>
                        {carrier.agencyCode || <span className="text-muted-foreground">—</span>}
                      </TableCell>
                      <TableCell>
                        {carrier.commissionRate != null
                          ? `${carrier.commissionRate}%`
                          : <span className="text-muted-foreground">—</span>}
                      </TableCell>
                      <TableCell>
                        <Badge variant={carrier.isActive ? 'success' : 'secondary'}>
                          {carrier.isActive
                            ? t('common.status.active')
                            : t('common.status.inactive')}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-8 w-8 text-destructive hover:bg-destructive/10 hover:text-destructive"
                          onClick={() => handleRemoveCarrier(carrier)}
                          disabled={removeCarrier.isPending}
                          title={t('settings.tenants.detail.carriers.remove')}
                        >
                          <Trash2 className="h-4 w-4" />
                          <span className="sr-only">{t('settings.tenants.detail.carriers.remove')}</span>
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      {/* Add Carrier Dialog */}
      <Dialog open={addCarrierOpen} onOpenChange={setAddCarrierOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t('settings.tenants.detail.carriers.addTitle')}</DialogTitle>
          </DialogHeader>

          <form onSubmit={handleAddCarrier} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="carrier-id" required>
                {t('settings.tenants.detail.carriers.carrierId')}
              </Label>
              <Input
                id="carrier-id"
                value={carrierIdInput}
                onChange={(e) => {
                  setCarrierIdInput(e.target.value);
                  setAddCarrierError('');
                }}
                placeholder={t('settings.tenants.detail.carriers.carrierIdPlaceholder')}
                error={addCarrierError}
                autoFocus
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="agency-code">
                {t('settings.tenants.detail.carriers.agencyCode')}
              </Label>
              <Input
                id="agency-code"
                value={agencyCodeInput}
                onChange={(e) => setAgencyCodeInput(e.target.value)}
                placeholder={t('settings.tenants.detail.carriers.agencyCodePlaceholder')}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="commission-rate">
                {t('settings.tenants.detail.carriers.commissionRate')}
              </Label>
              <Input
                id="commission-rate"
                type="number"
                min="0"
                max="100"
                step="0.01"
                value={commissionRateInput}
                onChange={(e) => setCommissionRateInput(e.target.value)}
                placeholder="0.00"
              />
            </div>

            <div className="flex justify-end gap-3 pt-2">
              <Button type="button" variant="outline" onClick={() => setAddCarrierOpen(false)}>
                {t('common.actions.cancel')}
              </Button>
              <Button type="submit" isLoading={addCarrier.isPending}>
                {t('settings.tenants.detail.carriers.addCarrier')}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}

/**
 * Loading skeleton for the tenant detail page.
 */
function TenantDetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-9 w-36" />
      <div className="flex items-center gap-4">
        <Skeleton className="h-12 w-12 rounded-full" />
        <div className="space-y-2">
          <Skeleton className="h-8 w-56" />
          <Skeleton className="h-4 w-32" />
        </div>
      </div>
      <Skeleton className="h-40" />
      <Skeleton className="h-32" />
      <Skeleton className="h-32" />
      <Skeleton className="h-48" />
    </div>
  );
}
