import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { FormSection } from '@/components/common/form-section';
import { useToast } from '@/components/ui/toast';
import { useCreateCarrier } from '@/hooks/use-carriers';
import { createCarrierSchema, type CreateCarrierFormData } from '@/lib/validations/carrier';

/**
 * Props for the CarrierDialog component.
 */
export interface CarrierDialogProps {
  /** Whether the dialog is open */
  open: boolean;
  /** Callback when open state changes */
  onOpenChange: (open: boolean) => void;
}

/**
 * Dialog for creating a new carrier.
 */
export function CarrierDialog({ open, onOpenChange }: CarrierDialogProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const createCarrier = useCreateCarrier();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateCarrierFormData>({
    resolver: zodResolver(createCarrierSchema),
    defaultValues: {
      name: '',
      code: '',
      amBestRating: '',
      naicCode: '',
      website: '',
    },
  });

  const handleClose = () => {
    onOpenChange(false);
    setTimeout(() => reset(), 200);
  };

  const onSubmit = async (data: CreateCarrierFormData) => {
    try {
      const result = await createCarrier.mutateAsync({
        name: data.name,
        code: data.code,
        amBestRating: data.amBestRating || undefined,
        naicCode: data.naicCode || undefined,
        websiteUrl: data.website || undefined,
      });

      addToast({
        title: t('carriers.dialog.create'),
        description: `${data.name} has been added.`,
        variant: 'success',
      });

      handleClose();
      navigate(`/carriers/${result.id}`);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : 'Failed to create carrier',
        variant: 'error',
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{t('carriers.dialog.addTitle')}</DialogTitle>
          <DialogDescription>
            {t('carriers.dialog.description')}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2 sm:col-span-2">
              <Label htmlFor="name">{t('carriers.dialog.name')} *</Label>
              <Input
                id="name"
                {...register('name')}
                error={errors.name?.message}
                placeholder="Acme Insurance Company"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="code">{t('carriers.dialog.code')} *</Label>
              <Input
                id="code"
                {...register('code')}
                error={errors.code?.message}
                placeholder="ACME"
                className="uppercase"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="amBestRating">{t('carriers.dialog.amBestRating')}</Label>
              <Input
                id="amBestRating"
                {...register('amBestRating')}
                error={errors.amBestRating?.message}
                placeholder="A+"
              />
            </div>
          </div>

          <FormSection
            title={t('common.form.additionalDetails')}
            description="Optional carrier information"
            collapsible
            defaultExpanded={false}
          >
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="naicCode">{t('carriers.dialog.naicCode')}</Label>
                <Input
                  id="naicCode"
                  {...register('naicCode')}
                  placeholder="12345"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="website">{t('carriers.dialog.website')}</Label>
                <Input
                  id="website"
                  type="url"
                  {...register('website')}
                  error={errors.website?.message}
                  placeholder="https://example.com"
                />
              </div>
            </div>
          </FormSection>

          <div className="flex justify-end gap-3">
            <Button type="button" variant="outline" onClick={handleClose}>
              {t('common.actions.cancel')}
            </Button>
            <Button type="submit" disabled={createCarrier.isPending}>
              {createCarrier.isPending ? t('carriers.dialog.creating') : t('carriers.dialog.create')}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
