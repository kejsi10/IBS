import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tenantService } from '@/services/tenant.service';
import type {
  CreateTenantRequest,
  UpdateTenantRequest,
  UpdateSubscriptionRequest,
  AddTenantCarrierRequest,
} from '@/types/tenant';

/**
 * Query keys for tenant-related queries.
 */
export const tenantKeys = {
  all: ['tenants'] as const,
  lists: () => [...tenantKeys.all, 'list'] as const,
  list: (params: Record<string, unknown>) => [...tenantKeys.lists(), params] as const,
  details: () => [...tenantKeys.all, 'detail'] as const,
  detail: (id: string) => [...tenantKeys.details(), id] as const,
};

/**
 * Hook to fetch a paginated list of tenants.
 */
export function useTenants(params?: { page?: number; pageSize?: number; search?: string }) {
  return useQuery({
    queryKey: tenantKeys.list(params || {}),
    queryFn: () => tenantService.getAll(params),
  });
}

/**
 * Hook to fetch a single tenant by ID.
 */
export function useTenant(id: string) {
  return useQuery({
    queryKey: tenantKeys.detail(id),
    queryFn: () => tenantService.getById(id),
    enabled: !!id,
  });
}

/**
 * Hook to create a new tenant.
 */
export function useCreateTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateTenantRequest) => tenantService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

/**
 * Hook to update a tenant.
 */
export function useUpdateTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTenantRequest }) =>
      tenantService.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

/**
 * Hook to suspend a tenant.
 */
export function useSuspendTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => tenantService.suspend(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

/**
 * Hook to activate a tenant.
 */
export function useActivateTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => tenantService.activate(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

/**
 * Hook to cancel a tenant.
 */
export function useCancelTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => tenantService.cancel(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

/**
 * Hook to update a tenant's subscription.
 */
export function useUpdateSubscription() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSubscriptionRequest }) =>
      tenantService.updateSubscription(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

/**
 * Hook to add a carrier to a tenant.
 */
export function useAddTenantCarrier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AddTenantCarrierRequest }) =>
      tenantService.addCarrier(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.detail(id) });
    },
  });
}

/**
 * Hook to remove a carrier from a tenant.
 */
export function useRemoveTenantCarrier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, carrierId }: { id: string; carrierId: string }) =>
      tenantService.removeCarrier(id, carrierId),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.detail(id) });
    },
  });
}
