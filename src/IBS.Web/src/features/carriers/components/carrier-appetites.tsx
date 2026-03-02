import * as React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Trash2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Select, type SelectOption } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { Checkbox } from '@/components/ui/checkbox';
import { useToast } from '@/components/ui/toast';
import { useAddAppetite, useDeactivateAppetite } from '@/hooks/use-carriers';
import { addAppetiteSchema, lineOfBusinessOptions, type AddAppetiteFormData } from '@/lib/validations/carrier';
import type { Appetite } from '@/types/api';

/**
 * Props for the CarrierAppetites component.
 */
export interface CarrierAppetitesProps {
  carrierId: string;
  appetites: Appetite[];
}

/**
 * Formats line of business for display.
 */
function formatLob(lob: string): string {
  return lob.replace(/([A-Z])/g, ' $1').trim();
}

/**
 * Appetites management component for carrier detail page.
 */
export function CarrierAppetites({ carrierId, appetites }: CarrierAppetitesProps) {
  const [showAddForm, setShowAddForm] = React.useState(false);
  const { t } = useTranslation();
  const { addToast } = useToast();
  const addAppetite = useAddAppetite();
  const deactivateAppetite = useDeactivateAppetite();

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<AddAppetiteFormData>({
    resolver: zodResolver(addAppetiteSchema),
    defaultValues: {
      lineOfBusiness: undefined,
      isPreferred: false,
      minimumPremium: undefined,
      maximumPremium: undefined,
      targetIndustries: '',
      excludedIndustries: '',
      notes: '',
    },
  });

  const selectedLob = watch('lineOfBusiness');
  const isPreferred = watch('isPreferred');

  const lobSelectOptions: SelectOption[] = lineOfBusinessOptions.map(lob => ({
    value: lob,
    label: formatLob(lob),
  }));

  const handleAddAppetite = async (data: AddAppetiteFormData) => {
    try {
      await addAppetite.mutateAsync({
        carrierId,
        data: {
          lineOfBusiness: data.lineOfBusiness,
          states: 'All', // Default to all states
          minAnnualRevenue: data.minimumPremium,
          maxAnnualRevenue: data.maximumPremium,
          acceptedIndustries: data.targetIndustries || undefined,
          excludedIndustries: data.excludedIndustries || undefined,
        },
      });
      addToast({
        title: t('carriers.detail.appetites.addAppetite'),
        description: `${formatLob(data.lineOfBusiness)} appetite has been added.`,
        variant: 'success',
      });
      reset();
      setShowAddForm(false);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : 'Failed to add appetite',
        variant: 'error',
      });
    }
  };

  const handleRemove = async (appetite: Appetite) => {
    try {
      await deactivateAppetite.mutateAsync({
        carrierId,
        appetiteId: appetite.id,
      });
      addToast({
        title: t('common.toast.success'),
        description: 'The appetite criteria has been removed.',
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : 'Failed to remove appetite',
        variant: 'error',
      });
    }
  };

  const activeAppetites = appetites.filter(a => a.isActive);

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>{t('carriers.detail.appetites.title')}</CardTitle>
        {!showAddForm && (
          <Button variant="outline" size="sm" onClick={() => setShowAddForm(true)}>
            <Plus className="mr-2 h-4 w-4" />
            {t('carriers.detail.appetites.addAppetite')}
          </Button>
        )}
      </CardHeader>
      <CardContent>
        {showAddForm && (
          <form onSubmit={handleSubmit(handleAddAppetite)} className="mb-6 space-y-4 rounded-lg border p-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="appetiteLob">{t('carriers.detail.appetites.lineOfBusiness')} *</Label>
                <Select
                  id="appetiteLob"
                  options={lobSelectOptions}
                  value={selectedLob}
                  onChange={(value) => setValue('lineOfBusiness', value as typeof lineOfBusinessOptions[number])}
                  placeholder="Select line of business"
                />
                {errors.lineOfBusiness && (
                  <p className="text-sm text-destructive">{errors.lineOfBusiness.message}</p>
                )}
              </div>
              <div className="flex items-center space-x-2 pt-8">
                <Checkbox
                  id="isPreferred"
                  checked={isPreferred}
                  onChange={(e) => setValue('isPreferred', e.target.checked)}
                  label="Preferred line of business"
                />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="minPremium">{t('carriers.detail.appetites.minPremium')}</Label>
                <Input
                  id="minPremium"
                  type="number"
                  {...register('minimumPremium', { valueAsNumber: true })}
                  placeholder="0"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="maxPremium">{t('carriers.detail.appetites.maxPremium')}</Label>
                <Input
                  id="maxPremium"
                  type="number"
                  {...register('maximumPremium', { valueAsNumber: true })}
                  placeholder="Unlimited"
                />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="targetIndustries">{t('carriers.detail.appetites.targetIndustries')}</Label>
                <Textarea
                  id="targetIndustries"
                  {...register('targetIndustries')}
                  placeholder="Construction, Manufacturing, ..."
                  rows={2}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="excludedIndustries">{t('carriers.detail.appetites.excludedIndustries')}</Label>
                <Textarea
                  id="excludedIndustries"
                  {...register('excludedIndustries')}
                  placeholder="Cannabis, Fireworks, ..."
                  rows={2}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="notes">Notes</Label>
              <Textarea
                id="notes"
                {...register('notes')}
                placeholder="Additional underwriting notes..."
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
              <Button type="submit" disabled={addAppetite.isPending}>
                {addAppetite.isPending ? t('carriers.detail.appetites.adding') : t('carriers.detail.appetites.addAppetite')}
              </Button>
            </div>
          </form>
        )}

        {activeAppetites.length === 0 ? (
          <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
            {t('carriers.detail.appetites.noAppetites')}
          </div>
        ) : (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {activeAppetites.map((appetite) => (
              <Card key={appetite.id}>
                <CardContent className="pt-6">
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-2">
                      <Badge variant="outline">{formatLob(appetite.lineOfBusiness)}</Badge>
                      {/* Note: API doesn't have isPreferred but we'd show it here */}
                    </div>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-destructive hover:text-destructive"
                      onClick={() => handleRemove(appetite)}
                      title="Remove"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>

                  <div className="mt-4 space-y-2 text-sm">
                    <div>
                      <span className="text-muted-foreground">States: </span>
                      <span>{appetite.states || 'All'}</span>
                    </div>
                    {(appetite.minAnnualRevenue || appetite.maxAnnualRevenue) && (
                      <div>
                        <span className="text-muted-foreground">Premium Range: </span>
                        <span>
                          {appetite.minAnnualRevenue
                            ? `$${appetite.minAnnualRevenue.toLocaleString()}`
                            : '$0'}
                          {' - '}
                          {appetite.maxAnnualRevenue
                            ? `$${appetite.maxAnnualRevenue.toLocaleString()}`
                            : 'Unlimited'}
                        </span>
                      </div>
                    )}
                    {appetite.acceptedIndustries && (
                      <div>
                        <span className="text-muted-foreground">Target: </span>
                        <span>{appetite.acceptedIndustries}</span>
                      </div>
                    )}
                    {appetite.excludedIndustries && (
                      <div>
                        <span className="text-muted-foreground">Excluded: </span>
                        <span className="text-destructive">{appetite.excludedIndustries}</span>
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
