import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Star, Trash2, MapPin } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Select, type SelectOption } from '@/components/ui/select';
import { useToast } from '@/components/ui/toast';
import { useAddAddress, useSetPrimaryAddress, useRemoveAddress } from '@/hooks/use-clients';
import { addAddressSchema, type AddAddressFormData } from '@/lib/validations/client';
import type { Address } from '@/types/api';


/**
 * Props for the ClientAddressesTab component.
 */
export interface ClientAddressesTabProps {
  clientId: string;
  addresses: Address[];
}

/**
 * Addresses tab with inline add form and address management.
 */
export function ClientAddressesTab({ clientId, addresses }: ClientAddressesTabProps) {
  const [showAddForm, setShowAddForm] = React.useState(false);
  const { t } = useTranslation();
  const { addToast } = useToast();

  const addressTypeOptions: SelectOption[] = [
    { value: 'Mailing', label: t('clients.detail.addresses.types.mailing') },
    { value: 'Physical', label: t('clients.detail.addresses.types.physical') },
    { value: 'Billing', label: t('clients.detail.addresses.types.billing') },
  ];
  const addAddress = useAddAddress();
  const setPrimaryAddress = useSetPrimaryAddress();
  const removeAddress = useRemoveAddress();

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<AddAddressFormData>({
    resolver: zodResolver(addAddressSchema),
    defaultValues: {
      type: 'Mailing',
      line1: '',
      line2: '',
      city: '',
      state: '',
      postalCode: '',
      country: 'USA',
      isPrimary: false,
    },
  });

  const addressType = watch('type');

  const handleAddAddress = async (data: AddAddressFormData) => {
    try {
      await addAddress.mutateAsync({
        clientId,
        data: {
          type: data.type as 'Mailing' | 'Physical' | 'Billing',
          line1: data.line1,
          line2: data.line2 || undefined,
          city: data.city,
          state: data.state,
          postalCode: data.postalCode,
          country: data.country,
          isPrimary: data.isPrimary,
        },
      });
      addToast({
        title: t('clients.detail.addresses.addAddress'),
        description: t('clients.toast.addressAdded'),
        variant: 'success',
      });
      reset();
      setShowAddForm(false);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('clients.toast.addressAddFailed'),
        variant: 'error',
      });
    }
  };

  const handleSetPrimary = async (address: Address) => {
    try {
      await setPrimaryAddress.mutateAsync({
        clientId,
        addressId: address.id,
      });
      addToast({
        title: t('clients.toast.primaryAddressUpdated'),
        description: t('clients.toast.primaryAddressUpdatedDesc'),
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('clients.toast.addressUpdateFailed'),
        variant: 'error',
      });
    }
  };

  const handleRemove = async (address: Address) => {
    try {
      await removeAddress.mutateAsync({
        clientId,
        addressId: address.id,
      });
      addToast({
        title: t('clients.toast.addressRemoved'),
        description: t('clients.toast.addressRemovedDesc'),
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('clients.toast.addressRemoveFailed'),
        variant: 'error',
      });
    }
  };

  const formatAddress = (address: Address) => {
    const parts = [address.line1];
    if (address.line2) parts.push(address.line2);
    parts.push(`${address.city}, ${address.state} ${address.postalCode}`);
    if (address.country !== 'USA') parts.push(address.country);
    return parts;
  };

  return (
    <div className="space-y-6">
      {/* Address Cards */}
      {addresses.length === 0 && !showAddForm ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <p className="text-muted-foreground">{t('clients.detail.addresses.noAddresses')}</p>
            <Button
              variant="outline"
              className="mt-4"
              onClick={() => setShowAddForm(true)}
            >
              <Plus className="mr-2 h-4 w-4" />
              {t('clients.detail.addresses.addAddress')}
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {addresses.map((address) => (
            <Card key={address.id}>
              <CardContent className="pt-6">
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-2">
                    <MapPin className="h-4 w-4 text-muted-foreground" />
                    <Badge variant="outline">{address.type}</Badge>
                    {address.isPrimary && (
                      <Badge variant="secondary" className="text-xs">
                        <Star className="mr-1 h-3 w-3" />
                        {t('clients.detail.addresses.primary')}
                      </Badge>
                    )}
                  </div>
                  <div className="flex gap-1">
                    {!address.isPrimary && (
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-8 w-8"
                        onClick={() => handleSetPrimary(address)}
                        title={t('clients.detail.addresses.setPrimary')}
                      >
                        <Star className="h-4 w-4" />
                      </Button>
                    )}
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-destructive hover:text-destructive"
                      onClick={() => handleRemove(address)}
                      title={t('clients.detail.addresses.remove')}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
                <div className="mt-4 space-y-1 text-sm">
                  {formatAddress(address).map((line, i) => (
                    <p key={i}>{line}</p>
                  ))}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Add Address Form */}
      {showAddForm ? (
        <Card>
          <CardHeader>
            <CardTitle>{t('clients.detail.addresses.addAddress')}</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(handleAddAddress)} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="type">{t('clients.detail.addresses.addressType')} *</Label>
                <Select
                  id="type"
                  options={addressTypeOptions}
                  value={addressType}
                  onChange={(value) => setValue('type', value as 'Mailing' | 'Physical' | 'Billing')}
                  placeholder="Select address type"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="line1">{t('clients.detail.addresses.addressLine1')} *</Label>
                <Input
                  id="line1"
                  {...register('line1')}
                  error={errors.line1?.message}
                  placeholder="123 Main St"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="line2">{t('clients.detail.addresses.addressLine2')}</Label>
                <Input
                  id="line2"
                  {...register('line2')}
                  placeholder="Suite 100"
                />
              </div>

              <div className="grid gap-4 sm:grid-cols-3">
                <div className="space-y-2">
                  <Label htmlFor="city">{t('clients.detail.addresses.city')} *</Label>
                  <Input
                    id="city"
                    {...register('city')}
                    error={errors.city?.message}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="state">{t('clients.detail.addresses.state')} *</Label>
                  <Input
                    id="state"
                    {...register('state')}
                    error={errors.state?.message}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="postalCode">{t('clients.detail.addresses.postalCode')} *</Label>
                  <Input
                    id="postalCode"
                    {...register('postalCode')}
                    error={errors.postalCode?.message}
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
                <Button type="submit" disabled={addAddress.isPending}>
                  {addAddress.isPending ? t('common.form.creating') : t('clients.detail.addresses.addAddress')}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      ) : addresses.length > 0 ? (
        <Button variant="outline" onClick={() => setShowAddForm(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t('clients.detail.addresses.addAddress')}
        </Button>
      ) : null}
    </div>
  );
}
