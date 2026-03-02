import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { clientsService } from '@/services';
import type {
  ClientFilters,
  CreateIndividualClientRequest,
  CreateBusinessClientRequest,
  UpdateClientRequest,
  AddContactRequest,
  AddAddressRequest,
} from '@/types/api';

/**
 * Query keys for client-related queries.
 */
export const clientKeys = {
  all: ['clients'] as const,
  lists: () => [...clientKeys.all, 'list'] as const,
  list: (filters: ClientFilters) => [...clientKeys.lists(), filters] as const,
  details: () => [...clientKeys.all, 'detail'] as const,
  detail: (id: string) => [...clientKeys.details(), id] as const,
};

/**
 * Hook to fetch a paginated list of clients.
 */
export function useClients(filters?: ClientFilters) {
  return useQuery({
    queryKey: clientKeys.list(filters || {}),
    queryFn: () => clientsService.getAll(filters),
  });
}

/**
 * Hook to fetch a single client by ID.
 */
export function useClient(id: string) {
  return useQuery({
    queryKey: clientKeys.detail(id),
    queryFn: () => clientsService.getById(id),
    enabled: !!id,
  });
}

/**
 * Hook to create a new individual client.
 */
export function useCreateIndividualClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateIndividualClientRequest) => clientsService.createIndividual(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clientKeys.lists() });
    },
  });
}

/**
 * Hook to create a new business client.
 */
export function useCreateBusinessClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateBusinessClientRequest) => clientsService.createBusiness(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clientKeys.lists() });
    },
  });
}

/**
 * Hook to update an existing client.
 */
export function useUpdateClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateClientRequest }) =>
      clientsService.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: clientKeys.lists() });
    },
  });
}

/**
 * Hook to deactivate a client.
 */
export function useDeactivateClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => clientsService.deactivate(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: clientKeys.lists() });
    },
  });
}

/**
 * Hook to reactivate a deactivated client.
 */
export function useReactivateClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => clientsService.reactivate(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: clientKeys.lists() });
    },
  });
}

/**
 * Hook to add a contact to a client.
 */
export function useAddContact() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ clientId, data }: { clientId: string; data: AddContactRequest }) =>
      clientsService.addContact(clientId, data),
    onSuccess: (_, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
    },
  });
}

/**
 * Hook to update a contact.
 */
export function useUpdateContact() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      clientId,
      contactId,
      data,
    }: {
      clientId: string;
      contactId: string;
      data: Partial<AddContactRequest>;
    }) => clientsService.updateContact(clientId, contactId, data),
    onSuccess: (_, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
    },
  });
}

/**
 * Hook to set a contact as the primary contact for a client.
 */
export function useSetPrimaryContact() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ clientId, contactId }: { clientId: string; contactId: string }) =>
      clientsService.setPrimaryContact(clientId, contactId),
    onSuccess: (_, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
    },
  });
}

/**
 * Hook to remove a contact from a client.
 */
export function useRemoveContact() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ clientId, contactId }: { clientId: string; contactId: string }) =>
      clientsService.removeContact(clientId, contactId),
    onSuccess: (_, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
    },
  });
}

/**
 * Hook to add an address to a client.
 */
export function useAddAddress() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ clientId, data }: { clientId: string; data: AddAddressRequest }) =>
      clientsService.addAddress(clientId, data),
    onSuccess: (_, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
    },
  });
}

/**
 * Hook to update an address.
 */
export function useUpdateAddress() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      clientId,
      addressId,
      data,
    }: {
      clientId: string;
      addressId: string;
      data: Partial<AddAddressRequest>;
    }) => clientsService.updateAddress(clientId, addressId, data),
    onSuccess: (_, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
    },
  });
}

/**
 * Hook to set an address as the primary address for a client.
 */
export function useSetPrimaryAddress() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ clientId, addressId }: { clientId: string; addressId: string }) =>
      clientsService.setPrimaryAddress(clientId, addressId),
    onSuccess: (_, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
    },
  });
}

/**
 * Hook to remove an address from a client.
 */
export function useRemoveAddress() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ clientId, addressId }: { clientId: string; addressId: string }) =>
      clientsService.removeAddress(clientId, addressId),
    onSuccess: (_, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
    },
  });
}
