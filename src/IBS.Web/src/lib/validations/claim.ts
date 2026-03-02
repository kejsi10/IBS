import { z } from 'zod';

/**
 * Loss type options for select inputs.
 */
export const lossTypeOptions = [
  'PropertyDamage',
  'Liability',
  'WorkersComp',
  'Auto',
  'Professional',
  'Cyber',
  'NaturalDisaster',
  'TheftFraud',
  'BodilyInjury',
  'Other',
] as const;

/**
 * Schema for creating a new claim (FNOL).
 */
export const createClaimSchema = z.object({
  policyId: z.string().min(1, 'Policy is required'),
  clientId: z.string().min(1, 'Client is required'),
  lossDate: z.string().min(1, 'Loss date is required'),
  reportedDate: z.string().min(1, 'Reported date is required'),
  lossType: z.enum(lossTypeOptions),
  lossDescription: z
    .string()
    .min(10, 'Description must be at least 10 characters')
    .max(4000, 'Description must be 4000 characters or less'),
  estimatedLossAmount: z
    .number()
    .min(0.01, 'Amount must be greater than zero')
    .optional(),
  estimatedLossCurrency: z.string().length(3, 'Currency must be a 3-letter code').default('USD'),
}).refine(
  (data) => {
    if (!data.lossDate || !data.reportedDate) return true;
    return new Date(data.lossDate) <= new Date(data.reportedDate);
  },
  {
    message: 'Loss date must be on or before reported date',
    path: ['lossDate'],
  }
);

/**
 * Schema for adding a note to a claim.
 */
export const addClaimNoteSchema = z.object({
  content: z
    .string()
    .min(1, 'Note content is required')
    .max(4000, 'Note must be 4000 characters or less'),
  isInternal: z.boolean().default(false),
});

/**
 * Schema for setting a reserve.
 */
export const setReserveSchema = z.object({
  reserveType: z.string().min(1, 'Reserve type is required'),
  amount: z.number().min(0.01, 'Amount must be greater than zero'),
  currency: z.string().length(3, 'Currency must be a 3-letter code').default('USD'),
  notes: z.string().max(2000, 'Notes must be 2000 characters or less').optional(),
});

/**
 * Schema for authorizing a payment.
 */
export const authorizePaymentSchema = z.object({
  paymentType: z.string().min(1, 'Payment type is required'),
  amount: z.number().min(0.01, 'Amount must be greater than zero'),
  currency: z.string().length(3, 'Currency must be a 3-letter code').default('USD'),
  payeeName: z.string().min(1, 'Payee name is required'),
  paymentDate: z.string().min(1, 'Payment date is required'),
  checkNumber: z.string().optional(),
});

/**
 * Schema for voiding a payment.
 */
export const voidPaymentSchema = z.object({
  reason: z
    .string()
    .min(1, 'Void reason is required')
    .max(1000, 'Reason must be 1000 characters or less'),
});

/** Type for create claim form data */
export type CreateClaimFormData = z.infer<typeof createClaimSchema>;

/** Type for add note form data */
export type AddClaimNoteFormData = z.infer<typeof addClaimNoteSchema>;

/** Type for set reserve form data */
export type SetReserveFormData = z.infer<typeof setReserveSchema>;

/** Type for authorize payment form data */
export type AuthorizePaymentFormData = z.infer<typeof authorizePaymentSchema>;

/** Type for void payment form data */
export type VoidPaymentFormData = z.infer<typeof voidPaymentSchema>;
