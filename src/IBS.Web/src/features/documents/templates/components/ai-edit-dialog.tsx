import * as React from 'react';
import { Wand2, Loader2, Check, X, Edit } from 'lucide-react';
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
import { useAIEditTemplate, useUpdateDocumentTemplate, useDocumentTemplate } from '@/hooks/use-documents';
import { Skeleton } from '@/components/ui/skeleton';
import type { DocumentTemplateListItem } from '@/types/api';

/** Step in the AI edit flow. */
type Step = 'input' | 'loading' | 'preview';

interface AIEditDialogProps {
  /** The template to edit. */
  template: DocumentTemplateListItem;
  /** Whether the dialog is open. */
  open: boolean;
  /** Callback to close the dialog. */
  onClose: () => void;
  /**
   * Called when "Edit Manually" is clicked, passing the AI-modified content so the
   * caller can open the standard template editor pre-populated with it.
   */
  onEditManually?: (modifiedContent: string) => void;
}

/**
 * Dialog for applying natural language AI edits to an existing template.
 *
 * Step 1 – Input: User types a natural language instruction.
 * Step 2 – Loading: AI processes the request.
 * Step 3 – Preview: Side-by-side view of original vs modified template.
 *           • Apply Changes – saves the modified content to the template.
 *           • Reject – goes back to the instruction step.
 *           • Edit Manually – passes the modified content to the parent for manual editing.
 */
export function AIEditDialog({ template, open, onClose, onEditManually }: AIEditDialogProps) {
  const { t } = useTranslation();
  const [step, setStep] = React.useState<Step>('input');
  const [instruction, setInstruction] = React.useState('');
  const [originalContent, setOriginalContent] = React.useState('');
  const [modifiedContent, setModifiedContent] = React.useState('');

  const { data: fullTemplate, isLoading: isLoadingTemplate } = useDocumentTemplate(template.id);
  const aiEditMutation = useAIEditTemplate();
  const updateMutation = useUpdateDocumentTemplate();

  const handleClose = () => {
    setStep('input');
    setInstruction('');
    setOriginalContent('');
    setModifiedContent('');
    onClose();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!instruction.trim()) return;
    setStep('loading');
    try {
      const result = await aiEditMutation.mutateAsync({ id: template.id, request: { instruction } });
      setOriginalContent(result.originalContent);
      setModifiedContent(result.modifiedContent);
      setStep('preview');
    } catch {
      setStep('input');
    }
  };

  const handleApply = async () => {
    await updateMutation.mutateAsync({
      id: template.id,
      request: { name: template.name, description: template.description, content: modifiedContent },
    });
    handleClose();
  };

  const handleReject = () => {
    setStep('input');
    setOriginalContent('');
    setModifiedContent('');
  };

  const handleEditManually = () => {
    onEditManually?.(modifiedContent);
    handleClose();
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className={step === 'preview' ? 'max-w-5xl' : 'max-w-2xl'}>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Wand2 className="h-5 w-5 text-purple-500" />
            {t('documents.templatesPage.aiEditDialog.title', { name: template.name })}
          </DialogTitle>
          <DialogDescription>
            {step === 'input' && t('documents.templatesPage.aiEditDialog.stepInput')}
            {step === 'loading' && t('documents.templatesPage.aiEditDialog.stepLoading')}
            {step === 'preview' && t('documents.templatesPage.aiEditDialog.stepPreview')}
          </DialogDescription>
        </DialogHeader>

        {/* Step 1 – Instruction Input */}
        {step === 'input' && (
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <p className="text-xs font-medium text-muted-foreground mb-1">{t('documents.templatesPage.aiEditDialog.currentTemplate')}</p>
              {isLoadingTemplate ? (
                <Skeleton className="w-full h-48" />
              ) : (
                <iframe
                  srcDoc={fullTemplate?.content ?? ''}
                  className="w-full h-48 border rounded bg-white"
                  title="Current template"
                  sandbox="allow-scripts"
                />
              )}
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('documents.templatesPage.aiEditDialog.instruction')}</label>
              <textarea
                className="w-full border rounded p-3 text-sm h-28 resize-none focus:outline-none focus:ring-1 focus:ring-ring"
                value={instruction}
                onChange={e => setInstruction(e.target.value)}
                placeholder={t('documents.templatesPage.aiEditDialog.instructionPlaceholder')}
                required
                autoFocus
              />
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={handleClose}>{t('common.actions.cancel')}</Button>
              <Button type="submit" disabled={!instruction.trim()}>
                <Wand2 className="h-4 w-4 mr-2" />
                {t('documents.templatesPage.aiEditDialog.applyWithAi')}
              </Button>
            </DialogFooter>
          </form>
        )}

        {/* Step 2 – Loading */}
        {step === 'loading' && (
          <div className="flex flex-col items-center justify-center py-12 gap-4">
            <Loader2 className="h-10 w-10 animate-spin text-purple-500" />
            <p className="text-sm text-muted-foreground">{t('documents.templatesPage.aiEditDialog.modifying')}</p>
            <p className="text-xs text-muted-foreground">{t('documents.templatesPage.aiEditDialog.takesAMinute')}</p>
          </div>
        )}

        {/* Step 3 – Preview */}
        {step === 'preview' && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4 h-96">
              <div className="flex flex-col gap-1">
                <p className="text-xs font-medium text-muted-foreground">{t('documents.templatesPage.aiEditDialog.original')}</p>
                <iframe
                  srcDoc={originalContent}
                  className="w-full flex-1 border rounded bg-white"
                  title="Original template"
                  sandbox="allow-scripts"
                />
              </div>
              <div className="flex flex-col gap-1">
                <p className="text-xs font-medium text-purple-600">{t('documents.templatesPage.aiEditDialog.modifiedByAi')}</p>
                <iframe
                  srcDoc={modifiedContent}
                  className="w-full flex-1 border rounded border-purple-300 bg-white"
                  title="AI-modified template"
                  sandbox="allow-scripts"
                />
              </div>
            </div>
            <DialogFooter className="flex-wrap gap-2">
              <Button variant="outline" onClick={handleReject}>
                <X className="h-4 w-4 mr-2" />
                {t('documents.templatesPage.aiEditDialog.reject')}
              </Button>
              <Button variant="outline" onClick={handleEditManually}>
                <Edit className="h-4 w-4 mr-2" />
                {t('documents.templatesPage.aiEditDialog.editManually')}
              </Button>
              <Button onClick={handleApply} disabled={updateMutation.isPending}>
                {updateMutation.isPending ? (
                  <><Loader2 className="h-4 w-4 mr-2 animate-spin" />{t('documents.templatesPage.aiEditDialog.saving')}</>
                ) : (
                  <><Check className="h-4 w-4 mr-2" />{t('documents.templatesPage.aiEditDialog.applyChanges')}</>
                )}
              </Button>
            </DialogFooter>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
