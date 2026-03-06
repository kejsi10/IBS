import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { Download, Trash2, Upload, Search, FolderOpen, FileText } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { NativeSelect } from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import { Pagination, PaginationInfo } from '@/components/ui/pagination';
import { Skeleton } from '@/components/ui/skeleton';
import { useDocuments, useUploadDocument, useDeleteDocument } from '@/hooks/use-documents';
import { documentsService } from '@/services';
import { openSafeUrl } from '@/lib/utils';
import type { DocumentFilters, DocumentCategory, DocumentEntityType } from '@/types/api';

/** Category badge color mapping. */
const CATEGORY_COLORS: Record<DocumentCategory, string> = {
  Policy: 'bg-blue-100 text-blue-800',
  Endorsement: 'bg-purple-100 text-purple-800',
  COI: 'bg-green-100 text-green-800',
  ClaimReport: 'bg-red-100 text-red-800',
  KYC: 'bg-yellow-100 text-yellow-800',
  Invoice: 'bg-orange-100 text-orange-800',
  Proposal: 'bg-indigo-100 text-indigo-800',
  Other: 'bg-gray-100 text-gray-700',
};

/** Format file size to human-readable. */
function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/** Documents page component. */
export default function DocumentsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [filters, setFilters] = React.useState<DocumentFilters>({ page: 1, pageSize: 20 });
  const [search, setSearch] = React.useState('');
  const [showUpload, setShowUpload] = React.useState(false);

  const { data, isLoading } = useDocuments(filters);
  const deleteMutation = useDeleteDocument();

  /** Category filter options — depends on t so defined inside component. */
  const CATEGORY_OPTIONS = React.useMemo(() => [
    { value: '', label: t('documents.categories.all') },
    { value: 'Policy', label: t('documents.categories.policy') },
    { value: 'Endorsement', label: t('documents.categories.endorsement') },
    { value: 'COI', label: t('documents.categories.coi') },
    { value: 'ClaimReport', label: t('documents.categories.claimReport') },
    { value: 'KYC', label: t('documents.categories.kyc') },
    { value: 'Invoice', label: t('documents.categories.invoice') },
    { value: 'Other', label: t('documents.categories.other') },
  ], [t]);

  /** Entity type filter options — depends on t so defined inside component. */
  const ENTITY_TYPE_OPTIONS = React.useMemo(() => [
    { value: '', label: t('documents.entityTypes.all') },
    { value: 'Policy', label: t('documents.entityTypes.policy') },
    { value: 'Client', label: t('documents.entityTypes.client') },
    { value: 'Claim', label: t('documents.entityTypes.claim') },
    { value: 'Carrier', label: t('documents.entityTypes.carrier') },
    { value: 'General', label: t('documents.entityTypes.general') },
  ], [t]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setFilters(f => ({ ...f, searchTerm: search || undefined, page: 1 }));
  };

  const handleDownload = async (id: string) => {
    const url = await documentsService.getDownloadUrl(id);
    openSafeUrl(url);
  };

  const handleDelete = async (id: string) => {
    if (confirm(t('documents.uploadDialog.description'))) {
      await deleteMutation.mutateAsync(id);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t('documents.title')}</h1>
          <p className="text-muted-foreground text-sm mt-1">{t('documents.subtitle')}</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => navigate('/documents/templates')}>
            <FileText className="h-4 w-4 mr-2" />
            {t('documents.templatesPage.title')}
          </Button>
          <Button onClick={() => setShowUpload(true)}>
            <Upload className="h-4 w-4 mr-2" />
            {t('documents.upload')}
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-4">
          <form onSubmit={handleSearch} className="flex gap-3 flex-wrap">
            <div className="relative flex-1 min-w-48">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                className="pl-9"
                placeholder={t('documents.searchPlaceholder')}
                value={search}
                onChange={e => setSearch(e.target.value)}
              />
            </div>
            <NativeSelect
              options={CATEGORY_OPTIONS}
              value={filters.category ?? ''}
              onChange={e => setFilters(f => ({ ...f, category: (e.target.value as DocumentCategory) || undefined, page: 1 }))}
            />
            <NativeSelect
              options={ENTITY_TYPE_OPTIONS}
              value={filters.entityType ?? ''}
              onChange={e => setFilters(f => ({ ...f, entityType: (e.target.value as DocumentEntityType) || undefined, page: 1 }))}
            />
            <Button type="submit" variant="outline">{t('common.actions.search')}</Button>
          </form>
        </CardContent>
      </Card>

      {/* Document List */}
      <Card>
        <CardContent className="pt-4">
          {isLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : !data?.items.length ? (
            <div className="flex flex-col items-center justify-center py-12 text-muted-foreground">
              <FolderOpen className="h-12 w-12 mb-3 opacity-40" />
              <p className="text-sm">{t('documents.noDocuments')}</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-muted-foreground">
                    <th className="pb-2 pr-4 font-medium">{t('documents.columns.fileName')}</th>
                    <th className="pb-2 pr-4 font-medium">{t('documents.columns.category')}</th>
                    <th className="pb-2 pr-4 font-medium">{t('documents.columns.entityType')}</th>
                    <th className="pb-2 pr-4 font-medium">{t('documents.columns.size')}</th>
                    <th className="pb-2 pr-4 font-medium">{t('documents.columns.uploaded')}</th>
                    <th className="pb-2 font-medium">{t('documents.columns.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map(doc => (
                    <tr key={doc.id} className="border-b last:border-0 hover:bg-muted/30">
                      <td className="py-2 pr-4">
                        <span className="font-medium">{doc.fileName}</span>
                        {doc.isArchived && (
                          <Badge className="ml-2 bg-gray-100 text-gray-600 text-xs">{t('documents.archived')}</Badge>
                        )}
                      </td>
                      <td className="py-2 pr-4">
                        <span className={`px-2 py-0.5 rounded text-xs font-medium ${CATEGORY_COLORS[doc.category]}`}>
                          {doc.category}
                        </span>
                      </td>
                      <td className="py-2 pr-4 text-muted-foreground">{doc.entityType}</td>
                      <td className="py-2 pr-4 text-muted-foreground">{formatFileSize(doc.fileSizeBytes)}</td>
                      <td className="py-2 pr-4 text-muted-foreground">
                        {new Date(doc.uploadedAt).toLocaleDateString()}
                      </td>
                      <td className="py-2">
                        <div className="flex gap-1">
                          <Button size="sm" variant="ghost" onClick={() => handleDownload(doc.id)} title={t('common.actions.download')}>
                            <Download className="h-4 w-4" />
                          </Button>
                          {!doc.isArchived && (
                            <Button
                              size="sm"
                              variant="ghost"
                              onClick={() => handleDelete(doc.id)}
                              title={t('common.actions.archive')}
                              className="text-red-500 hover:text-red-700"
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {data && data.totalPages > 1 && (
            <div className="mt-4 flex items-center justify-between">
              <PaginationInfo currentPage={data.page} pageSize={data.pageSize} totalCount={data.totalCount} />
              <Pagination
                currentPage={data.page}
                totalPages={data.totalPages}
                onPageChange={p => setFilters(f => ({ ...f, page: p }))}
              />
            </div>
          )}
        </CardContent>
      </Card>

      <UploadDialog open={showUpload} onClose={() => setShowUpload(false)} />
    </div>
  );
}

/** Upload document dialog. */
function UploadDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { t } = useTranslation();
  const [file, setFile] = React.useState<File | null>(null);
  const [entityType, setEntityType] = React.useState<DocumentEntityType>('General');
  const [category, setCategory] = React.useState<DocumentCategory>('Other');
  const [entityId, setEntityId] = React.useState('');
  const [description, setDescription] = React.useState('');
  const uploadMutation = useUploadDocument();

  /** Upload category options — depends on t so defined inside component. */
  const UPLOAD_CATEGORY_OPTIONS = React.useMemo(() => [
    { value: 'Policy', label: t('documents.categories.policy') },
    { value: 'Endorsement', label: t('documents.categories.endorsement') },
    { value: 'COI', label: t('documents.categories.coi') },
    { value: 'ClaimReport', label: t('documents.categories.claimReport') },
    { value: 'KYC', label: t('documents.categories.kyc') },
    { value: 'Invoice', label: t('documents.categories.invoice') },
    { value: 'Other', label: t('documents.categories.other') },
  ], [t]);

  /** Upload entity type options — depends on t so defined inside component. */
  const UPLOAD_ENTITY_TYPE_OPTIONS = React.useMemo(() => [
    { value: 'General', label: t('documents.entityTypes.general') },
    { value: 'Policy', label: t('documents.entityTypes.policy') },
    { value: 'Client', label: t('documents.entityTypes.client') },
    { value: 'Claim', label: t('documents.entityTypes.claim') },
    { value: 'Carrier', label: t('documents.entityTypes.carrier') },
  ], [t]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!file) return;
    await uploadMutation.mutateAsync({
      file,
      entityType,
      category,
      entityId: entityId || undefined,
      description: description || undefined,
    });
    onClose();
    setFile(null);
    setEntityId('');
    setDescription('');
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('documents.uploadDialog.title')}</DialogTitle>
          <DialogDescription>{t('documents.uploadDialog.description')}</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">{t('documents.uploadDialog.file')}</label>
            <Input type="file" onChange={e => setFile(e.target.files?.[0] ?? null)} required />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium mb-1">{t('documents.uploadDialog.category')}</label>
              <NativeSelect
                options={UPLOAD_CATEGORY_OPTIONS}
                value={category}
                onChange={e => setCategory(e.target.value as DocumentCategory)}
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('documents.uploadDialog.entityType')}</label>
              <NativeSelect
                options={UPLOAD_ENTITY_TYPE_OPTIONS}
                value={entityType}
                onChange={e => setEntityType(e.target.value as DocumentEntityType)}
              />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">{t('documents.uploadDialog.entityId')}</label>
            <Input
              placeholder="UUID of linked entity"
              value={entityId}
              onChange={e => setEntityId(e.target.value)}
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">{t('documents.uploadDialog.description')}</label>
            <Input
              placeholder="Brief description"
              value={description}
              onChange={e => setDescription(e.target.value)}
            />
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>{t('common.actions.cancel')}</Button>
            <Button type="submit" disabled={!file || uploadMutation.isPending}>
              {uploadMutation.isPending ? t('documents.uploadDialog.uploading') : t('documents.uploadDialog.upload')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
