import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Building2, ExternalLink } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { InlineEdit } from '@/components/common/inline-edit';
import { useToast } from '@/components/ui/toast';
import { CarrierProducts } from './components/carrier-products';
import { CarrierAppetites } from './components/carrier-appetites';
import { useCarrier, useUpdateCarrier } from '@/hooks/use-carriers';
import type { CarrierStatus } from '@/types/api';
import type { BadgeVariant } from '@/components/ui/badge';

/**
 * Status badge colors.
 */
const statusColors: Record<CarrierStatus, BadgeVariant> = {
  Active: 'success',
  Inactive: 'secondary',
  Suspended: 'error',
};

/**
 * Carrier detail page with products and appetites management.
 */
export function CarrierDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const { data: carrier, isLoading, error } = useCarrier(id!);
  const updateCarrier = useUpdateCarrier();

  const handleUpdate = async (field: string, value: string) => {
    if (!carrier) return;
    try {
      await updateCarrier.mutateAsync({
        id: id!,
        data: { name: carrier.name, [field]: value || undefined },
      });
      addToast({
        title: t('common.toast.updated'),
        description: 'Carrier information has been updated.',
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : 'Failed to update',
        variant: 'error',
      });
      throw error;
    }
  };

  if (isLoading) {
    return <CarrierDetailSkeleton />;
  }

  if (error || !carrier) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate('/carriers')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('carriers.detail.backToCarriers')}
        </Button>
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {error instanceof Error ? error.message : 'Carrier not found'}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <Button variant="ghost" onClick={() => navigate('/carriers')}>
        <ArrowLeft className="mr-2 h-4 w-4" />
        {t('carriers.detail.backToCarriers')}
      </Button>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary/10">
            <Building2 className="h-6 w-6 text-primary" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold">{carrier.name}</h1>
              <Badge variant={statusColors[carrier.status]}>
                {carrier.status}
              </Badge>
              {carrier.amBestRating && (
                <Badge variant="outline">AM Best: {carrier.amBestRating}</Badge>
              )}
            </div>
            <p className="font-mono text-muted-foreground">{carrier.code}</p>
          </div>
        </div>
      </div>

      {/* Carrier Details Card */}
      <Card>
        <CardHeader>
          <CardTitle>{t('carriers.detail.carrierInfo')}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            <div>
              <label className="text-sm text-muted-foreground">{t('carriers.dialog.name')}</label>
              <InlineEdit
                value={carrier.name}
                onSave={(value) => handleUpdate('name', value)}
                placeholder="Enter carrier name"
              />
            </div>
            <div>
              <label className="text-sm text-muted-foreground">{t('clients.form.business.legalName')}</label>
              <InlineEdit
                value={carrier.legalName || ''}
                onSave={(value) => handleUpdate('legalName', value)}
                placeholder="Enter legal name"
              />
            </div>
            <div>
              <label className="text-sm text-muted-foreground">{t('carriers.dialog.amBestRating')}</label>
              <InlineEdit
                value={carrier.amBestRating || ''}
                onSave={(value) => handleUpdate('amBestRating', value)}
                placeholder="e.g., A+"
              />
            </div>
            <div>
              <label className="text-sm text-muted-foreground">{t('carriers.dialog.naicCode')}</label>
              <InlineEdit
                value={carrier.naicCode || ''}
                onSave={(value) => handleUpdate('naicCode', value)}
                placeholder="Enter NAIC code"
              />
            </div>
            <div>
              <label className="text-sm text-muted-foreground">{t('carriers.dialog.website')}</label>
              <div className="flex items-center gap-2">
                <InlineEdit
                  value={carrier.websiteUrl || ''}
                  onSave={(value) => handleUpdate('websiteUrl', value)}
                  type="url"
                  placeholder="Enter website URL"
                />
                {carrier.websiteUrl && (
                  <a
                    href={carrier.websiteUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-primary hover:text-primary/80"
                  >
                    <ExternalLink className="h-4 w-4" />
                  </a>
                )}
              </div>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">API Endpoint</label>
              <InlineEdit
                value={carrier.apiEndpoint || ''}
                onSave={(value) => handleUpdate('apiEndpoint', value)}
                type="url"
                placeholder="Enter API endpoint"
              />
            </div>
          </div>

          <div className="mt-6 border-t pt-6">
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <div>
                <label className="text-sm text-muted-foreground">Carrier ID</label>
                <p className="font-mono text-sm">{carrier.id}</p>
              </div>
              <div>
                <label className="text-sm text-muted-foreground">{t('carriers.dialog.code')}</label>
                <p className="font-mono text-sm">{carrier.code}</p>
              </div>
              <div>
                <label className="text-sm text-muted-foreground">{t('clients.detail.created')}</label>
                <p className="text-sm">
                  {new Date(carrier.createdAt).toLocaleDateString()}
                </p>
              </div>
              <div>
                <label className="text-sm text-muted-foreground">{t('clients.detail.lastUpdated')}</label>
                <p className="text-sm">
                  {carrier.updatedAt
                    ? new Date(carrier.updatedAt).toLocaleDateString()
                    : '—'}
                </p>
              </div>
            </div>
          </div>

          {carrier.notes && (
            <div className="mt-6 border-t pt-6">
              <label className="text-sm text-muted-foreground">Notes</label>
              <p className="mt-1 text-sm">{carrier.notes}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Products Section */}
      <CarrierProducts carrierId={carrier.id} products={carrier.products} />

      {/* Appetites Section */}
      <CarrierAppetites carrierId={carrier.id} appetites={carrier.appetites} />
    </div>
  );
}

/**
 * Loading skeleton for carrier detail page.
 */
function CarrierDetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-9 w-32" />
      <div className="flex items-center gap-4">
        <Skeleton className="h-12 w-12 rounded-full" />
        <div>
          <Skeleton className="h-8 w-48" />
          <Skeleton className="mt-1 h-4 w-24" />
        </div>
      </div>
      <Skeleton className="h-64" />
      <Skeleton className="h-48" />
      <Skeleton className="h-48" />
    </div>
  );
}
