import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { policyAssistantService } from '../services/policy-assistant.service';
import type { ConversationMode } from '../types';

/**
 * Query keys for policy assistant queries.
 */
export const conversationKeys = {
  all: ['policy-assistant'] as const,
  lists: () => [...conversationKeys.all, 'list'] as const,
  details: () => [...conversationKeys.all, 'detail'] as const,
  detail: (id: string) => [...conversationKeys.details(), id] as const,
};

/**
 * Hook to fetch the list of all conversations.
 */
export function useConversations() {
  return useQuery({
    queryKey: conversationKeys.lists(),
    queryFn: () => policyAssistantService.getConversations(),
  });
}

/**
 * Hook to fetch a single conversation by ID.
 * @param id - Conversation ID (query is disabled when falsy).
 */
export function useConversation(id: string | undefined) {
  return useQuery({
    queryKey: conversationKeys.detail(id ?? ''),
    queryFn: () => policyAssistantService.getConversation(id!),
    enabled: !!id,
  });
}

/**
 * Hook to create a new conversation.
 */
export function useCreateConversation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      mode,
      lineOfBusiness,
      clientId,
      carrierId,
    }: {
      mode: ConversationMode;
      lineOfBusiness?: string;
      clientId?: string;
      carrierId?: string;
    }) => policyAssistantService.createConversation(mode, lineOfBusiness, clientId, carrierId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: conversationKeys.lists() });
    },
  });
}

/**
 * Hook to send a message to a conversation.
 */
export function useSendMessage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ conversationId, content }: { conversationId: string; content: string }) =>
      policyAssistantService.sendMessage(conversationId, content),
    onSuccess: (_, { conversationId }) => {
      queryClient.invalidateQueries({ queryKey: conversationKeys.detail(conversationId) });
    },
  });
}

/**
 * Hook to create a policy from a completed conversation.
 */
export function useCreatePolicyFromConversation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      conversationId,
      clientId,
      carrierId,
      overrides,
    }: {
      conversationId: string;
      clientId: string;
      carrierId: string;
      overrides?: {
        lineOfBusiness?: string;
        effectiveDate?: string;
        expirationDate?: string;
        billingType?: string;
        paymentPlan?: string;
      };
    }) => policyAssistantService.createPolicy(conversationId, clientId, carrierId, overrides),
    onSuccess: (_, { conversationId }) => {
      queryClient.invalidateQueries({ queryKey: conversationKeys.detail(conversationId) });
      queryClient.invalidateQueries({ queryKey: conversationKeys.lists() });
    },
  });
}

/**
 * Hook to abandon a conversation.
 */
export function useAbandonConversation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (conversationId: string) =>
      policyAssistantService.abandonConversation(conversationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: conversationKeys.lists() });
    },
  });
}
