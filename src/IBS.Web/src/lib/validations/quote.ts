import { z } from 'zod';
import { lineOfBusinessOptions } from './carrier';

/**
 * Schema for creating a new quote.
 */
export const createQuoteSchema = z.object({
  clientId: z.string().min(1, 'Client is required'),
  lineOfBusiness: z.enum(lineOfBusinessOptions),
  effectiveDate: z.string().min(1, 'Effective date is required'),
  expirationDate: z.string().min(1, 'Expiration date is required'),
  notes: z
    .string()
    .max(2000, 'Notes must be 2000 characters or less')
    .optional(),
}).refine(
  (data) => {
    if (!data.effectiveDate || !data.expirationDate) return true;
    return new Date(data.expirationDate) > new Date(data.effectiveDate);
  },
  {
    message: 'Expiration date must be after effective date',
    path: ['expirationDate'],
  }
);

/**
 * Schema for recording a quoted response.
 */
export const recordQuotedResponseSchema = z.object({
  premiumAmount: z
    .number()
    .min(0.01, 'Premium amount must be greater than zero'),
  premiumCurrency: z.string().length(3, 'Currency must be a 3-letter code').default('USD'),
  conditions: z
    .string()
    .max(2000, 'Conditions must be 2000 characters or less')
    .optional(),
  proposedCoverages: z.string().optional(),
  carrierExpiresAt: z.string().optional(),
});

/**
 * Schema for recording a declined response.
 */
export const recordDeclinedResponseSchema = z.object({
  declinationReason: z
    .string()
    .max(500, 'Reason must be 500 characters or less')
    .optional(),
});

/** Type for create quote form data */
export type CreateQuoteFormData = z.infer<typeof createQuoteSchema>;

/** Type for record quoted response form data */
export type RecordQuotedResponseFormData = z.infer<typeof recordQuotedResponseSchema>;

/** Type for record declined response form data */
export type RecordDeclinedResponseFormData = z.infer<typeof recordDeclinedResponseSchema>;
