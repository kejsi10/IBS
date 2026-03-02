import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { Checkbox } from '@/components/ui/checkbox';
import { useToast } from '@/components/ui/toast';
import { usePermissions, useGrantPermission, useRevokePermission } from '@/hooks/use-roles';
import type { Permission } from '@/types/api';

/**
 * Props for the RolePermissions component.
 */
export interface RolePermissionsProps {
  /** Role ID */
  roleId: string;
  /** Permissions currently assigned to this role */
  assignedPermissions: Permission[];
  /** Whether this is a system role (read-only) */
  isSystemRole: boolean;
}

/**
 * Permission grid for a role, grouped by module.
 * Allows toggling permissions on/off for custom roles.
 */
export function RolePermissions({
  roleId,
  assignedPermissions,
  isSystemRole,
}: RolePermissionsProps) {
  const { t } = useTranslation();
  const { addToast } = useToast();
  const { data: allPermissions, isLoading } = usePermissions();
  const grantPermission = useGrantPermission();
  const revokePermission = useRevokePermission();

  // Group permissions by module
  const groupedPermissions = React.useMemo(() => {
    if (!allPermissions) return {};
    const groups: Record<string, Permission[]> = {};
    for (const perm of allPermissions) {
      if (!groups[perm.module]) {
        groups[perm.module] = [];
      }
      groups[perm.module].push(perm);
    }
    return groups;
  }, [allPermissions]);

  const assignedIds = React.useMemo(
    () => new Set(assignedPermissions.map((p) => p.id)),
    [assignedPermissions]
  );

  const handleToggle = async (permission: Permission, checked: boolean) => {
    try {
      if (checked) {
        await grantPermission.mutateAsync({
          roleId,
          data: { permissionId: permission.id },
        });
      } else {
        await revokePermission.mutateAsync({
          roleId,
          permissionId: permission.id,
        });
      }
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('settings.roles.permissions.updateError'),
        variant: 'error',
      });
    }
  };

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">{t('settings.roles.permissions.loading')}</p>;
  }

  if (!allPermissions || allPermissions.length === 0) {
    return <p className="text-sm text-muted-foreground">{t('settings.roles.permissions.noPermissions')}</p>;
  }

  const isMutating = grantPermission.isPending || revokePermission.isPending;

  return (
    <div className="space-y-6">
      {Object.entries(groupedPermissions).map(([module, permissions]) => (
        <div key={module}>
          <h4 className="mb-3 text-sm font-semibold uppercase tracking-wider text-muted-foreground">
            {module}
          </h4>
          <div className="grid gap-3 sm:grid-cols-2">
            {permissions.map((perm) => (
              <Checkbox
                key={perm.id}
                label={perm.name}
                description={perm.description}
                checked={assignedIds.has(perm.id)}
                disabled={isSystemRole || isMutating}
                onChange={(e) => handleToggle(perm, e.target.checked)}
              />
            ))}
          </div>
        </div>
      ))}
      {isSystemRole && (
        <p className="text-sm text-muted-foreground">
          {t('settings.roles.permissions.systemRoleReadOnly')}
        </p>
      )}
    </div>
  );
}
