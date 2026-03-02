import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { DataTable } from '@/components/common/data-table';
import { useToast } from '@/components/ui/toast';
import { ClientFilters, type ClientStatusFilter } from './components/client-filters';
import { createClientColumns } from './components/client-table';
import { ClientDialog } from './components/client-dialog';
import { useClients, useDeactivateClient, useReactivateClient } from '@/hooks/use-clients';
import type { ClientSummary, ClientFilters as ClientFilterParams } from '@/types/api';

/**
 * Clients list page with filters, search, and data table.
 */
export function ClientsPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState<ClientStatusFilter>('all');

  // Build filter params
  const filterParams: ClientFilterParams = React.useMemo(() => ({
    search: search || undefined,
    status: statusFilter === 'all' ? undefined : statusFilter,
  }), [search, statusFilter]);

  // Fetch clients
  const { data, isLoading, error } = useClients(filterParams);
  const deactivateClient = useDeactivateClient();
  const reactivateClient = useReactivateClient();

  // Calculate counts for filters
  const counts = React.useMemo(() => {
    if (!data?.items) return undefined;
    const all = data.totalCount;
    const active = data.items.filter(c => c.status === 'Active').length;
    const inactive = data.items.filter(c => c.status === 'Inactive').length;
    return { all, active, inactive };
  }, [data]);

  // Action handlers
  const handleView = (client: ClientSummary) => {
    navigate(`/clients/${client.id}`);
  };

  const handleEdit = (client: ClientSummary) => {
    navigate(`/clients/${client.id}?edit=true`);
  };

  const handleDeactivate = async (client: ClientSummary) => {
    try {
      await deactivateClient.mutateAsync(client.id);
      addToast({
        title: t('clients.toast.deactivated'),
        description: t('clients.toast.deactivatedDesc', { name: client.displayName }),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : t('clients.toast.failedDeactivate'),
        variant: 'error',
      });
    }
  };

  const handleReactivate = async (client: ClientSummary) => {
    try {
      await reactivateClient.mutateAsync(client.id);
      addToast({
        title: t('clients.toast.reactivated'),
        description: t('clients.toast.reactivatedDesc', { name: client.displayName }),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : t('clients.toast.failedReactivate'),
        variant: 'error',
      });
    }
  };

  // Create columns with actions
  const columns = React.useMemo(
    () => createClientColumns({
      onView: handleView,
      onEdit: handleEdit,
      onDeactivate: handleDeactivate,
      onReactivate: handleReactivate,
    }, t),
    [t]
  );

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{t('clients.title')}</h1>
          <p className="text-muted-foreground">
            {t('clients.subtitle')}
          </p>
        </div>
        <Button onClick={() => setDialogOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t('clients.addClient')}
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <ClientFilters
            search={search}
            onSearchChange={setSearch}
            statusFilter={statusFilter}
            onStatusFilterChange={setStatusFilter}
            counts={counts}
          />
        </CardContent>
      </Card>

      {/* Error State */}
      {error && (
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              Failed to load clients: {error instanceof Error ? error.message : 'Unknown error'}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Clients Table */}
      <DataTable
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        emptyMessage={t('clients.noClients')}
        onRowClick={handleView}
        getRowId={(row) => row.id}
      />

      {/* Create Client Dialog */}
      <ClientDialog open={dialogOpen} onOpenChange={setDialogOpen} />
    </div>
  );
}
