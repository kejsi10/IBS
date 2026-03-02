import * as React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, Building2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { SearchInput } from '@/components/common/search-input';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableEmpty,
  TableHead,
  TableHeader,
  TableLoading,
  TableRow,
} from '@/components/ui/table';
import { useToast } from '@/components/ui/toast';
import { useTenants, useCreateTenant } from '@/hooks/use-tenants';
import { formatDate } from '@/lib/format';
import { cn } from '@/lib/utils';
import type { TenantStatus, SubscriptionTier } from '@/types/tenant';
import type { BadgeVariant } from '@/components/ui/badge';

/** Maps tenant status to badge variant. */
const statusVariantMap: Record<TenantStatus, BadgeVariant> = {
  Active: 'success',
  Suspended: 'warning',
  Cancelled: 'error',
};

/** Maps subscription tier to badge variant. */
const tierVariantMap: Record<SubscriptionTier, BadgeVariant> = {
  Basic: 'secondary',
  Professional: 'default',
  Enterprise: 'outline',
};

/** Subscription tier options for the create dialog. */
const TIER_OPTIONS: SubscriptionTier[] = ['Basic', 'Professional', 'Enterprise'];

/** Number of rows per page. */
const PAGE_SIZE = 20;

/**
 * Create tenant form data.
 */
interface CreateTenantFormData {
  name: string;
  subdomain: string;
  subscriptionTier: SubscriptionTier;
}

/**
 * Tenant management list page.
 * Displays a searchable, paginated table of all tenants with a create dialog.
 */
