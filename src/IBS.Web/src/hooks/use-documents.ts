import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { documentsService } from '@/services/documents.service';
import type {
  DocumentFilters,
  DocumentTemplateFilters,
  CreateDocumentTemplateRequest,
  UpdateDocumentTemplateRequest,
  GenerateCOIRequest,
  GenerateProposalRequest,
  AIEditTemplateRequest,
  DocumentEntityType,
  DocumentCategory,
} from '@/types/api';

/** Query key factory for documents. */
export const documentKeys = {
  all: ['documents'] as const,
  list: (filters?: DocumentFilters) => [...documentKeys.all, 'list', filters] as const,
  detail: (id: string) => [...documentKeys.all, 'detail', id] as const,
  downloadUrl: (id: string) => [...documentKeys.all, 'downloadUrl', id] as const,
};

/** Query key factory for document templates. */
export const templateKeys = {
  all: ['documentTemplates'] as const,
  list: (filters?: DocumentTemplateFilters) => [...templateKeys.all, 'list', filters] as const,
  detail: (id: string) => [...templateKeys.all, 'detail', id] as const,
};

// ── Document Hooks ──

/** Hook to fetch a paginated list of documents. */
export function useDocuments(filters?: DocumentFilters) {
  return useQuery({
    queryKey: documentKeys.list(filters),
    queryFn: () => documentsService.getAll(filters),
  });
}

/** Hook to fetch a single document by ID. */
export function useDocument(id: string) {
  return useQuery({
    queryKey: documentKeys.detail(id),
    queryFn: () => documentsService.getById(id),
    enabled: !!id,
  });
}

/** Hook to upload a document. */
export function useUploadDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      file,
      entityType,
      category,
      entityId,
      description,
    }: {
      file: File;
      entityType: DocumentEntityType;
      category: DocumentCategory;
      entityId?: string;
      description?: string;
    }) => documentsService.upload(file, entityType, category, entityId, description),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: documentKeys.all });
    },
  });
}

/** Hook to archive (delete) a document. */
export function useDeleteDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => documentsService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: documentKeys.all });
    },
  });
}

// ── Template Hooks ──

/** Hook to fetch a paginated list of document templates. */
export function useDocumentTemplates(filters?: DocumentTemplateFilters) {
  return useQuery({
    queryKey: templateKeys.list(filters),
    queryFn: () => documentsService.getTemplates(filters),
  });
}

/** Hook to fetch a single document template by ID. */
export function useDocumentTemplate(id: string) {
  return useQuery({
    queryKey: templateKeys.detail(id),
    queryFn: () => documentsService.getTemplateById(id),
    enabled: !!id,
  });
}

/** Hook to create a document template. */
export function useCreateDocumentTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateDocumentTemplateRequest) => documentsService.createTemplate(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: templateKeys.all });
    },
  });
}

/** Hook to update a document template. */
export function useUpdateDocumentTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateDocumentTemplateRequest }) =>
      documentsService.updateTemplate(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: templateKeys.all });
    },
  });
}

/** Hook to activate a document template. */
export function useActivateDocumentTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => documentsService.activateTemplate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: templateKeys.all });
    },
  });
}

/** Hook to deactivate a document template. */
export function useDeactivateDocumentTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => documentsService.deactivateTemplate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: templateKeys.all });
    },
  });
}

/** Hook to generate a COI document. */
export function useGenerateCOI() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: GenerateCOIRequest) => documentsService.generateCOI(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: documentKeys.all });
    },
  });
}

/** Hook to import a COI template from a PDF using AI. Returns generated content for preview. */
export function useImportTemplate() {
  return useMutation({
    mutationFn: (file: File) => documentsService.importTemplate(file),
  });
}

/** Hook to apply a natural language edit instruction to a template using AI. */
export function useAIEditTemplate() {
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: AIEditTemplateRequest }) =>
      documentsService.aiEditTemplate(id, request),
  });
}

/** Hook to generate a proposal document from a quote. */
export function useGenerateProposal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: GenerateProposalRequest) => documentsService.generateProposal(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: documentKeys.all });
    },
  });
}
