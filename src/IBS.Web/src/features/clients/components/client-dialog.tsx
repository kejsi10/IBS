import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { useToast } from '@/components/ui/toast';
import {
  IndividualClientForm,
  BusinessClientForm,
  ClientTypeSelector,
} from './client-form';
import { useCreateIndividualClient, useCreateBusinessClient } from '@/hooks/use-clients';
import type { ClientType } from '@/types/api';
import type { CreateIndividualClientFormData, CreateBusinessClientFormData } from '@/lib/validations/client';

/**
 * Props for the ClientDialog component.
 */
export interface ClientDialogProps {
  /** Whether the dialog is open */
  open: boolean;
  /** Callback when open state changes */
  onOpenChange: (open: boolean) => void;
}

/**
 * Dialog for creating a new client.
 * Supports both individual and business client types.
 */
export function ClientDialog({ open, onOpenChange }: ClientDialogProps) {
  const [clientType, setClientType] = React.useState<ClientType>('Individual');
  const [step, setStep] = React.useState<'type' | 'form'>('type');
  const navigate = useNavigate();
  const { addToast } = useToast();
  const { t } = useTranslation();

  const createIndividual = useCreateIndividualClient();
  const createBusiness = useCreateBusinessClient();

  const handleClose = () => {
    onOpenChange(false);
    // Reset state after animation
    setTimeout(() => {
      setStep('type');
      setClientType('Individual');
    }, 200);
  };

  const handleTypeSelect = (type: ClientType) => {
    setClientType(type);
    setStep('form');
  };

  const handleIndividualSubmit = async (data: CreateIndividualClientFormData) => {
    try {
      const result = await createIndividual.mutateAsync({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        phone: data.phone || undefined,
        dateOfBirth: data.dateOfBirth || undefined,
      });

      addToast({
        title: t('clients.toast.created'),
        description: t('clients.toast.createdDesc', { name: `${data.firstName} ${data.lastName}` }),
        variant: 'success',
      });

      handleClose();
      navigate(`/clients/${result.id}`);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('clients.toast.failedCreate'),
        variant: 'error',
      });
    }
  };

  const handleBusinessSubmit = async (data: CreateBusinessClientFormData) => {
    try {
      const result = await createBusiness.mutateAsync({
        businessName: data.legalName,
        businessType: 'Corporation',
        dbaName: data.dba || undefined,
        email: data.email,
        phone: data.phone || undefined,
      });

      addToast({
        title: t('clients.toast.created'),
        description: t('clients.toast.createdDesc', { name: data.legalName }),
        variant: 'success',
      });

      handleClose();
      navigate(`/clients/${result.id}`);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('clients.toast.failedCreate'),
        variant: 'error',
      });
    }
  };

  const isSubmitting = createIndividual.isPending || createBusiness.isPending;

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {step === 'type' ? t('clients.dialog.addTitle') : t('clients.dialog.typeTitle', { type: clientType })}
          </DialogTitle>
          <DialogDescription>
            {step === 'type'
              ? t('clients.dialog.chooseType')
              : t('clients.dialog.fillInfo')}
          </DialogDescription>
        </DialogHeader>

        {step === 'type' ? (
          <ClientTypeSelector value={clientType} onChange={handleTypeSelect} />
        ) : clientType === 'Individual' ? (
          <IndividualClientForm
            onSubmit={handleIndividualSubmit}
            onCancel={handleClose}
            isSubmitting={isSubmitting}
          />
        ) : (
          <BusinessClientForm
            onSubmit={handleBusinessSubmit}
            onCancel={handleClose}
            isSubmitting={isSubmitting}
          />
        )}

        {step === 'form' && (
          <button
            type="button"
            onClick={() => setStep('type')}
            className="text-sm text-muted-foreground hover:text-foreground"
          >
            {t('clients.dialog.backToType')}
          </button>
        )}
      </DialogContent>
    </Dialog>
  );
}
