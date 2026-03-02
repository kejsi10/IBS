import * as React from 'react';
import { Plus, Edit, CheckCircle, XCircle, FileDown, Upload, Wand2, ChevronLeft } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { NativeSelect } from '@/components/ui/select';
import { Skeleton } from '@/components/ui/skeleton';
import {
  useDocumentTemplates,
  useDocumentTemplate,
  useCreateDocumentTemplate,
  useUpdateDocumentTemplate,
  useActivateDocumentTemplate,
  useDeactivateDocumentTemplate,
} from '@/hooks/use-documents';
import type { DocumentTemplateListItem, TemplateType } from '@/types/api';
import { GenerateCOIDialog } from './components/generate-coi-dialog';
import { ImportTemplateDialog } from './components/import-template-dialog';
import { AIEditDialog } from './components/ai-edit-dialog';

/** Handlebar placeholder reference sidebar text. */
const PLACEHOLDERS = [
  '{{PolicyNumber}}',
  '{{ClientName}}',
  '{{CarrierName}}',
  '{{EffectiveDate}}',
  '{{ExpirationDate}}',
  '{{LineOfBusiness}}',
  '{{BrokerName}}',
  '{{IssuedDate}}',
];

/** Templates management page. */
export default function TemplatesPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [editTemplate, setEditTemplate] = React.useState<DocumentTemplateListItem | null>(null);
  const [showCreate, setShowCreate] = React.useState(false);
  const [showImport, setShowImport] = React.useState(false);
  const [generateCOITemplate, setGenerateCOITemplate] = React.useState<DocumentTemplateListItem | null>(null);
  const [aiEditTemplate, setAIEditTemplate] = React.useState<DocumentTemplateListItem | null>(null);

  const { data, isLoading } = useDocumentTemplates();
  const activateMutation = useActivateDocumentTemplate();
  const deactivateMutation = useDeactivateDocumentTemplate();

  const handleToggle = async (template: DocumentTemplateListItem) => {
    if (template.isActive) {
      await deactivateMutation.mutateAsync(template.id);
    } else {
      await activateMutation.mutateAsync(template.id);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <Button variant="ghost" size="sm" className="-ml-2 mb-1 text-muted-foreground" onClick={() => navigate('/documents')}>
            <ChevronLeft className="h-4 w-4 mr-1" />
            {t('documents.title')}
          </Button>
          <h1 className="text-2xl font-bold">{t('documents.templatesPage.title')}</h1>
          <p className="text-muted-foreground text-sm mt-1">{t('documents.templatesPage.subtitle')}</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setShowImport(true)}>
            <Upload className="h-4 w-4 mr-2" />
            {t('documents.templatesPage.importFromPdf')}
          </Button>
          <Button onClick={() => setShowCreate(true)}>
            <Plus className="h-4 w-4 mr-2" />
            {t('documents.templatesPage.newTemplate')}
          </Button>
        </div>
      </div>

      <Card>
        <CardContent className="pt-4">
          {isLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 3 }).map((_, i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : !data?.items.length ? (
            <div className="flex flex-col items-center justify-center py-12 text-muted-foreground">
              <p className="text-sm">{t('documents.templatesPage.noTemplates')}</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-muted-foreground">
                    <th className="pb-2 pr-4 font-medium">{t('documents.columns.name')}</th>
                    <th className="pb-2 pr-4 font-medium">{t('documents.templatesPage.editorDialog.type')}</th>
                    <th className="pb-2 pr-4 font-medium">{t('common.table.status')}</th>
                    <th className="pb-2 pr-4 font-medium">{t('documents.templatesPage.version')}</th>
                    <th className="pb-2 font-medium">{t('common.table.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map(template => (
                    <tr key={template.id} className="border-b last:border-0 hover:bg-muted/30">
                      <td className="py-2 pr-4 font-medium">{template.name}</td>
                      <td className="py-2 pr-4 text-muted-foreground">
                        {template.templateType === 'CertificateOfInsurance'
                          ? t('documents.templatesPage.templateTypes.coiShort')
                          : t('documents.templatesPage.templateTypes.policySummary')}
                      </td>
                      <td className="py-2 pr-4">
                        {template.isActive ? (
                          <Badge className="bg-green-100 text-green-800">{t('common.status.active')}</Badge>
                        ) : (
                          <Badge className="bg-gray-100 text-gray-600">{t('common.status.inactive')}</Badge>
                        )}
                      </td>
                      <td className="py-2 pr-4 text-muted-foreground">v{template.version}</td>
                      <td className="py-2">
                        <div className="flex gap-1">
                          <Button size="sm" variant="ghost" onClick={() => setEditTemplate(template)} title={t('common.actions.edit')}>
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => setAIEditTemplate(template)}
                            title={t('documents.templatesPage.aiEdit')}
                          >
                            <Wand2 className="h-4 w-4 text-purple-500" />
                          </Button>
                          {template.isActive && template.templateType === 'CertificateOfInsurance' && (
                            <Button
                              size="sm"
                              variant="ghost"
                              onClick={() => setGenerateCOITemplate(template)}
                              title={t('documents.templatesPage.generate')}
                            >
                              <FileDown className="h-4 w-4 text-blue-500" />
                            </Button>
                          )}
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => handleToggle(template)}
                            title={template.isActive ? t('common.actions.deactivate') : t('common.actions.activate')}
                          >
                            {template.isActive ? (
                              <XCircle className="h-4 w-4 text-red-500" />
                            ) : (
                              <CheckCircle className="h-4 w-4 text-green-500" />
                            )}
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      <TemplateEditorDialog
        open={showCreate}
        onClose={() => setShowCreate(false)}
        mode="create"
      />
      {editTemplate && (
        <TemplateEditorDialog
          open={!!editTemplate}
          onClose={() => setEditTemplate(null)}
          mode="edit"
          template={editTemplate}
        />
      )}
      {generateCOITemplate && (
        <GenerateCOIDialog
          open={!!generateCOITemplate}
          onClose={() => setGenerateCOITemplate(null)}
          template={generateCOITemplate}
        />
      )}
      <ImportTemplateDialog
        open={showImport}
        onClose={() => setShowImport(false)}
      />
      {aiEditTemplate && (
        <AIEditDialog
          open={!!aiEditTemplate}
          onClose={() => setAIEditTemplate(null)}
          template={aiEditTemplate}
          onEditManually={_modifiedContent => {
            // Opens the standard editor; user can paste the modified content manually
            setEditTemplate({ ...aiEditTemplate });
            setAIEditTemplate(null);
          }}
        />
      )}
    </div>
  );
}

/** Template editor dialog for create/edit. */
function TemplateEditorDialog({
  open,
  onClose,
  mode,
  template,
}: {
  open: boolean;
  onClose: () => void;
  mode: 'create' | 'edit';
  template?: DocumentTemplateListItem;
}) {
  const { t } = useTranslation();
  const templateTypeOptions = [
    { value: 'CertificateOfInsurance', label: t('documents.templatesPage.templateTypes.certificateOfInsurance') },
    { value: 'PolicySummary', label: t('documents.templatesPage.templateTypes.policySummary') },
  ];
  const [name, setName] = React.useState(template?.name ?? '');
  const [description, setDescription] = React.useState(template?.description ?? '');
  const [templateType, setTemplateType] = React.useState<TemplateType>(
    template?.templateType ?? 'CertificateOfInsurance'
  );
  const [content, setContent] = React.useState('');

  const { data: fullTemplate, isLoading: isLoadingContent } = useDocumentTemplate(
    mode === 'edit' && template ? template.id : ''
  );

  React.useEffect(() => {
    if (fullTemplate?.content) {
      setContent(fullTemplate.content);
    }
  }, [fullTemplate?.content]);

  const createMutation = useCreateDocumentTemplate();
  const updateMutation = useUpdateDocumentTemplate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (mode === 'create') {
      await createMutation.mutateAsync({ name, description, templateType, content });
    } else if (template) {
      await updateMutation.mutateAsync({ id: template.id, request: { name, description, content } });
    }
    onClose();
  };

  const isPending = createMutation.isPending || updateMutation.isPending;

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-3xl">
        <DialogHeader>
          <DialogTitle>
            {mode === 'create'
              ? t('documents.templatesPage.editorDialog.createTitle')
              : t('documents.templatesPage.editorDialog.editTitle')}
          </DialogTitle>
          <DialogDescription>
            {t('documents.templatesPage.editorDialog.description')}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium mb-1">{t('documents.templatesPage.editorDialog.name')}</label>
              <Input value={name} onChange={e => setName(e.target.value)} required />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('documents.templatesPage.editorDialog.type')}</label>
              <NativeSelect
                options={templateTypeOptions}
                value={templateType}
                onChange={e => setTemplateType(e.target.value as TemplateType)}
                disabled={mode === 'edit'}
              />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">{t('documents.templatesPage.editorDialog.templateDescription')}</label>
            <Input value={description} onChange={e => setDescription(e.target.value)} placeholder={t('documents.templatesPage.editorDialog.templateDescription')} />
          </div>
          <div className="flex gap-3">
            <div className="flex-1">
              <label className="block text-sm font-medium mb-1">{t('documents.templatesPage.editorDialog.content')}</label>
              {isLoadingContent ? (
                <Skeleton className="w-full h-64" />
              ) : (
                <textarea
                  className="w-full border rounded p-2 text-sm font-mono h-64 resize-none focus:outline-none focus:ring-1 focus:ring-ring"
                  value={content}
                  onChange={e => setContent(e.target.value)}
                  placeholder="<h1>Certificate of Insurance</h1><p>Policy: {{PolicyNumber}}</p>"
                  required
                />
              )}
            </div>
            <div className="w-48">
              <p className="text-sm font-medium mb-1">{t('documents.templatesPage.editorDialog.placeholders')}</p>
              <div className="space-y-1">
                {PLACEHOLDERS.map(p => (
                  <button
                    key={p}
                    type="button"
                    className="block w-full text-left text-xs font-mono bg-muted px-2 py-1 rounded hover:bg-muted/70"
                    onClick={() => setContent(c => c + p)}
                  >
                    {p}
                  </button>
                ))}
              </div>
            </div>
          </div>
          {content && (
            <div>
              <label className="block text-sm font-medium mb-1">{t('documents.templatesPage.editorDialog.preview')}</label>
              <iframe
                srcDoc={content}
                className="w-full h-64 border rounded bg-white"
                title="Template preview"
                sandbox="allow-scripts"
              />
            </div>
          )}
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>{t('common.actions.cancel')}</Button>
            <Button type="submit" disabled={isPending}>
              {isPending
                ? (mode === 'create' ? t('documents.templatesPage.editorDialog.creating') : t('documents.templatesPage.editorDialog.saving'))
                : (mode === 'create' ? t('documents.templatesPage.editorDialog.create') : t('documents.templatesPage.editorDialog.save'))}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
