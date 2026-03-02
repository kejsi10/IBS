import * as React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Power, PowerOff } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Select, type SelectOption } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { DataTable } from '@/components/common/data-table';
import { useToast } from '@/components/ui/toast';
import { useAddProduct, useDeactivateProduct } from '@/hooks/use-carriers';
import { addProductSchema, lineOfBusinessOptions, type AddProductFormData } from '@/lib/validations/carrier';
import type { Product } from '@/types/api';
import type { ColumnDef } from '@tanstack/react-table';

/**
 * Props for the CarrierProducts component.
 */
export interface CarrierProductsProps {
  carrierId: string;
  products: Product[];
}

/**
 * Formats line of business for display.
 */
function formatLob(lob: string): string {
  return lob.replace(/([A-Z])/g, ' $1').trim();
}

/**
 * Products management component for carrier detail page.
 */
export function CarrierProducts({ carrierId, products }: CarrierProductsProps) {
  const [showAddForm, setShowAddForm] = React.useState(false);
  const { t } = useTranslation();
  const { addToast } = useToast();
  const addProduct = useAddProduct();
  const deactivateProduct = useDeactivateProduct();

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<AddProductFormData>({
    resolver: zodResolver(addProductSchema),
    defaultValues: {
      name: '',
      lineOfBusiness: undefined,
      code: '',
      description: '',
    },
  });

  const selectedLob = watch('lineOfBusiness');

  const lobSelectOptions: SelectOption[] = lineOfBusinessOptions.map(lob => ({
    value: lob,
    label: formatLob(lob),
  }));

  const handleAddProduct = async (data: AddProductFormData) => {
    try {
      await addProduct.mutateAsync({
        carrierId,
        data: {
          name: data.name,
          lineOfBusiness: data.lineOfBusiness,
          code: data.code || data.name.substring(0, 10).toUpperCase().replace(/\s/g, ''),
          description: data.description || undefined,
        },
      });
      addToast({
        title: t('carriers.detail.products.addProduct'),
        description: `${data.name} has been added.`,
        variant: 'success',
      });
      reset();
      setShowAddForm(false);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : 'Failed to add product',
        variant: 'error',
      });
    }
  };

  const handleToggleStatus = async (product: Product) => {
    if (product.isActive) {
      try {
        await deactivateProduct.mutateAsync({
          carrierId,
          productId: product.id,
        });
        addToast({
          title: t('carriers.detail.products.deactivate'),
          description: `${product.name} has been deactivated.`,
          variant: 'success',
        });
      } catch (error) {
        addToast({
          title: t('common.toast.error'),
          description: error instanceof Error ? error.message : 'Failed to deactivate product',
          variant: 'error',
        });
      }
    }
  };

  const columns: ColumnDef<Product>[] = React.useMemo(() => [
    {
      accessorKey: 'name',
      header: () => t('carriers.detail.products.title'),
      cell: ({ row }) => (
        <div className="flex flex-col">
          <span className="font-medium">{row.original.name}</span>
          <span className="text-xs text-muted-foreground font-mono">
            {row.original.code}
          </span>
        </div>
      ),
    },
    {
      accessorKey: 'lineOfBusiness',
      header: () => t('carriers.detail.products.lineOfBusiness'),
      cell: ({ getValue }) => formatLob(getValue<string>()),
    },
    {
      accessorKey: 'minimumPremium',
      header: () => t('carriers.detail.products.minPremium'),
      cell: ({ getValue }) => {
        const value = getValue<number>();
        return value
          ? new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value)
          : '—';
      },
    },
    {
      accessorKey: 'isActive',
      header: () => t('common.table.status'),
      cell: ({ getValue }) => (
        <Badge variant={getValue<boolean>() ? 'default' : 'secondary'}>
          {getValue<boolean>() ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      id: 'actions',
      cell: ({ row }) => {
        const product = row.original;
        return (
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => handleToggleStatus(product)}
            title={product.isActive ? t('carriers.detail.products.deactivate') : t('carriers.detail.products.activate')}
          >
            {product.isActive ? (
              <PowerOff className="h-4 w-4 text-destructive" />
            ) : (
              <Power className="h-4 w-4" />
            )}
          </Button>
        );
      },
    },
  ], [t, handleToggleStatus]);

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>{t('carriers.detail.products.title')}</CardTitle>
        {!showAddForm && (
          <Button variant="outline" size="sm" onClick={() => setShowAddForm(true)}>
            <Plus className="mr-2 h-4 w-4" />
            {t('carriers.detail.products.addProduct')}
          </Button>
        )}
      </CardHeader>
      <CardContent>
        {showAddForm && (
          <form onSubmit={handleSubmit(handleAddProduct)} className="mb-6 space-y-4 rounded-lg border p-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="productName">{t('carriers.detail.products.name')} *</Label>
                <Input
                  id="productName"
                  {...register('name')}
                  error={errors.name?.message}
                  placeholder="General Liability"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="lineOfBusiness">{t('carriers.detail.products.lineOfBusiness')} *</Label>
                <Select
                  id="lineOfBusiness"
                  options={lobSelectOptions}
                  value={selectedLob}
                  onChange={(value) => setValue('lineOfBusiness', value as typeof lineOfBusinessOptions[number])}
                  placeholder="Select line of business"
                />
                {errors.lineOfBusiness && (
                  <p className="text-sm text-destructive">{errors.lineOfBusiness.message}</p>
                )}
              </div>
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="productCode">{t('carriers.detail.products.code')}</Label>
                <Input
                  id="productCode"
                  {...register('code')}
                  placeholder="Auto-generated if empty"
                  className="uppercase"
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="description"
                {...register('description')}
                placeholder="Optional description"
                rows={2}
              />
            </div>
            <div className="flex justify-end gap-3">
              <Button
                type="button"
                variant="outline"
                onClick={() => {
                  reset();
                  setShowAddForm(false);
                }}
              >
                {t('common.actions.cancel')}
              </Button>
              <Button type="submit" disabled={addProduct.isPending}>
                {addProduct.isPending ? t('carriers.detail.products.adding') : t('carriers.detail.products.addProduct')}
              </Button>
            </div>
          </form>
        )}

        <DataTable
          columns={columns}
          data={products}
          emptyMessage={t('carriers.detail.products.noProducts')}
          getRowId={(row) => row.id}
        />
      </CardContent>
    </Card>
  );
}
