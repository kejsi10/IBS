import { api } from '@/lib/api';
import type {
  UserSummary,
  UserDetail,
  CreateUserRequest,
  UpdateUserProfileRequest,
  AssignRoleRequest,
  UserFilters,
  PaginatedResult,
  CreateResponse,
} from '@/types/api';

/**
 * Service for user management operations.
 */
export const usersService = {
  /**
   * Gets a paginated list of users with optional filtering.
   */
  getAll: async (filters?: UserFilters): Promise<PaginatedResult<UserSummary>> => {
    const response = await api.get<PaginatedResult<UserSummary>>('/users', {
      params: filters,
    });
    return response.data;
  },

  /**
   * Gets a user by ID.
   */
  getById: async (id: string): Promise<UserDetail> => {
    const response = await api.get<UserDetail>(`/users/${id}`);
    return response.data;
  },

  /**
   * Creates a new user via the registration endpoint.
   */
  create: async (data: CreateUserRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/auth/register', data);
    return response.data;
  },

  /**
   * Updates a user's profile.
   */
  update: async (id: string, data: UpdateUserProfileRequest): Promise<void> => {
    await api.put(`/users/${id}/profile`, data);
  },

  /**
   * Activates a user account.
   */
  activate: async (id: string): Promise<void> => {
    await api.post(`/users/${id}/activate`);
  },

  /**
   * Deactivates a user account.
   */
  deactivate: async (id: string): Promise<void> => {
    await api.post(`/users/${id}/deactivate`);
  },

  /**
   * Assigns a role to a user.
   */
  assignRole: async (userId: string, data: AssignRoleRequest): Promise<void> => {
    await api.post(`/users/${userId}/roles`, data);
  },

  /**
   * Removes a role from a user.
   */
  removeRole: async (userId: string, roleId: string): Promise<void> => {
    await api.delete(`/users/${userId}/roles/${roleId}`);
  },
};
