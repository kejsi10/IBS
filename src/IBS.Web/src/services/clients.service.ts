import { api } from '@/lib/api';
import type {
  Client,
  ClientSummary,
  CreateIndividualClientRequest,
  CreateBusinessClientRequest,
  UpdateClientRequest,
  AddContactRequest,
  AddAddressRequest,
  ClientFilters,
  PaginatedResult,
  CreateResponse,
  Address,
} from '@/types/api';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * Maps a backend ClientDetailsDto (nested personName/businessInfo/addresses)
 * to the flat frontend Client type.
 */
function mapClientDetail(dto: any): Client {
  return {
    id: dto.id,
    clientType: dto.clientType,
    status: dto.status,
    // Flatten personName
    firstName: dto.personName?.firstName,
    lastName: dto.personName?.lastName,
    middleName: dto.personName?.middleName,
    suffix: dto.personName?.suffix,
    dateOfBirth: dto.dateOfBirth,
    // Flatten businessInfo
    businessName: dto.businessInfo?.name,
    businessType: dto.businessInfo?.businessType,
    dbaName: dto.businessInfo?.dbaName,
    industry: dto.businessInfo?.industry,
    yearEstablished: dto.businessInfo?.yearEstablished,
    numberOfEmployees: dto.businessInfo?.numberOfEmployees,
    annualRevenue: dto.businessInfo?.annualRevenue,
    website: dto.businessInfo?.website,
    // Common
    email: dto.email,
    phone: dto.phone,
    // Relations
    contacts: dto.contacts ?? [],
    addresses: (dto.addresses ?? []).map(mapAddressResponse),
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt,
  };
}

/**
 * Maps a backend AddressDto (addressType, streetLine1, streetLine2)
 * to the frontend Address type (type, line1, line2).
 */
function mapAddressResponse(dto: any): Address {
  return {
    id: dto.id,
    type: dto.addressType,
    line1: dto.streetLine1,
    line2: dto.streetLine2,
    city: dto.city,
    state: dto.state,
    postalCode: dto.postalCode,
    country: dto.country,
    isPrimary: dto.isPrimary,
  };
}

/* eslint-enable @typescript-eslint/no-explicit-any */

/**
 * Maps frontend AddAddressRequest (type, line1, line2)
 * to backend format (addressType, streetLine1, streetLine2).
 */
function mapAddressRequest(data: AddAddressRequest): Record<string, unknown> {
  return {
    addressType: data.type,
    streetLine1: data.line1,
    streetLine2: data.line2,
    city: data.city,
    state: data.state,
    postalCode: data.postalCode,
    country: data.country,
    isPrimary: data.isPrimary,
  };
}

/**
 * Service for client management operations.
 */
export const clientsService = {
  /**
   * Gets a paginated list of clients with optional filtering.
   */
  getAll: async (filters?: ClientFilters): Promise<PaginatedResult<ClientSummary>> => {
    const response = await api.get<PaginatedResult<ClientSummary>>('/clients', {
      params: filters,
    });
    return response.data;
  },

  /**
   * Gets a client by ID.
   */
  getById: async (id: string): Promise<Client> => {
    const response = await api.get(`/clients/${id}`);
    return mapClientDetail(response.data);
  },

  /**
   * Creates a new individual client.
   */
  createIndividual: async (data: CreateIndividualClientRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/clients/individual', data);
    return response.data;
  },

  /**
   * Creates a new business client.
   */
  createBusiness: async (data: CreateBusinessClientRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/clients/business', data);
    return response.data;
  },

  /**
   * Updates an existing client.
   */
  update: async (id: string, data: UpdateClientRequest): Promise<void> => {
    await api.put(`/clients/${id}`, data);
  },

  /**
   * Deactivates a client.
   */
  deactivate: async (id: string): Promise<void> => {
    await api.post(`/clients/${id}/deactivate`);
  },

  /**
   * Reactivates a deactivated client.
   */
  reactivate: async (id: string): Promise<void> => {
    await api.post(`/clients/${id}/reactivate`);
  },

  /**
   * Adds a contact to a business client.
   */
  addContact: async (clientId: string, data: AddContactRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(`/clients/${clientId}/contacts`, data);
    return response.data;
  },

  /**
   * Updates a contact.
   */
  updateContact: async (clientId: string, contactId: string, data: Partial<AddContactRequest>): Promise<void> => {
    await api.put(`/clients/${clientId}/contacts/${contactId}`, data);
  },

  /**
   * Removes a contact from a client.
   */
  removeContact: async (clientId: string, contactId: string): Promise<void> => {
    await api.delete(`/clients/${clientId}/contacts/${contactId}`);
  },

  /**
   * Sets a contact as the primary contact for a client.
   */
  setPrimaryContact: async (clientId: string, contactId: string): Promise<void> => {
    await api.patch(`/clients/${clientId}/contacts/${contactId}`, { isPrimary: true });
  },

  /**
   * Adds an address to a client.
   */
  addAddress: async (clientId: string, data: AddAddressRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>(
      `/clients/${clientId}/addresses`,
      mapAddressRequest(data),
    );
    return response.data;
  },

  /**
   * Updates an address.
   */
  updateAddress: async (clientId: string, addressId: string, data: Partial<AddAddressRequest>): Promise<void> => {
    await api.put(
      `/clients/${clientId}/addresses/${addressId}`,
      mapAddressRequest(data as AddAddressRequest),
    );
  },

  /**
   * Removes an address from a client.
   */
  removeAddress: async (clientId: string, addressId: string): Promise<void> => {
    await api.delete(`/clients/${clientId}/addresses/${addressId}`);
  },

  /**
   * Sets an address as the primary address for a client.
   */
  setPrimaryAddress: async (clientId: string, addressId: string): Promise<void> => {
    await api.patch(`/clients/${clientId}/addresses/${addressId}`, { isPrimary: true });
  },
};
