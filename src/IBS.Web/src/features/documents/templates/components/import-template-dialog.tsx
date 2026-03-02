import * as React from 'react';
import { Upload, Loader2, Check } from 'lucide-react';
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
import { Input } from '@/components/ui/input';
import { useImportTemplate, useCreateDocumentTemplate } from '@/hooks/use-documents';

/** Step in the import flow. */
type Step = 'upload' | 'loading' | 'preview' | 'save';

interface ImportTemplateDialogProps {
  /** Whether the dialog is open. */
  open: boolean;
  /** Callback to close the dialog. */
  onClose: () => void;
}

/**
 * Multi-step dialog for importing a COI template from a PDF using AI vision analysis.
 *
 * Step 1 – Upload: User selects a PDF file.
 * Step 2 – Loading: AI analyzes the document.
 * Step 3 – Preview: Side-by-side view of the rendered HTML (iframe) and editable source (textarea).
 * Step 4 – Save: User sets name/description and saves the template.
 */
export function ImportTemplateDialog({ open, onClose }: ImportTemplateDialogProps) {
  const { t } = useTranslation();
  const [step, setStep] = React.useState<Step>('upload');
  const [selectedFile, setSelectedFile] = React.useState<File | null>(null);
  const [generatedContent, setGeneratedContent] = React.useState('');
  const [editableContent, setEditableContent] = React.useState('');
  const [templateName, setTemplateName] = React.useState('');
  const [templateDescription, setTemplateDescription] = React.useState('');
  const [dragOver, setDragOver] = React.useState(false);

  const importMutation = useImportTemplate();
  const createMutation = useCreateDocumentTemplate();

  const handleClose = () => {
    setStep('upload');
    setSelectedFile(null);
    setGeneratedContent('');
    setEditableContent('');
    setTemplateName('');
    setTemplateDescription('');
    onClose();
  };

  const handleFileSelect = (file: File) => {
    if (!file.name.endsWith('.pdf')) return;
    setSelectedFile(file);
  };

  const handleAnalyze = async () => {
    if (!selectedFile) return;
    setStep('loading');
    try {
      const result = await importMutation.mutateAsync(selectedFile);
      setGeneratedContent(result.generatedContent);
      setEditableContent(result.generatedContent);
      setTemplateName(result.suggestedName || '');
      setStep('preview');
    } catch {
      setStep('upload');
    }
  };

  const handleSave = async () => {
    await createMutation.mutateAsync({
      name: templateName,
      description: templateDescription,
      templateType: 'CertificateOfInsurance',
      content: editableContent,
    });
    handleClose();
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className={step === 'preview' ? 'max-w-5xl' : 'max-w-lg'}>
        <DialogHeader>
          <DialogTitle>{t('documents.templatesPage.importDialog.title')}</DialogTitle>
          <DialogDescription>
            {step === 'upload' && t('documents.templatesPage.importDialog.stepUpload')}
            {step === 'loading' && t('documents.templatesPage.importDialog.stepLoading')}
            {step === 'preview' && t('documents.templatesPage.importDialog.stepPreview')}
          </DialogDescription>
        </DialogHeader>

        {/* Step 1 – File Upload */}
        {step === 'upload' && (
          <div className="space-y-4">
            <div
              className={`border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors ${
                dragOver ? 'border-primary bg-primary/5' : 'border-muted-foreground/30 hover:border-primary/50'
              }`}
              onDragOver={e => { e.preventDefault(); setDragOver(true); }}
              onDragLeave={() => setDragOver(false)}
              onDrop={e => {
                e.preventDefault();
                setDragOver(false);
                const file = e.dataTransfer.files[0];
                if (file) handleFileSelect(file);
              }}
              onClick={() => document.getElementById('pdf-input')?.click()}
            >
              <Upload className="h-8 w-8 mx-auto mb-2 text-muted-foreground" />
              <p className="text-sm font-medium">
                {selectedFile ? selectedFile.name : t('documents.templatesPage.importDialog.dropZone')}
              </p>
              <p className="text-xs text-muted-foreground mt-1">{t('documents.templatesPage.importDialog.pdfOnly')}</p>
              <input
                id="pdf-input"
                type="file"
                accept=".pdf,application/pdf"
                className="hidden"
                onChange={e => { const f = e.target.files?.[0]; if (f) handleFileSelect(f); }}
              />
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={handleClose}>{t('common.actions.cancel')}</Button>
              <Button onClick={handleAnalyze} disabled={!selectedFile}>
                {t('documents.templatesPage.importDialog.analyzeWithAi')}
              </Button>
            </DialogFooter>
          </div>
        )}

        {/* Step 2 – Loading */}
        {step === 'loading' && (
          <div className="flex flex-col items-center justify-center py-12 gap-4">
            <Loader2 className="h-10 w-10 animate-spin text-primary" />
            <p className="text-sm text-muted-foreground">{t('documents.templatesPage.importDialog.analyzing')}</p>
            <p className="text-xs text-muted-foreground">{t('documents.templatesPage.importDialog.takesAMinute')}</p>
          </div>
        )}

        {/* Step 3 – Preview */}
        {step === 'preview' && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4 h-96">
              <div className="flex flex-col gap-1">
                <p className="text-xs font-medium text-muted-foreground">{t('documents.templatesPage.importDialog.renderedPreview')}</p>
                <iframe
                  srcDoc={generatedContent}
                  className="w-full flex-1 border rounded bg-white"
                  title="Template preview"
                  sandbox="allow-same-origin"
                />
              </div>
              <div className="flex flex-col gap-1">
                <p className="text-xs font-medium text-muted-foreground">{t('documents.templatesPage.importDialog.handlebarsSource')}</p>
                <textarea
                  className="flex-1 border rounded p-2 text-xs font-mono resize-none focus:outline-none focus:ring-1 focus:ring-ring"
                  value={editableContent}
                  onChange={e => setEditableContent(e.target.value)}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium mb-1">{t('documents.templatesPage.importDialog.templateName')}</label>
                <Input
                  value={templateName}
                  onChange={e => setTemplateName(e.target.value)}
                  placeholder={t('documents.templatesPage.importDialog.templateNamePlaceholder')}
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">{t('documents.templatesPage.importDialog.descriptionOptional')}</label>
                <Input
                  value={templateDescription}
                  onChange={e => setTemplateDescription(e.target.value)}
                  placeholder={t('documents.templatesPage.importDialog.descriptionPlaceholder')}
                />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={handleClose}>{t('documents.templatesPage.importDialog.discard')}</Button>
              <Button
                onClick={handleSave}
                disabled={!templateName.trim() || createMutation.isPending}
              >
                {createMutation.isPending ? (
                  <><Loader2 className="h-4 w-4 mr-2 animate-spin" />{t('documents.templatesPage.importDialog.saving')}</>
                ) : (
                  <><Check className="h-4 w-4 mr-2" />{t('documents.templatesPage.importDialog.saveAsTemplate')}</>
                )}
              </Button>
            </DialogFooter>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
