import { api } from '@/lib/api';
import type {
  ConversationDto,
  ConversationDetailsDto,
  ConversationMode,
  SendMessageResponse,
} from '../types';

/**
 * Service for Policy Assistant API operations.
 */
export const policyAssistantService = {
  /**
   * Creates a new conversation.
   * @param mode - Guided or Freeform.
   * @param lineOfBusiness - Optional line of business to pre-populate.
   * @param clientId - Optional client ID.
   * @param carrierId - Optional carrier ID.
   */
  async createConversation(
    mode: ConversationMode,
    lineOfBusiness?: string,
    clientId?: string,
    carrierId?: string
  ): Promise<ConversationDto> {
    const response = await api.post<ConversationDto>('/policy-assistant/conversations', {
      mode,
      lineOfBusiness,
      clientId,
      carrierId,
    });
    return response.data;
  },

  /**
   * Gets the list of all conversations.
   */
  async getConversations(): Promise<ConversationDto[]> {
    const response = await api.get<ConversationDto[]>('/policy-assistant/conversations');
    return response.data;
  },

  /**
   * Gets full details for a single conversation.
   * @param id - Conversation ID.
   */
  async getConversation(id: string): Promise<ConversationDetailsDto> {
    const response = await api.get<ConversationDetailsDto>(`/policy-assistant/conversations/${id}`);
    return response.data;
  },

  /**
   * Sends a chat message to a conversation.
   * @param conversationId - Conversation ID.
   * @param content - The message text.
   */
  async sendMessage(conversationId: string, content: string): Promise<SendMessageResponse> {
    const response = await api.post<SendMessageResponse>(
      `/policy-assistant/conversations/${conversationId}/messages`,
      { content }
    );
    return response.data;
  },

  /**
   * Creates a policy from the extracted conversation data.
   * @param conversationId - Conversation ID.
   * @param clientId - Client ID to associate with the policy.
   * @param carrierId - Carrier ID to associate with the policy.
   * @param overrides - User-confirmed values that override AI-extracted data.
   */
  async createPolicy(
    conversationId: string,
    clientId: string,
    carrierId: string,
    overrides?: {
      lineOfBusiness?: string;
      effectiveDate?: string;
      expirationDate?: string;
      billingType?: string;
      paymentPlan?: string;
    }
  ): Promise<{ policyId: string }> {
    const response = await api.post<{ policyId: string }>(
      `/policy-assistant/conversations/${conversationId}/create-policy`,
      { clientId, carrierId, ...overrides }
    );
    return response.data;
  },

  /**
   * Abandons (soft-deletes) a conversation.
   * @param conversationId - Conversation ID.
   */
  async abandonConversation(conversationId: string): Promise<void> {
    await api.delete(`/policy-assistant/conversations/${conversationId}`);
  },
};
