import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';
import { useToast } from '@/components/ui/toast';
import { useCreateRole, useUpdateRole } from '@/hooks/use-roles';
import {
  createRoleSchema,
  type CreateRoleFormData,
} from '@/lib/validations/role';
import type { RoleSummary } from '@/types/api';

/**
 * Props for the RoleDialog component.
 */
export interface RoleDialogProps {
  /** Whether the dialog is open */
  open: boolean;
  /** Callback when open state changes */
  onOpenChange: (open: boolean) => void;
  /** Role to edit (null for create mode) */
  role?: RoleSummary | null;
}

/**
 * Dialog for creating or editing a role.
 */
export function RoleDialog({ open, onOpenChange, role }: RoleDialogProps) {
  const { t } = useTranslation();
  const { addToast } = useToast();
  const createRole = useCreateRole();
  const updateRole = useUpdateRole();
  const isEdit = !!role;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateRoleFormData>({
    resolver: zodResolver(createRoleSchema),
    defaultValues: {
      name: role?.name ?? '',
      description: role?.description ?? '',
    },
  });

  const handleClose = () => {
    onOpenChange(false);
    setTimeout(() => reset(), 200);
  };

  const onSubmit = async (data: CreateRoleFormData) => {
    try {
      if (isEdit) {
        await updateRole.mutateAsync({
          id: role.id,
          data: {
            name: data.name,
            description: data.description || undefined,
          },
        });
        addToast({
          title: t('common.toast.success'),
          description: `${data.name} has been updated.`,
          variant: 'success',
        });
      } else {
        await createRole.mutateAsync({
          name: data.name,
          description: data.description || undefined,
        });
        addToast({
          title: t('common.toast.success'),
          description: `${data.name} has been created.`,
          variant: 'success',
        });
      }
      handleClose();
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : `Failed to ${isEdit ? 'update' : 'create'} role`,
        variant: 'error',
      });
    }
  };

  const isPending = createRole.isPending || updateRole.isPending;

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>
            {isEdit ? t('settings.roles.dialog.editTitle') : t('settings.roles.dialog.createTitle')}
          </DialogTitle>
          <DialogDescription>
            {t('settings.roles.dialog.description')}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="role-name">{t('settings.roles.dialog.name')} *</Label>
            <Input
              id="role-name"
              {...register('name')}
              error={errors.name?.message}
              placeholder="e.g. Claims Manager"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="role-description">{t('settings.roles.dialog.desc')}</Label>
            <Textarea
              id="role-description"
              {...register('description')}
              placeholder="Describe what this role is for..."
              rows={3}
            />
            {errors.description?.message && (
              <p className="text-sm text-destructive">{errors.description.message}</p>
            )}
          </div>
          <div className="flex justify-end gap-3">
            <Button type="button" variant="outline" onClick={handleClose}>
              {t('common.actions.cancel')}
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending
                ? (isEdit ? t('settings.roles.dialog.saving') : t('settings.roles.dialog.creating'))
                : (isEdit ? t('settings.roles.dialog.save') : t('settings.roles.dialog.create'))}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
