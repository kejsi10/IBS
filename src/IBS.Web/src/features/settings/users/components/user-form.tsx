import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import {
  createUserSchema,
  updateUserProfileSchema,
  type CreateUserFormData,
  type UpdateUserProfileFormData,
} from '@/lib/validations/user';

/**
 * Props for the create user form.
 */
export interface UserCreateFormProps {
  onSubmit: (data: CreateUserFormData) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
}

/**
 * Form for creating a new user.
 */
export function UserCreateForm({
  onSubmit,
  onCancel,
  isSubmitting = false,
}: UserCreateFormProps) {
  const { t } = useTranslation();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateUserFormData>({
    resolver: zodResolver(createUserSchema),
    defaultValues: {
      email: '',
      password: '',
      firstName: '',
      lastName: '',
      title: '',
      phoneNumber: '',
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="firstName">{t('settings.users.form.firstName')} *</Label>
          <Input
            id="firstName"
            {...register('firstName')}
            error={errors.firstName?.message}
            placeholder="John"
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="lastName">{t('settings.users.form.lastName')} *</Label>
          <Input
            id="lastName"
            {...register('lastName')}
            error={errors.lastName?.message}
            placeholder="Doe"
          />
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="email">{t('settings.users.form.email')} *</Label>
        <Input
          id="email"
          type="email"
          {...register('email')}
          error={errors.email?.message}
          placeholder="john.doe@example.com"
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="password">{t('settings.users.form.password')} *</Label>
        <Input
          id="password"
          type="password"
          {...register('password')}
          error={errors.password?.message}
          placeholder="Min. 8 characters"
        />
        <p className="text-xs text-muted-foreground">
          Must contain uppercase, lowercase, number, and special character.
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="title">Title</Label>
          <Input
            id="title"
            {...register('title')}
            error={errors.title?.message}
            placeholder="Insurance Agent"
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="phoneNumber">Phone</Label>
          <Input
            id="phoneNumber"
            type="tel"
            {...register('phoneNumber')}
            error={errors.phoneNumber?.message}
            placeholder="(555) 123-4567"
          />
        </div>
      </div>

      <div className="flex justify-end gap-3">
        <Button type="button" variant="outline" onClick={onCancel}>
          {t('common.actions.cancel')}
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? t('settings.users.form.creating') : t('settings.users.form.createUser')}
        </Button>
      </div>
    </form>
  );
}

/**
 * Props for the user profile form.
 */
export interface UserProfileFormProps {
  onSubmit: (data: UpdateUserProfileFormData) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
  defaultValues?: Partial<UpdateUserProfileFormData>;
}

/**
 * Form for updating a user's profile.
 */
export function UserProfileForm({
  onSubmit,
  onCancel,
  isSubmitting = false,
  defaultValues,
}: UserProfileFormProps) {
  const { t } = useTranslation();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateUserProfileFormData>({
    resolver: zodResolver(updateUserProfileSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      title: '',
      phoneNumber: '',
      ...defaultValues,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="profile-firstName">{t('settings.users.form.firstName')} *</Label>
          <Input
            id="profile-firstName"
            {...register('firstName')}
            error={errors.firstName?.message}
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="profile-lastName">{t('settings.users.form.lastName')} *</Label>
          <Input
            id="profile-lastName"
            {...register('lastName')}
            error={errors.lastName?.message}
          />
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="profile-title">Title</Label>
          <Input
            id="profile-title"
            {...register('title')}
            error={errors.title?.message}
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="profile-phoneNumber">Phone</Label>
          <Input
            id="profile-phoneNumber"
            type="tel"
            {...register('phoneNumber')}
            error={errors.phoneNumber?.message}
          />
        </div>
      </div>

      <div className="flex justify-end gap-3">
        <Button type="button" variant="outline" onClick={onCancel}>
          {t('common.actions.cancel')}
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? t('settings.users.form.saving') : t('settings.users.form.saveChanges')}
        </Button>
      </div>
    </form>
  );
}
