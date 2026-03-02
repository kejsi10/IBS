import { api } from '@/lib/api';
import type {
  Carrier,
  CarrierSummary,
  CreateCarrierRequest,
  UpdateCarrierRequest,
  CarrierStatus,
  AddProductRequest,
  AddAppetiteRequest,
  CreateResponse,
} from '@/types/api';

/**
 * Service for carrier management operations.
 */
export const carriersService = {
  /**
   * Gets all carriers.
   */
  getAll: async (): Promise<CarrierSummary[]> => {
    const response = await api.get<CarrierSummary[]>('/carriers');
    return response.data;
  },

  /**
   * Gets a carrier by ID.
   */
  getById: async (id: string): Promise<Carrier> => {
    const response = await api.get<Carrier>(`/carriers/${id}`);
    return response.data;
  },

  /**
   * Gets carriers by status.
   */
  getByStatus: async (status: CarrierStatus): Promise<CarrierSummary[]> => {
    const response = await api.get<CarrierSummary[]>('/carriers', {
      params: { status },
    });
    return response.data;
  },

  /**
   * Searches carriers by name or code.
   */
  search: async (searchTerm: string): Promise<CarrierSummary[]> => {
    const response = await api.get<CarrierSummary[]>('/carriers', {
      params: { search: searchTerm },
    });
    return response.data;
  },

  /**
   * Creates a new carrier.
   */
  create: async (data: CreateCarrierRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/carriers', data);
    return response.data;
  },

  /**
   * Updates an existing carrier.
   */
  update: async (id: string, data: UpdateCarrierRequest): Promise<void> => {
    await api.put(`/carriers/${id}`, data);
  },

  /**
   * Deactivates a carrier.
   */
  deactivate: async (id: string, reason?: string): Promise<void> => {
    await api.post(`/carriers/${id}/deactivate`, { reason });
  },

  /**
   * Adds a product to a carrier.
   */
  addProduct: async (carrierId: string, data: AddProductRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/carriers/${carrierId}/products`, data);
    return response.data;
  },

  /**
   * Updates a product.
   */
  updateProduct: async (carrierId: string, productId: string, data: Partial<AddProductRequest>): Promise<void> => {
    await api.put(`/carriers/${carrierId}/products/${productId}`, data);
  },

  /**
   * Deactivates a product.
   */
  deactivateProduct: async (carrierId: string, productId: string): Promise<void> => {
    await api.post(`/carriers/${carrierId}/products/${productId}/deactivate`);
  },

  /**
   * Adds appetite/underwriting criteria to a carrier.
   */
  addAppetite: async (carrierId: string, data: AddAppetiteRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/carriers/${carrierId}/appetites`, data);
    return response.data;
  },

  /**
   * Updates appetite criteria.
   */
  updateAppetite: async (carrierId: string, appetiteId: string, data: Partial<AddAppetiteRequest>): Promise<void> => {
    await api.put(`/carriers/${carrierId}/appetites/${appetiteId}`, data);
  },

  /**
   * Deactivates appetite criteria.
   */
  deactivateAppetite: async (carrierId: string, appetiteId: string): Promise<void> => {
    await api.post(`/carriers/${carrierId}/appetites/${appetiteId}/deactivate`);
  },
};
