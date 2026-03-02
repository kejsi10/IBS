import { useTranslation } from 'react-i18next';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { useToast } from '@/components/ui/toast';
import { UserCreateForm } from './user-form';
import { useCreateUser } from '@/hooks/use-users';
import type { CreateUserFormData } from '@/lib/validations/user';

/**
 * Props for the UserDialog component.
 */
export interface UserDialogProps {
  /** Whether the dialog is open */
  open: boolean;
  /** Callback when open state changes */
  onOpenChange: (open: boolean) => void;
}

/**
 * Dialog for creating a new user.
 */
export function UserDialog({ open, onOpenChange }: UserDialogProps) {
  const { t } = useTranslation();
  const { addToast } = useToast();
  const createUser = useCreateUser();

  const handleSubmit = async (data: CreateUserFormData) => {
    try {
      await createUser.mutateAsync({
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
        title: data.title || undefined,
        phoneNumber: data.phoneNumber || undefined,
      });

      addToast({
        title: t('common.toast.success'),
        description: `${data.firstName} ${data.lastName} has been added.`,
        variant: 'success',
      });

      onOpenChange(false);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : 'Failed to create user',
        variant: 'error',
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{t('settings.users.addUser')}</DialogTitle>
          <DialogDescription>
            Create a new user account. They will receive login credentials.
          </DialogDescription>
        </DialogHeader>
        <UserCreateForm
          onSubmit={handleSubmit}
          onCancel={() => onOpenChange(false)}
          isSubmitting={createUser.isPending}
        />
      </DialogContent>
    </Dialog>
  );
}
