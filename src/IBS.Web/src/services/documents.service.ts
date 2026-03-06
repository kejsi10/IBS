import { api } from '@/lib/api';
import type {
  DocumentDetail,
  DocumentListItem,
  DocumentTemplateDetail,
  DocumentTemplateListItem,
  DocumentFilters,
  DocumentTemplateFilters,
  CreateDocumentTemplateRequest,
  UpdateDocumentTemplateRequest,
  GenerateCOIRequest,
  GenerateProposalRequest,
  ImportTemplateResult,
  AIEditTemplateRequest,
  AIEditTemplateResult,
  PaginatedResult,
  CreateResponse,
  DocumentEntityType,
  DocumentCategory,
} from '@/types/api';

/** Backend paginated response shape for documents. */
interface BackendDocumentListResult {
  items: DocumentListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/** Backend paginated response shape for templates. */
interface BackendTemplateListResult {
  items: DocumentTemplateListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/** Maps backend document list to PaginatedResult. */
const mapDocumentList = (dto: BackendDocumentListResult): PaginatedResult<DocumentListItem> => ({
  items: dto.items,
  totalCount: dto.totalCount,
  page: dto.page,
  pageSize: dto.pageSize,
  totalPages: dto.totalPages,
});

/** Maps backend template list to PaginatedResult. */
const mapTemplateList = (dto: BackendTemplateListResult): PaginatedResult<DocumentTemplateListItem> => ({
  items: dto.items,
  totalCount: dto.totalCount,
  page: dto.page,
  pageSize: dto.pageSize,
  totalPages: dto.totalPages,
});

/** Service for document and template API operations. */
export const documentsService = {
  /**
   * Gets a paginated list of documents.
   * @param filters - Optional filter parameters.
   */
  async getAll(filters?: DocumentFilters): Promise<PaginatedResult<DocumentListItem>> {
    const params = new URLSearchParams();
    if (filters?.searchTerm) params.set('searchTerm', filters.searchTerm);
    if (filters?.category) params.set('category', filters.category);
    if (filters?.entityType) params.set('entityType', filters.entityType);
    if (filters?.entityId) params.set('entityId', filters.entityId);
    if (filters?.includeArchived) params.set('includeArchived', 'true');
    if (filters?.page) params.set('page', String(filters.page));
    if (filters?.pageSize) params.set('pageSize', String(filters.pageSize));
    const response = await api.get<BackendDocumentListResult>(`/documents?${params}`);
    return mapDocumentList(response.data);
  },

  /**
   * Gets a document by its ID.
   * @param id - The document identifier.
   */
  async getById(id: string): Promise<DocumentDetail> {
    const response = await api.get<DocumentDetail>(`/documents/${id}`);
    return response.data;
  },

  /**
   * Gets a temporary download URL for a document.
   * @param id - The document identifier.
   */
  async getDownloadUrl(id: string): Promise<string> {
    const response = await api.get<string>(`/documents/${id}/download`);
    return response.data;
  },

  /**
   * Uploads a new document.
   * @param file - The file to upload.
   * @param entityType - The entity type.
   * @param category - The document category.
   * @param entityId - The linked entity ID (optional).
   * @param description - Optional description.
   */
  async upload(
    file: File,
    entityType: DocumentEntityType,
    category: DocumentCategory,
    entityId?: string,
    description?: string
  ): Promise<CreateResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', entityType);
    formData.append('category', category);
    if (entityId) formData.append('entityId', entityId);
    if (description) formData.append('description', description);
    const response = await api.post<CreateResponse>('/documents/upload', formData);
    return response.data;
  },

  /**
   * Archives (soft-deletes) a document.
   * @param id - The document identifier.
   */
  async delete(id: string): Promise<void> {
    await api.delete(`/documents/${id}`);
  },

  /**
   * Gets a paginated list of document templates.
   * @param filters - Optional filter parameters.
   */
  async getTemplates(filters?: DocumentTemplateFilters): Promise<PaginatedResult<DocumentTemplateListItem>> {
    const params = new URLSearchParams();
    if (filters?.searchTerm) params.set('searchTerm', filters.searchTerm);
    if (filters?.templateType) params.set('templateType', filters.templateType);
    if (filters?.isActive !== undefined) params.set('isActive', String(filters.isActive));
    if (filters?.page) params.set('page', String(filters.page));
    if (filters?.pageSize) params.set('pageSize', String(filters.pageSize));
    const response = await api.get<BackendTemplateListResult>(`/documents/templates?${params}`);
    return mapTemplateList(response.data);
  },

  /**
   * Gets a document template by its ID.
   * @param id - The template identifier.
   */
  async getTemplateById(id: string): Promise<DocumentTemplateDetail> {
    const response = await api.get<DocumentTemplateDetail>(`/documents/templates/${id}`);
    return response.data;
  },

  /**
   * Creates a new document template.
   * @param request - The create request.
   */
  async createTemplate(request: CreateDocumentTemplateRequest): Promise<CreateResponse> {
    const response = await api.post<CreateResponse>('/documents/templates', request);
    return response.data;
  },

  /**
   * Updates a document template.
   * @param id - The template identifier.
   * @param request - The update request.
   */
  async updateTemplate(id: string, request: UpdateDocumentTemplateRequest): Promise<void> {
    await api.put(`/documents/templates/${id}`, request);
  },

  /**
   * Activates a document template.
   * @param id - The template identifier.
   */
  async activateTemplate(id: string): Promise<void> {
    await api.post(`/documents/templates/${id}/activate`, {});
  },

  /**
   * Deactivates a document template.
   * @param id - The template identifier.
   */
  async deactivateTemplate(id: string): Promise<void> {
    await api.post(`/documents/templates/${id}/deactivate`, {});
  },

  /**
   * Generates a Certificate of Insurance PDF.
   * @param request - The COI generation request.
   */
  async generateCOI(request: GenerateCOIRequest): Promise<CreateResponse> {
    const response = await api.post<CreateResponse>('/documents/generate-coi', request);
    return response.data;
  },

  /**
   * Generates a proposal PDF from a quote.
   * @param request - The proposal generation request.
   */
  async generateProposal(request: GenerateProposalRequest): Promise<CreateResponse> {
    const response = await api.post<CreateResponse>('/documents/generate-proposal', request);
    return response.data;
  },

  /**
   * Imports a COI template from a PDF file using AI vision analysis.
   * Returns a preview for the user to review before saving.
   * @param file - The PDF file to analyze.
   */
  async importTemplate(file: File): Promise<ImportTemplateResult> {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post<ImportTemplateResult>('/documents/templates/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  /**
   * Applies a natural language instruction to a template using an AI model.
   * Returns the original and modified HTML for comparison before the user saves.
   * @param id - The template identifier.
   * @param request - The instruction to apply.
   */
  async aiEditTemplate(id: string, request: AIEditTemplateRequest): Promise<AIEditTemplateResult> {
    const response = await api.post<AIEditTemplateResult>(`/documents/templates/${id}/ai-edit`, request);
    return response.data;
  },
};
