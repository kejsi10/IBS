import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { DataTable } from '@/components/common/data-table';
import { useToast } from '@/components/ui/toast';
import { useAddCoverage, useRemoveCoverage } from '@/hooks/use-policies';
import { addCoverageSchema, type AddCoverageFormData } from '@/lib/validations/policy';
import type { Coverage, Policy } from '@/types/api';
import type { ColumnDef } from '@tanstack/react-table';

/**
 * Props for the PolicyCoveragesTab component.
 */
export interface PolicyCoveragesTabProps {
  policy: Policy;
}

/**
 * Coverages tab for policy detail page.
 */
export function PolicyCoveragesTab({ policy }: PolicyCoveragesTabProps) {
  const { t } = useTranslation();
  const [showAddForm, setShowAddForm] = React.useState(false);
  const { addToast } = useToast();
  const addCoverage = useAddCoverage();
  const removeCoverage = useRemoveCoverage();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AddCoverageFormData>({
    resolver: zodResolver(addCoverageSchema),
    defaultValues: {
      type: '',
      limit: 0,
      deductible: 0,
      premium: 0,
      description: '',
    },
  });

  const canEdit = policy.status === 'Draft' || policy.status === 'Bound';

  const handleAddCoverage = async (data: AddCoverageFormData) => {
    try {
      await addCoverage.mutateAsync({
        policyId: policy.id,
        data: {
          code: data.type.substring(0, 10).toUpperCase().replace(/\s/g, ''),
          name: data.type,
          premiumAmount: data.premium,
          limitAmount: data.limit,
          deductibleAmount: data.deductible,
          description: data.description,
        },
      });
      addToast({
        title: t('policies.detail.coverages.coverageAdded'),
        description: t('policies.detail.coverages.coverageAddedDesc', { name: data.type }),
        variant: 'success',
      });
      reset();
      setShowAddForm(false);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('policies.detail.coverages.addError'),
        variant: 'error',
      });
    }
  };

  const handleRemove = async (coverage: Coverage) => {
    try {
      await removeCoverage.mutateAsync({
        policyId: policy.id,
        coverageId: coverage.id,
      });
      addToast({
        title: t('policies.detail.coverages.coverageRemoved'),
        description: t('policies.detail.coverages.coverageRemovedDesc', { name: coverage.name }),
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('policies.detail.coverages.removeError'),
        variant: 'error',
      });
    }
  };

  const columns: ColumnDef<Coverage>[] = [
    {
      accessorKey: 'name',
      header: t('policies.detail.coverages.type'),
      cell: ({ row }) => (
        <div className="flex flex-col">
          <span className="font-medium">{row.original.name}</span>
          <span className="font-mono text-xs text-muted-foreground">
            {row.original.code}
          </span>
        </div>
      ),
    },
    {
      accessorKey: 'limitAmount',
      header: t('policies.detail.coverages.limit'),
      cell: ({ row }) => {
        const limit = row.original.limitAmount || row.original.aggregateLimit;
        return limit
          ? new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(limit)
          : '—';
      },
    },
    {
      accessorKey: 'deductibleAmount',
      header: t('policies.detail.coverages.deductible'),
      cell: ({ getValue }) => {
        const value = getValue<number>();
        return value
          ? new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value)
          : '—';
      },
    },
    {
      accessorKey: 'premiumAmount',
      header: t('policies.detail.coverages.premium'),
      cell: ({ getValue }) =>
        new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
          getValue<number>()
        ),
    },
    {
      accessorKey: 'isActive',
      header: t('policies.detail.coverages.coverageStatusHeader'),
      cell: ({ getValue }) => (
        <Badge variant={getValue<boolean>() ? 'default' : 'secondary'}>
          {getValue<boolean>() ? t('policies.detail.coverages.statusActive') : t('policies.detail.coverages.statusInactive')}
        </Badge>
      ),
    },
    ...(canEdit
      ? [
          {
            id: 'actions',
            cell: ({ row }: { row: { original: Coverage } }) => (
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-destructive"
                onClick={() => handleRemove(row.original)}
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            ),
          } as ColumnDef<Coverage>,
        ]
      : []),
  ];

  const totalPremium = policy.coverages.reduce((sum, c) => sum + c.premiumAmount, 0);

  return (
    <div className="space-y-6">
      {/* Add Coverage Form */}
      {canEdit && showAddForm && (
        <Card>
          <CardHeader>
            <CardTitle>{t('policies.detail.coverages.addCoverage')}</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(handleAddCoverage)} className="space-y-4">
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2 sm:col-span-2">
                  <Label htmlFor="type">{t('policies.detail.coverages.type')} *</Label>
                  <Input
                    id="type"
                    {...register('type')}
                    error={errors.type?.message}
                    placeholder={t('policies.detail.coverages.typePlaceholder')}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="limit">{t('policies.detail.coverages.limit')} *</Label>
                  <Input
                    id="limit"
                    type="number"
                    {...register('limit', { valueAsNumber: true })}
                    error={errors.limit?.message}
                    placeholder="1000000"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="deductible">{t('policies.detail.coverages.deductible')}</Label>
                  <Input
                    id="deductible"
                    type="number"
                    {...register('deductible', { valueAsNumber: true })}
                    placeholder="1000"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="premium">{t('policies.detail.coverages.premium')} *</Label>
                  <Input
                    id="premium"
                    type="number"
                    {...register('premium', { valueAsNumber: true })}
                    error={errors.premium?.message}
                    placeholder="5000"
                  />
                </div>
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
                <Button type="submit" disabled={addCoverage.isPending}>
                  {addCoverage.isPending ? t('policies.detail.coverages.adding') : t('policies.detail.coverages.addCoverage')}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      {/* Coverages Table */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>{t('policies.detail.coverages.policyCoverages')}</CardTitle>
          {canEdit && !showAddForm && (
            <Button variant="outline" size="sm" onClick={() => setShowAddForm(true)}>
              <Plus className="mr-2 h-4 w-4" />
              {t('policies.detail.coverages.addCoverage')}
            </Button>
          )}
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={policy.coverages}
            emptyMessage={t('policies.detail.coverages.noCoverages')}
            getRowId={(row) => row.id}
          />

          {policy.coverages.length > 0 && (
            <div className="mt-4 flex items-center justify-between border-t pt-4">
              <span className="font-medium">{t('policies.detail.coverages.totalPremium')}</span>
              <span className="text-xl font-bold">
                {new Intl.NumberFormat('en-US', {
                  style: 'currency',
                  currency: policy.currency || 'USD',
                }).format(totalPremium)}
              </span>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
