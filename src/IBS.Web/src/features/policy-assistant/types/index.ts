/**
 * Conversation interaction mode — guided uses structured Q&A, freeform allows open-ended input.
 */
export type ConversationMode = 'Guided' | 'Freeform';

/**
 * Current status of a conversation.
 */
export type ConversationStatus = 'Active' | 'PolicyCreated' | 'Abandoned';

/**
 * Type of a chat message.
 */
export type MessageType = 'Chat' | 'PolicyExtraction' | 'Validation';

/**
 * Summary DTO for a policy assistant conversation.
 */
export interface ConversationDto {
  /** Unique conversation identifier. */
  id: string;
  /** Display title for the conversation. */
  title: string;
  /** Interaction mode. */
  mode: ConversationMode;
  /** Current status. */
  status: ConversationStatus;
  /** Optional line of business. */
  lineOfBusiness?: string;
  /** ID of the created policy, if any. */
  policyId?: string;
  /** Total number of messages. */
  messageCount: number;
  /** ISO timestamp of creation. */
  createdAt: string;
  /** ISO timestamp of last update. */
  updatedAt: string;
}

/**
 * A single chat message DTO.
 */
export interface ChatMessageDto {
  /** Unique message identifier. */
  id: string;
  /** Message author role. */
  role: 'user' | 'assistant';
  /** Message text content. */
  content: string;
  /** Message type classification. */
  messageType: MessageType;
  /** ISO timestamp of creation. */
  createdAt: string;
}

/**
 * An extracted coverage from the policy conversation.
 */
export interface ExtractedCoverage {
  /** Coverage code. */
  code?: string;
  /** Coverage name. */
  name?: string;
  /** Premium amount. */
  premium?: string;
  /** Coverage limit. */
  limit?: string;
  /** Deductible amount. */
  deductible?: string;
}

/**
 * Result of AI policy data extraction.
 */
export interface PolicyExtractionResult {
  /** Whether all required fields have been extracted. */
  isComplete: boolean;
  /** Client name. */
  clientName?: string;
  /** Carrier name. */
  carrierName?: string;
  /** Line of business. */
  lineOfBusiness?: string;
  /** Policy type. */
  policyType?: string;
  /** Effective date string. */
  effectiveDate?: string;
  /** Expiration date string. */
  expirationDate?: string;
  /** Billing type. */
  billingType?: string;
  /** Payment plan. */
  paymentPlan?: string;
  /** Extracted coverages. */
  coverages: ExtractedCoverage[];
  /** Names of fields still missing. */
  missingFields: string[];
}

/**
 * A single validation issue found in the extracted policy data.
 */
export interface ValidationIssue {
  /** Field name. */
  field: string;
  /** Rule that was violated. */
  rule: string;
  /** Human-readable description. */
  description: string;
  /** Severity level (e.g., Error, Warning). */
  severity: string;
}

/**
 * Result of validating extracted policy data against business rules.
 */
export interface PolicyValidationResult {
  /** Whether the policy data passes all validations. */
  isValid: boolean;
  /** List of validation issues. */
  issues: ValidationIssue[];
  /** Warning messages. */
  warnings: string[];
  /** Validation summary text. */
  summary: string;
}

/**
 * Full conversation details including messages and extracted data.
 */
export interface ConversationDetailsDto {
  /** Unique conversation identifier. */
  id: string;
  /** Display title for the conversation. */
  title: string;
  /** Interaction mode. */
  mode: ConversationMode;
  /** Current status. */
  status: ConversationStatus;
  /** Optional line of business. */
  lineOfBusiness?: string;
  /** ID of the created policy, if any. */
  policyId?: string;
  /** All messages in the conversation. */
  messages: ChatMessageDto[];
  /** Latest extracted policy data, if any. */
  extractedData?: PolicyExtractionResult;
  /** ISO timestamp of creation. */
  createdAt: string;
  /** ISO timestamp of last update. */
  updatedAt: string;
}

/**
 * Response from sending a message.
 * Matches SendMessageResponseDto on the backend.
 */
export interface SendMessageResponse {
  /** The assistant message identifier. */
  messageId: string;
  /** The assistant response text. */
  content: string;
  /** Updated extraction result, if extraction was triggered. */
  extraction?: PolicyExtractionResult;
  /** Updated validation result, if validation was triggered. */
  validation?: PolicyValidationResult;
}
