import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { carriersService } from '@/services';
import type {
  CreateCarrierRequest,
  UpdateCarrierRequest,
  CarrierStatus,
  AddProductRequest,
  AddAppetiteRequest,
} from '@/types/api';

/**
 * Query keys for carrier-related queries.
 */
export const carrierKeys = {
  all: ['carriers'] as const,
  lists: () => [...carrierKeys.all, 'list'] as const,
  list: (filters: string) => [...carrierKeys.lists(), filters] as const,
  details: () => [...carrierKeys.all, 'detail'] as const,
  detail: (id: string) => [...carrierKeys.details(), id] as const,
};

/**
 * Hook to fetch all carriers.
 */
export function useCarriers() {
  return useQuery({
    queryKey: carrierKeys.lists(),
    queryFn: () => carriersService.getAll(),
  });
}

/**
 * Hook to fetch a single carrier by ID.
 */
export function useCarrier(id: string) {
  return useQuery({
    queryKey: carrierKeys.detail(id),
    queryFn: () => carriersService.getById(id),
    enabled: !!id,
  });
}

/**
 * Hook to fetch carriers by status.
 */
export function useCarriersByStatus(status: CarrierStatus) {
  return useQuery({
    queryKey: carrierKeys.list(status),
    queryFn: () => carriersService.getByStatus(status),
  });
}

/**
 * Hook to search carriers.
 */
export function useCarrierSearch(searchTerm: string) {
  return useQuery({
    queryKey: carrierKeys.list(`search:${searchTerm}`),
    queryFn: () => carriersService.search(searchTerm),
    enabled: searchTerm.length >= 2,
  });
}

/**
 * Hook to create a new carrier.
 */
export function useCreateCarrier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCarrierRequest) => carriersService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.lists() });
    },
  });
}

/**
 * Hook to update an existing carrier.
 */
export function useUpdateCarrier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCarrierRequest }) =>
      carriersService.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: carrierKeys.lists() });
    },
  });
}

/**
 * Hook to deactivate a carrier.
 */
export function useDeactivateCarrier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string }) =>
      carriersService.deactivate(id, reason),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: carrierKeys.lists() });
    },
  });
}

/**
 * Hook to add a product to a carrier.
 */
export function useAddProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ carrierId, data }: { carrierId: string; data: AddProductRequest }) =>
      carriersService.addProduct(carrierId, data),
    onSuccess: (_, { carrierId }) => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.detail(carrierId) });
    },
  });
}

/**
 * Hook to update a product.
 */
export function useUpdateProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      carrierId,
      productId,
      data,
    }: {
      carrierId: string;
      productId: string;
      data: Partial<AddProductRequest>;
    }) => carriersService.updateProduct(carrierId, productId, data),
    onSuccess: (_, { carrierId }) => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.detail(carrierId) });
    },
  });
}

/**
 * Hook to deactivate a product.
 */
export function useDeactivateProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ carrierId, productId }: { carrierId: string; productId: string }) =>
      carriersService.deactivateProduct(carrierId, productId),
    onSuccess: (_, { carrierId }) => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.detail(carrierId) });
    },
  });
}

/**
 * Hook to add appetite criteria to a carrier.
 */
export function useAddAppetite() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ carrierId, data }: { carrierId: string; data: AddAppetiteRequest }) =>
      carriersService.addAppetite(carrierId, data),
    onSuccess: (_, { carrierId }) => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.detail(carrierId) });
    },
  });
}

/**
 * Hook to update appetite criteria.
 */
export function useUpdateAppetite() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      carrierId,
      appetiteId,
      data,
    }: {
      carrierId: string;
      appetiteId: string;
      data: Partial<AddAppetiteRequest>;
    }) => carriersService.updateAppetite(carrierId, appetiteId, data),
    onSuccess: (_, { carrierId }) => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.detail(carrierId) });
    },
  });
}

/**
 * Hook to deactivate appetite criteria.
 */
export function useDeactivateAppetite() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ carrierId, appetiteId }: { carrierId: string; appetiteId: string }) =>
      carriersService.deactivateAppetite(carrierId, appetiteId),
    onSuccess: (_, { carrierId }) => {
      queryClient.invalidateQueries({ queryKey: carrierKeys.detail(carrierId) });
    },
  });
}
