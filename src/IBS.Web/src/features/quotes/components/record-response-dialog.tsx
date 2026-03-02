import * as React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import { useToast } from '@/components/ui/toast';
import { useRecordQuoteResponse } from '@/hooks';
import {
  recordQuotedResponseSchema,
  recordDeclinedResponseSchema,
  type RecordDeclinedResponseFormData,
} from '@/lib/validations';

/**
 * Props for the RecordResponseDialog component.
 */
interface RecordResponseDialogProps {
  /** Whether the dialog is open. */
  open: boolean;
  /** Handler to close the dialog. */
  onClose: () => void;
  /** The quote ID. */
  quoteId: string;
  /** The quote carrier ID. */
  quoteCarrierId: string;
  /** The carrier display name. */
  carrierName: string;
}

/**
 * Dialog for recording a carrier's response (quoted or declined).
 */
export function RecordResponseDialog({
  open,
  onClose,
  quoteId,
  quoteCarrierId,
  carrierName,
}: RecordResponseDialogProps) {
  const { t } = useTranslation();
  const { addToast } = useToast();
  const recordResponse = useRecordQuoteResponse();
  const [responseType, setResponseType] = React.useState<'quoted' | 'declined'>('quoted');

  const quotedForm = useForm({
    resolver: zodResolver(recordQuotedResponseSchema),
    defaultValues: {
      premiumAmount: 0,
      premiumCurrency: 'USD',
      conditions: '',
      proposedCoverages: '',
      carrierExpiresAt: '',
    },
  });

  const declinedForm = useForm<RecordDeclinedResponseFormData>({
    resolver: zodResolver(recordDeclinedResponseSchema),
    defaultValues: {
      declinationReason: '',
    },
  });

  /** Resets forms when dialog closes. */
  React.useEffect(() => {
    if (!open) {
      quotedForm.reset();
      declinedForm.reset();
      setResponseType('quoted');
    }
  }, [open, quotedForm, declinedForm]);

  /** Handles submitting a quoted response. */
  const handleQuotedSubmit = async (data: Record<string, unknown>) => {
    try {
      await recordResponse.mutateAsync({
        quoteId,
        quoteCarrierId,
        data: {
          isQuoted: true,
          premiumAmount: data.premiumAmount as number,
          premiumCurrency: (data.premiumCurrency as string) || 'USD',
          conditions: (data.conditions as string) || undefined,
          proposedCoverages: (data.proposedCoverages as string) || undefined,
          carrierExpiresAt: (data.carrierExpiresAt as string) || undefined,
        },
      });
      addToast({
        title: 'Response recorded',
        description: `${carrierName} quoted response has been recorded.`,
        variant: 'success',
      });
      onClose();
    } catch (err) {
      addToast({
        title: 'Error',
        description: err instanceof Error ? err.message : 'Failed to record response',
        variant: 'error',
      });
    }
  };

  /** Handles submitting a declined response. */
  const handleDeclinedSubmit = async (data: RecordDeclinedResponseFormData) => {
    try {
      await recordResponse.mutateAsync({
        quoteId,
        quoteCarrierId,
        data: {
          isQuoted: false,
          declinationReason: data.declinationReason || undefined,
        },
      });
      addToast({
        title: 'Response recorded',
        description: `${carrierName} has declined.`,
        variant: 'success',
      });
      onClose();
    } catch (err) {
      addToast({
        title: 'Error',
        description: err instanceof Error ? err.message : 'Failed to record response',
        variant: 'error',
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>{t('quotes.detail.recordResponseDialog.title')}</DialogTitle>
          <DialogDescription>
            {t('quotes.detail.recordResponseDialog.description', { carrierName })}
          </DialogDescription>
        </DialogHeader>

        {/* Response Type Toggle */}
        <div className="flex gap-2">
          <Button
            variant={responseType === 'quoted' ? 'default' : 'outline'}
            size="sm"
            onClick={() => setResponseType('quoted')}
            className="flex-1"
          >
            {t('quotes.detail.quoted')}
          </Button>
          <Button
            variant={responseType === 'declined' ? 'default' : 'outline'}
            size="sm"
            onClick={() => setResponseType('declined')}
            className="flex-1"
          >
            {t('quotes.detail.declined')}
          </Button>
        </div>

        {/* Quoted Form */}
        {responseType === 'quoted' && (
          <form onSubmit={quotedForm.handleSubmit(handleQuotedSubmit)} className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">{t('quotes.detail.recordResponseDialog.premiumAmount')} *</label>
              <Input
                type="number"
                step="0.01"
                min="0.01"
                {...quotedForm.register('premiumAmount', { valueAsNumber: true })}
              />
              {quotedForm.formState.errors.premiumAmount && (
                <p className="text-sm text-destructive">
                  {quotedForm.formState.errors.premiumAmount.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">{t('quotes.detail.recordResponseDialog.currency')}</label>
              <Input {...quotedForm.register('premiumCurrency')} maxLength={3} />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">{t('quotes.detail.recordResponseDialog.conditions')}</label>
              <Textarea
                {...quotedForm.register('conditions')}
                placeholder="Any conditions or exclusions..."
                rows={3}
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">{t('quotes.detail.recordResponseDialog.quoteExpires')}</label>
              <Input type="date" {...quotedForm.register('carrierExpiresAt')} />
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={onClose}>
                {t('common.actions.cancel')}
              </Button>
              <Button type="submit" disabled={recordResponse.isPending}>
                {recordResponse.isPending
                  ? t('quotes.detail.recordResponseDialog.saving')
                  : t('quotes.detail.recordResponseDialog.recordQuoted')}
              </Button>
            </DialogFooter>
          </form>
        )}

        {/* Declined Form */}
        {responseType === 'declined' && (
          <form onSubmit={declinedForm.handleSubmit(handleDeclinedSubmit)} className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">{t('quotes.detail.recordResponseDialog.declinationReason')}</label>
              <Textarea
                {...declinedForm.register('declinationReason')}
                placeholder="Optional reason..."
                rows={3}
              />
              {declinedForm.formState.errors.declinationReason && (
                <p className="text-sm text-destructive">
                  {declinedForm.formState.errors.declinationReason.message}
                </p>
              )}
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={onClose}>
                {t('common.actions.cancel')}
              </Button>
              <Button type="submit" disabled={recordResponse.isPending}>
                {recordResponse.isPending
                  ? t('quotes.detail.recordResponseDialog.saving')
                  : t('quotes.detail.recordResponseDialog.recordDeclined')}
              </Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  );
}
