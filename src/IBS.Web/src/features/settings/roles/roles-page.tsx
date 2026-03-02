import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { DataTable } from '@/components/common/data-table';
import { createRoleColumns } from './components/role-table';
import { RoleDialog } from './components/role-dialog';
import { useRoles } from '@/hooks/use-roles';
import type { RoleSummary } from '@/types/api';

/**
 * Roles list page with data table and create dialog.
 */
export function RolesPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [editRole, setEditRole] = React.useState<RoleSummary | null>(null);

  const { data: roles, isLoading, error } = useRoles();

  // Action handlers
  const handleView = (role: RoleSummary) => {
    navigate(`/settings/roles/${role.id}`);
  };

  const handleEdit = (role: RoleSummary) => {
    setEditRole(role);
    setDialogOpen(true);
  };

  const handleDialogClose = (open: boolean) => {
    setDialogOpen(open);
    if (!open) {
      setTimeout(() => setEditRole(null), 200);
    }
  };

  // Create columns with actions
  const columns = React.useMemo(
    () => createRoleColumns({
      onView: handleView,
      onEdit: handleEdit,
    }, t),
    [t]
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold">{t('settings.roles.title')}</h2>
          <p className="text-sm text-muted-foreground">
            {t('settings.roles.subtitle')}
          </p>
        </div>
        <Button onClick={() => setDialogOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t('settings.roles.createRole')}
        </Button>
      </div>

      {/* Error State */}
      {error && (
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {t('settings.roles.loadError')}: {error instanceof Error ? error.message : 'Unknown error'}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Roles Table */}
      <DataTable
        columns={columns}
        data={roles ?? []}
        isLoading={isLoading}
        emptyMessage={t('settings.roles.noRoles')}
        onRowClick={handleView}
        getRowId={(row) => row.id}
      />

      {/* Create/Edit Role Dialog */}
      <RoleDialog
        open={dialogOpen}
        onOpenChange={handleDialogClose}
        role={editRole}
      />
    </div>
  );
}