export function TenantsPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();

  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [search, setSearch] = React.useState('');
  const [page, setPage] = React.useState(1);

  // Reset to page 1 when search changes
  React.useEffect(() => {
    setPage(1);
  }, [search]);

  const { data, isLoading, error } = useTenants({ page, pageSize: PAGE_SIZE, search: search || undefined });
  const createTenant = useCreateTenant();

  // Form state
  const [form, setForm] = React.useState<CreateTenantFormData>({
    name: '',
    subdomain: '',
    subscriptionTier: 'Basic',
  });
  const [formErrors, setFormErrors] = React.useState<Partial<Record<keyof CreateTenantFormData, string>>>({});

  const handleOpenDialog = () => {
    setForm({ name: '', subdomain: '', subscriptionTier: 'Basic' });
    setFormErrors({});
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
  };

  const validateForm = (): boolean => {
    const errors: Partial<Record<keyof CreateTenantFormData, string>> = {};
    if (!form.name.trim()) {
      errors.name = t('common.validation.required');
    }
    if (!form.subdomain.trim()) {
      errors.subdomain = t('common.validation.required');
    } else if (!/^[a-z0-9-]+$/.test(form.subdomain)) {
      errors.subdomain = t('settings.tenants.dialog.subdomainFormat');
    }
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;

    try {
      const result = await createTenant.mutateAsync({
        name: form.name.trim(),
        subdomain: form.subdomain.trim(),
        subscriptionTier: form.subscriptionTier,
      });
      addToast({
        title: t('common.toast.success'),
        description: t('settings.tenants.dialog.created', { name: form.name }),
        variant: 'success',
      });
      handleCloseDialog();
      navigate(`/settings/tenants/${result.id}`);
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: err instanceof Error ? err.message : t('settings.tenants.dialog.createError'),
        variant: 'error',
      });
    }
  };

  const totalPages = data?.totalPages ?? 1;
  const hasPrev = page > 1;
  const hasNext = page < totalPages;

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold">{t('settings.tenants.title')}</h2>
          <p className="text-sm text-muted-foreground">
            {t('settings.tenants.subtitle')}
          </p>
        </div>
        <Button onClick={handleOpenDialog}>
          <Plus className="mr-2 h-4 w-4" />
          {t('settings.tenants.createTenant')}
        </Button>
      </div>

      {/* Search */}
      <Card>
        <CardContent className="pt-6">
          <SearchInput
            value={search}
            onSearch={setSearch}
            placeholder={t('settings.tenants.searchPlaceholder')}
            className="w-full sm:w-80"
          />
        </CardContent>
      </Card>

      {/* Error State */}
      {error && (
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {t('settings.tenants.loadError')}: {error instanceof Error ? error.message : t('common.unknownError')}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Tenants Table */}
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t('settings.tenants.columns.name')}</TableHead>
              <TableHead>{t('settings.tenants.columns.subdomain')}</TableHead>
              <TableHead>{t('settings.tenants.columns.status')}</TableHead>
              <TableHead>{t('settings.tenants.columns.tier')}</TableHead>
              <TableHead>{t('settings.tenants.columns.createdAt')}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableLoading colSpan={5} rows={5} />
            ) : !data?.items?.length ? (
              <TableEmpty
                message={search ? t('settings.tenants.noTenantsSearch') : t('settings.tenants.noTenants')}
                colSpan={5}
              />
            ) : (
              data.items.map((tenant) => (
                <TableRow
                  key={tenant.id}
                  className="cursor-pointer"
                  onClick={() => navigate(`/settings/tenants/${tenant.id}`)}
                >
                  <TableCell>
                    <Link
                      to={`/settings/tenants/${tenant.id}`}
                      className="flex items-center gap-2 font-medium hover:text-primary"
                      onClick={(e) => e.stopPropagation()}
                    >
                      <Building2 className="h-4 w-4 text-muted-foreground" />
                      {tenant.name}
                    </Link>
                  </TableCell>
                  <TableCell>
                    <span className="font-mono text-sm">{tenant.subdomain}</span>
                  </TableCell>
                  <TableCell>
                    <Badge variant={statusVariantMap[tenant.status]}>
                      {t(`settings.tenants.status.${tenant.status.toLowerCase()}`)}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <Badge
                      variant={tierVariantMap[tenant.subscriptionTier]}
                      className={cn(
                        tenant.subscriptionTier === 'Enterprise' && 'border-purple-300 text-purple-700 dark:text-purple-300',
                        tenant.subscriptionTier === 'Professional' && 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-100',
                      )}
                    >
                      {tenant.subscriptionTier}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {formatDate(tenant.createdAt, 'en')}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>
            {t('common.pagination.showing', {
              from: (page - 1) * PAGE_SIZE + 1,
              to: Math.min(page * PAGE_SIZE, data.totalCount),
              total: data.totalCount,
            })}
          </span>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage((p) => p - 1)}
              disabled={!hasPrev}
            >
              {t('common.pagination.previous')}
            </Button>
            <span className="px-2">
              {t('common.pagination.pageOf', { page, totalPages })}
            </span>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage((p) => p + 1)}
              disabled={!hasNext}
            >
              {t('common.pagination.next')}
            </Button>
          </div>
        </div>
      )}

      {/* Create Tenant Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t('settings.tenants.dialog.title')}</DialogTitle>
          </DialogHeader>

          <form onSubmit={handleCreate} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="tenant-name" required>
                {t('settings.tenants.dialog.name')}
              </Label>
              <Input
                id="tenant-name"
                value={form.name}
                onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                placeholder={t('settings.tenants.dialog.namePlaceholder')}
                error={formErrors.name}
                autoFocus
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="tenant-subdomain" required>
                {t('settings.tenants.dialog.subdomain')}
              </Label>
              <Input
                id="tenant-subdomain"
                value={form.subdomain}
                onChange={(e) =>
                  setForm((f) => ({ ...f, subdomain: e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, '') }))
                }
                placeholder={t('settings.tenants.dialog.subdomainPlaceholder')}
                error={formErrors.subdomain}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="tenant-tier">
                {t('settings.tenants.dialog.subscriptionTier')}
              </Label>
              <select
                id="tenant-tier"
                value={form.subscriptionTier}
                onChange={(e) => setForm((f) => ({ ...f, subscriptionTier: e.target.value as SubscriptionTier }))}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {TIER_OPTIONS.map((tier) => (
                  <option key={tier} value={tier}>
                    {tier}
                  </option>
                ))}
              </select>
            </div>

            <div className="flex justify-end gap-3 pt-2">
              <Button type="button" variant="outline" onClick={handleCloseDialog}>
                {t('common.actions.cancel')}
              </Button>
              <Button type="submit" isLoading={createTenant.isPending}>
                {t('settings.tenants.dialog.create')}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
