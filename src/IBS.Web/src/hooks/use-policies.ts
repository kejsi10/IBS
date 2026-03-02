import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { policiesService } from '@/services';
import type {
  PolicyFilters,
  CreatePolicyRequest,
  CancelPolicyRequest,
  AddCoverageRequest,
  UpdateCoverageRequest,
  AddEndorsementRequest,
  RejectEndorsementRequest,
} from '@/types/api';

/**
 * Query keys for policy-related queries.
 */
export const policyKeys = {
  all: ['policies'] as const,
  lists: () => [...policyKeys.all, 'list'] as const,
  list: (filters: PolicyFilters) => [...policyKeys.lists(), filters] as const,
  details: () => [...policyKeys.all, 'detail'] as const,
  detail: (id: string) => [...policyKeys.details(), id] as const,
  expiring: (startDate: string, endDate: string) =>
    [...policyKeys.all, 'expiring', startDate, endDate] as const,
  dueForRenewal: (days: number) => [...policyKeys.all, 'dueForRenewal', days] as const,
  byClient: (clientId: string) => [...policyKeys.all, 'byClient', clientId] as const,
};

// ========================================
// Query Hooks
// ========================================

/**
 * Hook to fetch a paginated list of policies.
 */
export function usePolicies(filters?: PolicyFilters) {
  return useQuery({
    queryKey: policyKeys.list(filters || {}),
    queryFn: () => policiesService.getAll(filters),
  });
}

/**
 * Hook to fetch a single policy by ID.
 */
export function usePolicy(id: string) {
  return useQuery({
    queryKey: policyKeys.detail(id),
    queryFn: () => policiesService.getById(id),
    enabled: !!id,
  });
}

/**
 * Hook to fetch policies expiring within a date range.
 */
export function useExpiringPolicies(startDate: string, endDate: string) {
  return useQuery({
    queryKey: policyKeys.expiring(startDate, endDate),
    queryFn: () => policiesService.getExpiring(startDate, endDate),
    enabled: !!startDate && !!endDate,
  });
}

/**
 * Hook to fetch policies due for renewal within N days.
 */
export function usePoliciesDueForRenewal(daysUntilExpiration = 60) {
  return useQuery({
    queryKey: policyKeys.dueForRenewal(daysUntilExpiration),
    queryFn: () => policiesService.getDueForRenewal(daysUntilExpiration),
  });
}

/**
 * Hook to fetch all policies for a specific client.
 */
export function useClientPolicies(clientId: string) {
  return useQuery({
    queryKey: policyKeys.byClient(clientId),
    queryFn: () => policiesService.getByClient(clientId),
    enabled: !!clientId,
  });
}

// ========================================
// Policy Lifecycle Mutations
// ========================================

/**
 * Hook to create a new policy.
 */
export function useCreatePolicy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreatePolicyRequest) => policiesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: policyKeys.lists() });
    },
  });
}

/**
 * Hook to bind a draft policy.
 */
export function useBindPolicy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => policiesService.bind(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: policyKeys.lists() });
    },
  });
}

/**
 * Hook to activate (issue) a bound policy.
 */
export function useActivatePolicy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => policiesService.activate(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: policyKeys.lists() });
    },
  });
}

/**
 * Hook to cancel an active policy.
 */
export function useCancelPolicy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CancelPolicyRequest }) =>
      policiesService.cancel(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: policyKeys.lists() });
    },
  });
}

/**
 * Hook to renew an active policy.
 */
export function useRenewPolicy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => policiesService.renew(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: policyKeys.lists() });
    },
  });
}

// ========================================
// Coverage Mutations
// ========================================

/**
 * Hook to add a coverage to a policy.
 */
export function useAddCoverage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, data }: { policyId: string; data: AddCoverageRequest }) =>
      policiesService.addCoverage(policyId, data),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

/**
 * Hook to update a coverage.
 */
export function useUpdateCoverage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      policyId,
      coverageId,
      data,
    }: {
      policyId: string;
      coverageId: string;
      data: UpdateCoverageRequest;
    }) => policiesService.updateCoverage(policyId, coverageId, data),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

/**
 * Hook to remove a coverage from a policy.
 */
export function useRemoveCoverage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, coverageId }: { policyId: string; coverageId: string }) =>
      policiesService.removeCoverage(policyId, coverageId),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

// ========================================
// Endorsement Mutations
// ========================================

/**
 * Hook to add an endorsement to a policy.
 */
export function useAddEndorsement() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, data }: { policyId: string; data: AddEndorsementRequest }) =>
      policiesService.addEndorsement(policyId, data),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

/**
 * Hook to approve a pending endorsement.
 */
export function useApproveEndorsement() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, endorsementId }: { policyId: string; endorsementId: string }) =>
      policiesService.approveEndorsement(policyId, endorsementId),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

/**
 * Hook to issue an approved endorsement.
 */
export function useIssueEndorsement() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ policyId, endorsementId }: { policyId: string; endorsementId: string }) =>
      policiesService.issueEndorsement(policyId, endorsementId),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}

/**
 * Hook to reject a pending endorsement.
 */
export function useRejectEndorsement() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      policyId,
      endorsementId,
      data,
    }: {
      policyId: string;
      endorsementId: string;
      data: RejectEndorsementRequest;
    }) => policiesService.rejectEndorsement(policyId, endorsementId, data),
    onSuccess: (_, { policyId }) => {
      queryClient.invalidateQueries({ queryKey: policyKeys.detail(policyId) });
    },
  });
}
