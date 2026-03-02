import * as React from 'react';
import { FileDown } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import { Autocomplete } from '@/components/common/autocomplete';
import { useGenerateCOI } from '@/hooks/use-documents';
import { documentsService } from '@/services';
import { openSafeUrl } from '@/lib/utils';
import { usePolicies } from '@/hooks/use-policies';
import type { DocumentTemplateListItem } from '@/types/api';

/**
 * Props for GenerateCOIDialog.
 */
interface GenerateCOIDialogProps {
  /** Whether the dialog is open. */
  open: boolean;
  /** Callback to close the dialog. */
  onClose: () => void;
  /** The template to generate a COI from. */
  template: DocumentTemplateListItem;
}

/**
 * Dialog for generating a Certificate of Insurance PDF from a template and a searched policy.
 * On success, automatically opens the download URL in a new tab.
 */
export function GenerateCOIDialog({ open, onClose, template }: GenerateCOIDialogProps) {
  const { t } = useTranslation();
  const [policyId, setPolicyId] = React.useState('');
  const [search, setSearch] = React.useState('');
  const generateMutation = useGenerateCOI();

  const { data: policies, isLoading } = usePolicies({ search, pageSize: 20 });

  const policyOptions = React.useMemo(
    () =>
      policies?.items.map(p => ({
        value: p.id,
        label: p.policyNumber,
        description: p.clientName ?? undefined,
      })) ?? [],
    [policies]
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const result = await generateMutation.mutateAsync({
      templateId: template.id,
      policyId,
    });
    // Fetch the authenticated download URL, then open it in a new tab (same-origin enforced)
    const downloadUrl = await documentsService.getDownloadUrl(result.id);
    openSafeUrl(downloadUrl);
    onClose();
    setPolicyId('');
    setSearch('');
  };

  const handleOpenChange = (isOpen: boolean) => {
    if (!isOpen) {
      onClose();
      setPolicyId('');
      setSearch('');
      generateMutation.reset();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('documents.templatesPage.coi.title')}</DialogTitle>
          <DialogDescription>
            {t('documents.templatesPage.coi.description', { name: template.name })}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">{t('documents.templatesPage.coi.selectPolicy')}</label>
            <Autocomplete
              value={policyId || undefined}
              onChange={v => setPolicyId(v ?? '')}
              options={policyOptions}
              onSearch={setSearch}
              placeholder={t('documents.templatesPage.coi.searchPolicy')}
              searchPlaceholder={t('documents.templatesPage.coi.searchPolicy')}
              isLoading={isLoading}
              emptyMessage={t('documents.templatesPage.coi.noPolicies')}
            />
            <p className="text-xs text-muted-foreground mt-1">
              {t('documents.templatesPage.coi.policyHint')}
            </p>
          </div>

          {generateMutation.isError && (
            <p className="text-sm text-red-600">
              {t('documents.templatesPage.coi.generateError')}
            </p>
          )}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              {t('common.actions.cancel')}
            </Button>
            <Button type="submit" disabled={!policyId || generateMutation.isPending}>
              {generateMutation.isPending ? (
                t('documents.templatesPage.coi.generating')
              ) : (
                <>
                  <FileDown className="h-4 w-4 mr-2" />
                  {t('documents.templatesPage.coi.generate')}
                </>
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
