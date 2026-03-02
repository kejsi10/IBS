import { api } from '@/lib/api';
import type {
  TenantDetails,
  TenantPagedResult,
  CreateTenantRequest,
  UpdateTenantRequest,
  UpdateSubscriptionRequest,
  AddTenantCarrierRequest,
} from '@/types/tenant';
import type { CreateResponse } from '@/types/api';

/**
 * Service for tenant management operations.
 */
export const tenantService = {
  /**
   * Searches tenants with pagination.
   */
  getAll: async (params?: { page?: number; pageSize?: number; search?: string }): Promise<TenantPagedResult> => {
    const response = await api.get<TenantPagedResult>('/tenants', { params });
    return response.data;
  },

  /**
   * Gets a tenant by ID.
   */
  getById: async (id: string): Promise<TenantDetails> => {
    const response = await api.get<TenantDetails>(`/tenants/${id}`);
    return response.data;
  },

  /**
   * Creates a new tenant.
   */
  create: async (data: CreateTenantRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/tenants', data);
    return response.data;
  },

  /**
   * Updates an existing tenant.
   */
  update: async (id: string, data: UpdateTenantRequest): Promise<void> => {
    await api.put(`/tenants/${id}`, data);
  },

  /**
   * Suspends a tenant.
   */
  suspend: async (id: string): Promise<void> => {
    await api.post(`/tenants/${id}/suspend`);
  },

  /**
   * Activates a tenant.
   */
  activate: async (id: string): Promise<void> => {
    await api.post(`/tenants/${id}/activate`);
  },

  /**
   * Cancels a tenant.
   */
  cancel: async (id: string): Promise<void> => {
    await api.post(`/tenants/${id}/cancel`);
  },

  /**
   * Updates a tenant's subscription tier.
   */
  updateSubscription: async (id: string, data: UpdateSubscriptionRequest): Promise<void> => {
    await api.put(`/tenants/${id}/subscription`, data);
  },

  /**
   * Adds a carrier to a tenant.
   */
  addCarrier: async (id: string, data: AddTenantCarrierRequest): Promise<void> => {
    await api.post(`/tenants/${id}/carriers`, data);
  },

  /**
   * Removes a carrier from a tenant.
   */
  removeCarrier: async (id: string, carrierId: string): Promise<void> => {
    await api.delete(`/tenants/${id}/carriers/${carrierId}`);
  },
};
