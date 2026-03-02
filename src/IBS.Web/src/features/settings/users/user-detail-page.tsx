import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Mail, Phone, Shield } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/toast';
import { UserProfileForm } from './components/user-form';
import { UserRoleManager } from './components/user-role-manager';
import { useUser, useUpdateUser } from '@/hooks/use-users';
import type { UpdateUserProfileFormData } from '@/lib/validations/user';

/**
 * User detail page with profile editing and role management.
 */
export function UserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const { data: user, isLoading, error } = useUser(id!);
  const updateUser = useUpdateUser();

  const handleProfileSubmit = async (data: UpdateUserProfileFormData) => {
    try {
      await updateUser.mutateAsync({
        id: id!,
        data: {
          firstName: data.firstName,
          lastName: data.lastName,
          title: data.title || undefined,
          phoneNumber: data.phoneNumber || undefined,
        },
      });
      addToast({
        title: t('common.toast.success'),
        description: 'User profile has been updated.',
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : 'Failed to update profile',
        variant: 'error',
      });
    }
  };

  if (isLoading) {
    return <UserDetailSkeleton />;
  }

  if (error || !user) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate('/settings/users')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('settings.users.detail.backToUsers')}
        </Button>
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {error instanceof Error ? error.message : 'User not found'}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <Button variant="ghost" onClick={() => navigate('/settings/users')}>
        <ArrowLeft className="mr-2 h-4 w-4" />
        {t('settings.users.detail.backToUsers')}
      </Button>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary/10">
            <Shield className="h-6 w-6 text-primary" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold">{user.fullName}</h1>
              <Badge variant={user.isActive ? 'success' : 'secondary'}>
                {user.isActive ? t('settings.users.status.active') : t('settings.users.status.inactive')}
              </Badge>
            </div>
            <p className="text-muted-foreground">{user.email}</p>
          </div>
        </div>
      </div>

      {/* Quick Info Cards */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardContent className="flex items-center gap-3 pt-6">
            <Mail className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-sm text-muted-foreground">Email</p>
              <a href={`mailto:${user.email}`} className="text-primary hover:underline">
                {user.email}
              </a>
            </div>
          </CardContent>
        </Card>
        {user.phoneNumber && (
          <Card>
            <CardContent className="flex items-center gap-3 pt-6">
              <Phone className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-sm text-muted-foreground">Phone</p>
                <a href={`tel:${user.phoneNumber}`} className="hover:underline">
                  {user.phoneNumber}
                </a>
              </div>
            </CardContent>
          </Card>
        )}
        <Card>
          <CardContent className="flex items-center gap-3 pt-6">
            <Shield className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-sm text-muted-foreground">Last Login</p>
              <p className="font-medium">
                {user.lastLoginAt
                  ? new Date(user.lastLoginAt).toLocaleString()
                  : 'Never'}
              </p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Profile Section */}
      <Card>
        <CardHeader>
          <CardTitle>{t('settings.users.detail.profile')}</CardTitle>
        </CardHeader>
        <CardContent>
          <UserProfileForm
            onSubmit={handleProfileSubmit}
            onCancel={() => navigate('/settings/users')}
            isSubmitting={updateUser.isPending}
            defaultValues={{
              firstName: user.firstName,
              lastName: user.lastName,
              title: user.title ?? '',
              phoneNumber: user.phoneNumber ?? '',
            }}
          />
        </CardContent>
      </Card>

      {/* Roles Section */}
      <Card>
        <CardHeader>
          <CardTitle>{t('settings.users.detail.roles')}</CardTitle>
        </CardHeader>
        <CardContent>
          <UserRoleManager userId={user.id} currentRoles={user.roles} />
        </CardContent>
      </Card>
    </div>
  );
}

/**
 * Loading skeleton for user detail page.
 */
function UserDetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-9 w-32" />
      <div className="flex items-center gap-4">
        <Skeleton className="h-12 w-12 rounded-full" />
        <div>
          <Skeleton className="h-8 w-48" />
          <Skeleton className="mt-1 h-4 w-32" />
        </div>
      </div>
      <div className="grid gap-4 md:grid-cols-3">
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
      </div>
      <Skeleton className="h-64" />
      <Skeleton className="h-32" />
    </div>
  );
}
