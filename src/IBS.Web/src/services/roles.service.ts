import { api } from '@/lib/api';
import type {
  RoleSummary,
  RoleDetail,
  Permission,
  CreateRoleRequest,
  UpdateRoleRequest,
  GrantPermissionRequest,
  CreateResponse,
} from '@/types/api';

/**
 * Service for role and permission management operations.
 */
export const rolesService = {
  /**
   * Gets all roles.
   */
  getAll: async (): Promise<RoleSummary[]> => {
    const response = await api.get<RoleSummary[]>('/roles');
    return response.data;
  },

  /**
   * Gets a role by ID.
   */
  getById: async (id: string): Promise<RoleDetail> => {
    const response = await api.get<RoleDetail>(`/roles/${id}`);
    return response.data;
  },

  /**
   * Creates a new role.
   */
  create: async (data: CreateRoleRequest): Promise<CreateResponse> => {
    const response = await api.post<CreateResponse>('/roles', data);
    return response.data;
  },

  /**
   * Updates a role.
   */
  update: async (id: string, data: UpdateRoleRequest): Promise<void> => {
    await api.put(`/roles/${id}`, data);
  },

  /**
   * Grants a permission to a role.
   */
  grantPermission: async (roleId: string, data: GrantPermissionRequest): Promise<void> => {
    await api.post(`/roles/${roleId}/permissions`, data);
  },

  /**
   * Revokes a permission from a role.
   */
  revokePermission: async (roleId: string, permissionId: string): Promise<void> => {
    await api.delete(`/roles/${roleId}/permissions/${permissionId}`);
  },

  /**
   * Gets all available permissions, optionally filtered by module.
   */
  getPermissions: async (module?: string): Promise<Permission[]> => {
    const response = await api.get<Permission[]>('/permissions', {
      params: module ? { module } : undefined,
    });
    return response.data;
  },
};
