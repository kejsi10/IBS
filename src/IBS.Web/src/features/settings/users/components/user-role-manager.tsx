import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Select } from '@/components/ui/select';
import { useToast } from '@/components/ui/toast';
import { useRoles, useAssignRole, useRemoveRole } from '@/hooks';

/**
 * Props for the UserRoleManager component.
 */
export interface UserRoleManagerProps {
  /** User ID */
  userId: string;
  /** Current roles assigned to the user */
  currentRoles: string[];
}

/**
 * Manages role assignments for a user.
 * Shows current roles as removable badges and a dropdown to add new roles.
 */
export function UserRoleManager({ userId, currentRoles }: UserRoleManagerProps) {
  const { t } = useTranslation();
  const { addToast } = useToast();
  const { data: allRoles, isLoading: rolesLoading } = useRoles();
  const assignRole = useAssignRole();
  const removeRole = useRemoveRole();

  const availableRoles = allRoles?.filter(
    (role) => !currentRoles.includes(role.name)
  ) ?? [];

  const handleAssign = async (roleId: string) => {
    try {
      await assignRole.mutateAsync({ userId, data: { roleId } });
      const roleName = allRoles?.find((r) => r.id === roleId)?.name;
      addToast({
        title: t('common.toast.success'),
        description: `${roleName} role has been assigned.`,
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : 'Failed to assign role',
        variant: 'error',
      });
    }
  };

  const handleRemove = async (roleName: string) => {
    const role = allRoles?.find((r) => r.name === roleName);
    if (!role) return;

    try {
      await removeRole.mutateAsync({ userId, roleId: role.id });
      addToast({
        title: t('common.toast.success'),
        description: `${roleName} role has been removed.`,
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : 'Failed to remove role',
        variant: 'error',
      });
    }
  };

  return (
    <div className="space-y-4">
      {/* Current Roles */}
      <div className="flex flex-wrap gap-2">
        {currentRoles.length > 0 ? (
          currentRoles.map((role) => (
            <Badge key={role} variant="default" size="lg" className="gap-1">
              {role}
              <Button
                variant="ghost"
                size="icon"
                className="h-4 w-4 p-0 hover:bg-transparent"
                onClick={() => handleRemove(role)}
                disabled={removeRole.isPending}
              >
                <X className="h-3 w-3" />
                <span className="sr-only">Remove {role} role</span>
              </Button>
            </Badge>
          ))
        ) : (
          <p className="text-sm text-muted-foreground">{t('settings.users.roleManager.noRoles')}</p>
        )}
      </div>

      {/* Add Role */}
      {availableRoles.length > 0 && (
        <div className="flex items-center gap-2">
          <Select
            options={availableRoles.map((role) => ({
              value: role.id,
              label: role.name,
            }))}
            placeholder={t('settings.users.roleManager.assign')}
            onChange={handleAssign}
            disabled={rolesLoading || assignRole.isPending}
            aria-label={t('settings.users.roleManager.assign')}
          />
        </div>
      )}
    </div>
  );
}
