import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { getErrorMessage } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { DataTable } from '@/components/common/data-table';
import { useToast } from '@/components/ui/toast';
import { PolicyDashboard } from './components/policy-dashboard';
import { PolicyFilters, type PolicyStatusFilter, type AdvancedFilters } from './components/policy-filters';
import { createPolicyColumns } from './components/policy-table';
import {
  usePolicies,
  useBindPolicy,
  useActivatePolicy,
  useRenewPolicy,
} from '@/hooks/use-policies';
import type { PolicySummary, PolicyFilters as PolicyFilterParams } from '@/types/api';

/**
 * Policies page with dashboard, filters, and data table.
 */
export function PoliciesPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState<PolicyStatusFilter>('all');
  const [advancedFilters, setAdvancedFilters] = React.useState<AdvancedFilters>({});

  // Build filter params
  const filterParams: PolicyFilterParams = React.useMemo(() => ({
    search: search || undefined,
    status: statusFilter === 'all' ? undefined : statusFilter,
    effectiveDateFrom: advancedFilters.effectiveDateFrom,
    effectiveDateTo: advancedFilters.effectiveDateTo,
    expirationDateFrom: advancedFilters.expirationDateFrom,
    expirationDateTo: advancedFilters.expirationDateTo,
  }), [search, statusFilter, advancedFilters]);

  // Fetch policies
  const { data, isLoading, error } = usePolicies(filterParams);
  const bindPolicy = useBindPolicy();
  const activatePolicy = useActivatePolicy();
  const renewPolicy = useRenewPolicy();

  // Calculate counts for filters
  const counts = React.useMemo(() => {
    if (!data?.items) return undefined;
    return {
      all: data.totalCount,
      draft: data.items.filter((p) => p.status === 'Draft').length,
      bound: data.items.filter((p) => p.status === 'Bound').length,
      active: data.items.filter((p) => p.status === 'Active').length,
      cancelled: data.items.filter((p) => p.status === 'Cancelled').length,
      expired: data.items.filter((p) => p.status === 'Expired').length,
    };
  }, [data]);

  // Action handlers
  const handleView = (policy: PolicySummary) => {
    navigate(`/policies/${policy.id}`);
  };

  const handleEdit = (policy: PolicySummary) => {
    navigate(`/policies/${policy.id}?edit=true`);
  };

  const handleBind = async (policy: PolicySummary) => {
    try {
      await bindPolicy.mutateAsync(policy.id);
      addToast({
        title: t('policies.actions.bind'),
        description: t('policies.toast.bound', { policyNumber: policy.policyNumber }),
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

  const handleActivate = async (policy: PolicySummary) => {
    try {
      await activatePolicy.mutateAsync(policy.id);
      addToast({
        title: t('policies.actions.activate'),
        description: t('policies.toast.activated', { policyNumber: policy.policyNumber }),
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

  const handleRenew = async (policy: PolicySummary) => {
    try {
      await renewPolicy.mutateAsync(policy.id);
      addToast({
        title: t('policies.actions.renew'),
        description: t('policies.toast.renewed', { policyNumber: policy.policyNumber }),
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

  const handleCancel = (policy: PolicySummary) => {
    navigate(`/policies/${policy.id}?cancel=true`);
  };

  // Create columns with actions
  const columns = React.useMemo(
    () => createPolicyColumns({
      onView: handleView,
      onEdit: handleEdit,
      onBind: handleBind,
      onActivate: handleActivate,
      onRenew: handleRenew,
      onCancel: handleCancel,
    }, t),
    [t]
  );

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{t('policies.title')}</h1>
          <p className="text-muted-foreground">
            {t('policies.subtitle')}
          </p>
        </div>
        <Button onClick={() => navigate('/policies/new')}>
          <Plus className="mr-2 h-4 w-4" />
          {t('policies.newPolicy')}
        </Button>
      </div>

      {/* Dashboard Section */}
      <PolicyDashboard />

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <PolicyFilters
            search={search}
            onSearchChange={setSearch}
            statusFilter={statusFilter}
            onStatusFilterChange={setStatusFilter}
            advancedFilters={advancedFilters}
            onAdvancedFiltersChange={setAdvancedFilters}
            counts={counts}
          />
        </CardContent>
      </Card>

      {/* Error State */}
      {error && (
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {t('policies.loadError')}{error instanceof Error ? `: ${error.message}` : ''}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Policies Table */}
      <DataTable
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        emptyMessage={t('policies.noResults')}
        onRowClick={handleView}
        getRowId={(row) => row.id}
      />
    </div>
  );
}
