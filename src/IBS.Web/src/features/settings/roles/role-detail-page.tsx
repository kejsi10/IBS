import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Shield } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { RolePermissions } from './components/role-permissions';
import { useRole } from '@/hooks/use-roles';

/**
 * Role detail page with permission grid.
 */
export function RoleDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { data: role, isLoading, error } = useRole(id!);

  if (isLoading) {
    return <RoleDetailSkeleton />;
  }

  if (error || !role) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate('/settings/roles')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('settings.roles.detail.backToRoles')}
        </Button>
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {error instanceof Error ? error.message : t('settings.roles.notFound')}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <Button variant="ghost" onClick={() => navigate('/settings/roles')}>
        <ArrowLeft className="mr-2 h-4 w-4" />
        {t('settings.roles.detail.backToRoles')}
      </Button>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary/10">
            <Shield className="h-6 w-6 text-primary" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold">{role.name}</h1>
              <Badge variant={role.isSystemRole ? 'default' : 'outline'}>
                {role.isSystemRole ? t('settings.roles.systemRole') : t('settings.roles.customRole')}
              </Badge>
            </div>
            {role.description && (
              <p className="text-muted-foreground">{role.description}</p>
            )}
          </div>
        </div>
      </div>

      {/* Permissions */}
      <Card>
        <CardHeader>
          <CardTitle>{t('settings.roles.detail.permissions')}</CardTitle>
        </CardHeader>
        <CardContent>
          <RolePermissions
            roleId={role.id}
            assignedPermissions={role.permissions}
            isSystemRole={role.isSystemRole}
          />
        </CardContent>
      </Card>
    </div>
  );
}

/**
 * Loading skeleton for role detail page.
 */
function RoleDetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-9 w-32" />
      <div className="flex items-center gap-4">
        <Skeleton className="h-12 w-12 rounded-full" />
        <div>
          <Skeleton className="h-8 w-48" />
          <Skeleton className="mt-1 h-4 w-64" />
        </div>
      </div>
      <Skeleton className="h-64" />
    </div>
  );
}
