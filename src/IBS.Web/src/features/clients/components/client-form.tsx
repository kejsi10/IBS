import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { FormSection } from '@/components/common/form-section';
import {
  createIndividualClientSchema,
  createBusinessClientSchema,
  type CreateIndividualClientFormData,
  type CreateBusinessClientFormData,
} from '@/lib/validations/client';
import type { ClientType } from '@/types/api';

/**
 * Props for the individual client form.
 */
export interface IndividualClientFormProps {
  onSubmit: (data: CreateIndividualClientFormData) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
  defaultValues?: Partial<CreateIndividualClientFormData>;
}

/**
 * Form for creating/editing an individual client.
 */
export function IndividualClientForm({
  onSubmit,
  onCancel,
  isSubmitting = false,
  defaultValues,
}: IndividualClientFormProps) {
  const { t } = useTranslation();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateIndividualClientFormData>({
    resolver: zodResolver(createIndividualClientSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      ...defaultValues,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="firstName">{t('clients.form.individual.firstName')} *</Label>
          <Input
            id="firstName"
            {...register('firstName')}
            error={errors.firstName?.message}
            placeholder="John"
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="lastName">{t('clients.form.individual.lastName')} *</Label>
          <Input
            id="lastName"
            {...register('lastName')}
            error={errors.lastName?.message}
            placeholder="Doe"
          />
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="email">{t('clients.form.individual.email')} *</Label>
          <Input
            id="email"
            type="email"
            {...register('email')}
            error={errors.email?.message}
            placeholder="john.doe@example.com"
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="phone">{t('clients.form.individual.phone')}</Label>
          <Input
            id="phone"
            type="tel"
            {...register('phone')}
            error={errors.phone?.message}
            placeholder="(555) 123-4567"
          />
        </div>
      </div>

      <FormSection
        title={t('common.form.additionalDetails')}
        description={t('common.form.optional')}
        collapsible
        defaultExpanded={false}
      >
        <div className="space-y-2">
          <Label htmlFor="dateOfBirth">{t('clients.form.individual.dateOfBirth')}</Label>
          <Input
            id="dateOfBirth"
            type="date"
            {...register('dateOfBirth')}
          />
        </div>
      </FormSection>

      <div className="flex justify-end gap-3">
        <Button type="button" variant="outline" onClick={onCancel}>
          {t('common.actions.cancel')}
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? t('common.form.creating') : t('clients.dialog.create')}
        </Button>
      </div>
    </form>
  );
}

/**
 * Props for the business client form.
 */
export interface BusinessClientFormProps {
  onSubmit: (data: CreateBusinessClientFormData) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
  defaultValues?: Partial<CreateBusinessClientFormData>;
}

/**
 * Form for creating/editing a business client.
 */
export function BusinessClientForm({
  onSubmit,
  onCancel,
  isSubmitting = false,
  defaultValues,
}: BusinessClientFormProps) {
  const { t } = useTranslation();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateBusinessClientFormData>({
    resolver: zodResolver(createBusinessClientSchema),
    defaultValues: {
      legalName: '',
      email: '',
      phone: '',
      ...defaultValues,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="space-y-2">
        <Label htmlFor="legalName">{t('clients.form.business.legalName')} *</Label>
        <Input
          id="legalName"
          {...register('legalName')}
          error={errors.legalName?.message}
          placeholder="Acme Corporation"
        />
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="email">{t('clients.form.business.email')} *</Label>
          <Input
            id="email"
            type="email"
            {...register('email')}
            error={errors.email?.message}
            placeholder="contact@acme.com"
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="phone">{t('clients.form.business.phone')}</Label>
          <Input
            id="phone"
            type="tel"
            {...register('phone')}
            error={errors.phone?.message}
            placeholder="(555) 123-4567"
          />
        </div>
      </div>

      <FormSection
        title={t('common.form.additionalDetails')}
        description={t('clients.form.business.optionalInfo')}
        collapsible
        defaultExpanded={false}
      >
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <Label htmlFor="dba">{t('clients.form.business.dba')}</Label>
            <Input
              id="dba"
              {...register('dba')}
              placeholder="Acme Co"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="taxId">{t('clients.form.business.taxId')}</Label>
            <Input
              id="taxId"
              {...register('taxId')}
              placeholder="XX-XXXXXXX"
            />
          </div>
        </div>
      </FormSection>

      <div className="flex justify-end gap-3">
        <Button type="button" variant="outline" onClick={onCancel}>
          {t('common.actions.cancel')}
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? t('common.form.creating') : t('clients.dialog.create')}
        </Button>
      </div>
    </form>
  );
}

/**
 * Props for the client type selector.
 */
export interface ClientTypeSelectorProps {
  value: ClientType;
  onChange: (type: ClientType) => void;
}

/**
 * Selector for choosing between individual and business client types.
 */
export function ClientTypeSelector({ value, onChange }: ClientTypeSelectorProps) {
  const { t } = useTranslation();
  return (
    <div className="grid grid-cols-2 gap-4">
      <button
        type="button"
        onClick={() => onChange('Individual')}
        className={`rounded-lg border-2 p-4 text-left transition-colors ${
          value === 'Individual'
            ? 'border-primary bg-primary/5'
            : 'border-border hover:border-primary/50'
        }`}
      >
        <div className="font-medium">{t('clients.form.individual.title')}</div>
        <div className="text-sm text-muted-foreground">
          {t('clients.form.individual.description')}
        </div>
      </button>
      <button
        type="button"
        onClick={() => onChange('Business')}
        className={`rounded-lg border-2 p-4 text-left transition-colors ${
          value === 'Business'
            ? 'border-primary bg-primary/5'
            : 'border-border hover:border-primary/50'
        }`}
      >
        <div className="font-medium">{t('clients.form.business.title')}</div>
        <div className="text-sm text-muted-foreground">
          {t('clients.form.business.description')}
        </div>
      </button>
    </div>
  );
}
