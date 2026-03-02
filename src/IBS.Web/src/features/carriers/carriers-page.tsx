import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { DataTable } from '@/components/common/data-table';
import { SearchInput } from '@/components/common/search-input';
import { QuickFilters, type FilterOption } from '@/components/common/quick-filters';
import { useToast } from '@/components/ui/toast';
import { createCarrierColumns } from './components/carrier-table';
import { CarrierDialog } from './components/carrier-dialog';
import { useCarriers, useDeactivateCarrier } from '@/hooks/use-carriers';
import type { CarrierSummary, CarrierStatus } from '@/types/api';

/** Filter value type including 'all' option */
type CarrierStatusFilter = CarrierStatus | 'all';

/**
 * Carriers list page with filters, search, and data table.
 */
export function CarriersPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState<CarrierStatusFilter>('all');

  // Fetch carriers
  const { data, isLoading, error } = useCarriers();
  const deactivateCarrier = useDeactivateCarrier();

  // Filter data locally
  const filteredData = React.useMemo(() => {
    if (!data) return [];
    let items = data;

    // Filter by status
    if (statusFilter !== 'all') {
      items = items.filter((c: CarrierSummary) => c.status === statusFilter);
    }

    // Filter by search
    if (search) {
      const query = search.toLowerCase();
      items = items.filter((c: CarrierSummary) =>
        c.name.toLowerCase().includes(query) ||
        c.code.toLowerCase().includes(query)
      );
    }

    return items;
  }, [data, statusFilter, search]);

  // Calculate counts for filters
  const counts = React.useMemo(() => {
    if (!data) return undefined;
    const all = data.length;
    const active = data.filter((c: CarrierSummary) => c.status === 'Active').length;
    const inactive = data.filter((c: CarrierSummary) => c.status === 'Inactive').length;
    const suspended = data.filter((c: CarrierSummary) => c.status === 'Suspended').length;
    return { all, active, inactive, suspended };
  }, [data]);

  const filterOptions: FilterOption<CarrierStatusFilter>[] = React.useMemo(() => [
    { value: 'all', label: t('carriers.status.all'), count: counts?.all },
    { value: 'Active', label: t('carriers.status.active'), count: counts?.active },
    { value: 'Inactive', label: t('carriers.status.inactive'), count: counts?.inactive },
    { value: 'Suspended', label: t('carriers.status.suspended'), count: counts?.suspended },
  ], [t, counts]);

  // Action handlers
  const handleView = (carrier: CarrierSummary) => {
    navigate(`/carriers/${carrier.id}`);
  };

  const handleEdit = (carrier: CarrierSummary) => {
    navigate(`/carriers/${carrier.id}?edit=true`);
  };

  const handleDeactivate = async (carrier: CarrierSummary) => {
    try {
      await deactivateCarrier.mutateAsync({ id: carrier.id });
      addToast({
        title: t('carriers.actions.deactivate'),
        description: `${carrier.name} has been deactivated.`,
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : 'Failed to deactivate carrier',
        variant: 'error',
      });
    }
  };

  const handleActivate = async (_carrier: CarrierSummary) => {
    // Note: Would need an activate hook - for now just show a message
    addToast({
      title: 'Info',
      description: 'Carrier activation would be implemented here.',
      variant: 'info',
    });
  };

  // Create columns with actions
  const columns = React.useMemo(
    () => createCarrierColumns({
      onView: handleView,
      onEdit: handleEdit,
      onDeactivate: handleDeactivate,
      onActivate: handleActivate,
    }, t),
    [t]
  );

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{t('carriers.title')}</h1>
          <p className="text-muted-foreground">
            {t('carriers.subtitle')}
          </p>
        </div>
        <Button onClick={() => setDialogOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t('carriers.addCarrier')}
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <QuickFilters
              options={filterOptions}
              value={statusFilter}
              onChange={setStatusFilter}
            />
            <SearchInput
              value={search}
              onSearch={setSearch}
              placeholder={t('carriers.searchPlaceholder')}
              className="w-full sm:w-80"
            />
          </div>
        </CardContent>
      </Card>

      {/* Error State */}
      {error && (
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              Failed to load carriers: {error instanceof Error ? error.message : 'Unknown error'}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Carriers Table */}
      <DataTable
        columns={columns}
        data={filteredData}
        isLoading={isLoading}
        emptyMessage={t('carriers.noCarriers')}
        onRowClick={handleView}
        getRowId={(row) => row.id}
      />

      {/* Create Carrier Dialog */}
      <CarrierDialog open={dialogOpen} onOpenChange={setDialogOpen} />
    </div>
  );
}
