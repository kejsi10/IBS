import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { quotesService } from '@/services';
import type {
  QuoteFilters,
  CreateQuoteRequest,
  AddCarrierToQuoteRequest,
  RecordQuoteResponseRequest,
} from '@/types/api';

/**
 * Query keys for quote-related queries.
 */
export const quoteKeys = {
  all: ['quotes'] as const,
  lists: () => [...quoteKeys.all, 'list'] as const,
  list: (filters: QuoteFilters) => [...quoteKeys.lists(), filters] as const,
  details: () => [...quoteKeys.all, 'detail'] as const,
  detail: (id: string) => [...quoteKeys.details(), id] as const,
  summary: () => [...quoteKeys.all, 'summary'] as const,
  byClient: (clientId: string) => [...quoteKeys.all, 'byClient', clientId] as const,
};

// ========================================
// Query Hooks
// ========================================

/**
 * Hook to fetch a paginated list of quotes.
 */
export function useQuotes(filters?: QuoteFilters) {
  return useQuery({
    queryKey: quoteKeys.list(filters || {}),
    queryFn: () => quotesService.getAll(filters),
  });
}

/**
 * Hook to fetch a single quote by ID.
 */
export function useQuote(id: string) {
  return useQuery({
    queryKey: quoteKeys.detail(id),
    queryFn: () => quotesService.getById(id),
    enabled: !!id,
  });
}

/**
 * Hook to fetch quote summary statistics.
 */
export function useQuotesSummary() {
  return useQuery({
    queryKey: quoteKeys.summary(),
    queryFn: () => quotesService.getSummary(),
  });
}

/**
 * Hook to fetch quotes for a specific client.
 */
export function useClientQuotes(clientId: string) {
  return useQuery({
    queryKey: quoteKeys.byClient(clientId),
    queryFn: () => quotesService.getByClient(clientId),
    enabled: !!clientId,
  });
}

// ========================================
// Quote Lifecycle Mutations
// ========================================

/**
 * Hook to create a new quote.
 */
export function useCreateQuote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateQuoteRequest) => quotesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: quoteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: quoteKeys.summary() });
    },
  });
}

/**
 * Hook to add a carrier to a quote.
 */
export function useAddCarrierToQuote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ quoteId, data }: { quoteId: string; data: AddCarrierToQuoteRequest }) =>
      quotesService.addCarrier(quoteId, data),
    onSuccess: (_, { quoteId }) => {
      queryClient.invalidateQueries({ queryKey: quoteKeys.detail(quoteId) });
    },
  });
}

/**
 * Hook to remove a carrier from a quote.
 */
export function useRemoveCarrierFromQuote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ quoteId, quoteCarrierId }: { quoteId: string; quoteCarrierId: string }) =>
      quotesService.removeCarrier(quoteId, quoteCarrierId),
    onSuccess: (_, { quoteId }) => {
      queryClient.invalidateQueries({ queryKey: quoteKeys.detail(quoteId) });
    },
  });
}

/**
 * Hook to submit a quote to carriers.
 */
export function useSubmitQuote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (quoteId: string) => quotesService.submit(quoteId),
    onSuccess: (_, quoteId) => {
      queryClient.invalidateQueries({ queryKey: quoteKeys.detail(quoteId) });
      queryClient.invalidateQueries({ queryKey: quoteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: quoteKeys.summary() });
    },
  });
}

/**
 * Hook to record a carrier's response.
 */
export function useRecordQuoteResponse() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      quoteId,
      quoteCarrierId,
      data,
    }: {
      quoteId: string;
      quoteCarrierId: string;
      data: RecordQuoteResponseRequest;
    }) => quotesService.recordResponse(quoteId, quoteCarrierId, data),
    onSuccess: (_, { quoteId }) => {
      queryClient.invalidateQueries({ queryKey: quoteKeys.detail(quoteId) });
      queryClient.invalidateQueries({ queryKey: quoteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: quoteKeys.summary() });
    },
  });
}

/**
 * Hook to accept a carrier's quote.
 */
export function useAcceptQuoteCarrier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ quoteId, quoteCarrierId }: { quoteId: string; quoteCarrierId: string }) =>
      quotesService.acceptCarrier(quoteId, quoteCarrierId),
    onSuccess: (_, { quoteId }) => {
      queryClient.invalidateQueries({ queryKey: quoteKeys.detail(quoteId) });
      queryClient.invalidateQueries({ queryKey: quoteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: quoteKeys.summary() });
    },
  });
}

/**
 * Hook to cancel a quote.
 */
export function useCancelQuote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (quoteId: string) => quotesService.cancel(quoteId),
    onSuccess: (_, quoteId) => {
      queryClient.invalidateQueries({ queryKey: quoteKeys.detail(quoteId) });
      queryClient.invalidateQueries({ queryKey: quoteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: quoteKeys.summary() });
    },
  });
}
