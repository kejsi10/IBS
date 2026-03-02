import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { rolesService } from '@/services';
import type {
  CreateRoleRequest,
  UpdateRoleRequest,
  GrantPermissionRequest,
} from '@/types/api';

/**
 * Query keys for role-related queries.
 */
export const roleKeys = {
  all: ['roles'] as const,
  lists: () => [...roleKeys.all, 'list'] as const,
  list: () => [...roleKeys.lists(), 'all'] as const,
  details: () => [...roleKeys.all, 'detail'] as const,
  detail: (id: string) => [...roleKeys.details(), id] as const,
  permissions: () => ['permissions'] as const,
  permissionsByModule: (module?: string) => [...roleKeys.permissions(), module] as const,
};

/**
 * Hook to fetch all roles.
 */
export function useRoles() {
  return useQuery({
    queryKey: roleKeys.list(),
    queryFn: () => rolesService.getAll(),
  });
}

/**
 * Hook to fetch a single role by ID.
 */
export function useRole(id: string) {
  return useQuery({
    queryKey: roleKeys.detail(id),
    queryFn: () => rolesService.getById(id),
    enabled: !!id,
  });
}

/**
 * Hook to fetch all available permissions.
 */
export function usePermissions(module?: string) {
  return useQuery({
    queryKey: roleKeys.permissionsByModule(module),
    queryFn: () => rolesService.getPermissions(module),
  });
}

/**
 * Hook to create a new role.
 */
export function useCreateRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateRoleRequest) => rolesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roleKeys.lists() });
    },
  });
}

/**
 * Hook to update a role.
 */
export function useUpdateRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateRoleRequest }) =>
      rolesService.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: roleKeys.lists() });
    },
  });
}

/**
 * Hook to grant a permission to a role.
 */
export function useGrantPermission() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ roleId, data }: { roleId: string; data: GrantPermissionRequest }) =>
      rolesService.grantPermission(roleId, data),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(roleId) });
    },
  });
}

/**
 * Hook to revoke a permission from a role.
 */
export function useRevokePermission() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ roleId, permissionId }: { roleId: string; permissionId: string }) =>
      rolesService.revokePermission(roleId, permissionId),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(roleId) });
    },
  });
}
