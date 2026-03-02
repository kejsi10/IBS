import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { claimsService } from '@/services';
import type {
  ClaimFilters,
  CreateClaimRequest,
  UpdateClaimStatusRequest,
  AddClaimNoteRequest,
  SetReserveRequest,
  AuthorizePaymentRequest,
  VoidPaymentRequest,
} from '@/types/api';

/**
 * Query keys for claim-related queries.
 */
export const claimKeys = {
  all: ['claims'] as const,
  lists: () => [...claimKeys.all, 'list'] as const,
  list: (filters: ClaimFilters) => [...claimKeys.lists(), filters] as const,
  details: () => [...claimKeys.all, 'detail'] as const,
  detail: (id: string) => [...claimKeys.details(), id] as const,
  statistics: () => [...claimKeys.all, 'statistics'] as const,
};

/**
 * Hook to fetch a paginated list of claims.
 */
export function useClaims(filters?: ClaimFilters) {
  return useQuery({
    queryKey: claimKeys.list(filters || {}),
    queryFn: () => claimsService.getAll(filters),
  });
}

/**
 * Hook to fetch a single claim by ID.
 */
export function useClaim(id: string) {
  return useQuery({
    queryKey: claimKeys.detail(id),
    queryFn: () => claimsService.getById(id),
    enabled: !!id,
  });
}

/**
 * Hook to fetch claim statistics.
 */
export function useClaimStatistics() {
  return useQuery({
    queryKey: claimKeys.statistics(),
    queryFn: () => claimsService.getStatistics(),
  });
}

/**
 * Hook to create a new claim (FNOL).
 */
export function useCreateClaim() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateClaimRequest) => claimsService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: claimKeys.lists() });
      queryClient.invalidateQueries({ queryKey: claimKeys.statistics() });
    },
  });
}

/**
 * Hook to update claim status.
 */
export function useUpdateClaimStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ claimId, data }: { claimId: string; data: UpdateClaimStatusRequest }) =>
      claimsService.updateStatus(claimId, data),
    onSuccess: (_, { claimId }) => {
      queryClient.invalidateQueries({ queryKey: claimKeys.detail(claimId) });
      queryClient.invalidateQueries({ queryKey: claimKeys.lists() });
      queryClient.invalidateQueries({ queryKey: claimKeys.statistics() });
    },
  });
}

/**
 * Hook to add a note to a claim.
 */
export function useAddClaimNote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ claimId, data }: { claimId: string; data: AddClaimNoteRequest }) =>
      claimsService.addNote(claimId, data),
    onSuccess: (_, { claimId }) => {
      queryClient.invalidateQueries({ queryKey: claimKeys.detail(claimId) });
    },
  });
}

/**
 * Hook to set a reserve on a claim.
 */
export function useSetReserve() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ claimId, data }: { claimId: string; data: SetReserveRequest }) =>
      claimsService.setReserve(claimId, data),
    onSuccess: (_, { claimId }) => {
      queryClient.invalidateQueries({ queryKey: claimKeys.detail(claimId) });
    },
  });
}

/**
 * Hook to authorize a payment on a claim.
 */
export function useAuthorizePayment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ claimId, data }: { claimId: string; data: AuthorizePaymentRequest }) =>
      claimsService.authorizePayment(claimId, data),
    onSuccess: (_, { claimId }) => {
      queryClient.invalidateQueries({ queryKey: claimKeys.detail(claimId) });
    },
  });
}

/**
 * Hook to issue a payment.
 */
export function useIssuePayment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ claimId, paymentId }: { claimId: string; paymentId: string }) =>
      claimsService.issuePayment(claimId, paymentId),
    onSuccess: (_, { claimId }) => {
      queryClient.invalidateQueries({ queryKey: claimKeys.detail(claimId) });
    },
  });
}

/**
 * Hook to void a payment.
 */
export function useVoidPayment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ claimId, paymentId, data }: { claimId: string; paymentId: string; data: VoidPaymentRequest }) =>
      claimsService.voidPayment(claimId, paymentId, data),
    onSuccess: (_, { claimId }) => {
      queryClient.invalidateQueries({ queryKey: claimKeys.detail(claimId) });
    },
  });
}
