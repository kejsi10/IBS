import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { DataTable } from '@/components/common/data-table';
import { useToast } from '@/components/ui/toast';
import { UserFilters, type UserStatusFilter } from './components/user-filters';
import { createUserColumns } from './components/user-table';
import { UserDialog } from './components/user-dialog';
import { useUsers, useDeactivateUser, useActivateUser } from '@/hooks/use-users';
import type { UserSummary, UserFilters as UserFilterParams } from '@/types/api';

/**
 * Users list page with filters, search, and data table.
 */
export function UsersPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState<UserStatusFilter>('all');

  // Build filter params
  const filterParams: UserFilterParams = React.useMemo(() => ({
    search: search || undefined,
    isActive: statusFilter === 'all' ? undefined : statusFilter === 'active',
  }), [search, statusFilter]);

  // Fetch users
  const { data, isLoading, error } = useUsers(filterParams);
  const deactivateUser = useDeactivateUser();
  const activateUser = useActivateUser();

  // Calculate counts for filters
  const counts = React.useMemo(() => {
    if (!data?.items) return undefined;
    const all = data.totalCount;
    const active = data.items.filter(u => u.isActive).length;
    const inactive = data.items.filter(u => !u.isActive).length;
    return { all, active, inactive };
  }, [data]);

  // Action handlers
  const handleView = (user: UserSummary) => {
    navigate(`/settings/users/${user.id}`);
  };

  const handleDeactivate = async (user: UserSummary) => {
    try {
      await deactivateUser.mutateAsync(user.id);
      addToast({
        title: t('common.toast.success'),
        description: `${user.fullName} has been deactivated.`,
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : 'Failed to deactivate user',
        variant: 'error',
      });
    }
  };

  const handleActivate = async (user: UserSummary) => {
    try {
      await activateUser.mutateAsync(user.id);
      addToast({
        title: t('common.toast.success'),
        description: `${user.fullName} has been activated.`,
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : 'Failed to activate user',
        variant: 'error',
      });
    }
  };

  // Create columns with actions
  const columns = React.useMemo(
    () => createUserColumns({
      onView: handleView,
      onDeactivate: handleDeactivate,
      onActivate: handleActivate,
    }, t),
    [t]
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold">{t('settings.users.title')}</h2>
          <p className="text-sm text-muted-foreground">
            {t('settings.users.subtitle')}
          </p>
        </div>
        <Button onClick={() => setDialogOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t('settings.users.addUser')}
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <UserFilters
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
              Failed to load users: {error instanceof Error ? error.message : 'Unknown error'}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Users Table */}
      <DataTable
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        emptyMessage={t('settings.users.noUsers')}
        onRowClick={handleView}
        getRowId={(row) => row.id}
      />

      {/* Create User Dialog */}
      <UserDialog open={dialogOpen} onOpenChange={setDialogOpen} />
    </div>
  );
}
