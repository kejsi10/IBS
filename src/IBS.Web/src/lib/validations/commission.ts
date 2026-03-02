import { z } from 'zod';

/** Schema for creating a commission schedule. */
export const createScheduleSchema = z.object({
  carrierId: z.string().min(1, 'Carrier is required'),
  carrierName: z.string().min(1, 'Carrier name is required').max(200),
  lineOfBusiness: z.string().min(1, 'Line of business is required').max(100),
  newBusinessRate: z.number().min(0, 'Must be >= 0').max(100, 'Must be <= 100'),
  renewalRate: z.number().min(0, 'Must be >= 0').max(100, 'Must be <= 100'),
  effectiveFrom: z.string().min(1, 'Start date is required'),
  effectiveTo: z.string().optional(),
});

/** Schema for updating a commission schedule. */
export const updateScheduleSchema = createScheduleSchema.omit({ carrierId: true });

/** Schema for creating a commission statement. */
export const createStatementSchema = z.object({
  carrierId: z.string().min(1, 'Carrier is required'),
  carrierName: z.string().min(1, 'Carrier name is required').max(200),
  statementNumber: z.string().min(1, 'Statement number is required').max(50),
  periodMonth: z.number().min(1).max(12),
  periodYear: z.number().min(2000).max(2100),
  statementDate: z.string().min(1, 'Statement date is required'),
  totalPremium: z.number().min(0, 'Must be >= 0'),
  totalPremiumCurrency: z.string().min(1),
  totalCommission: z.number().min(0, 'Must be >= 0'),
  totalCommissionCurrency: z.string().min(1),
});

/** Schema for adding a line item. */
export const addLineItemSchema = z.object({
  policyNumber: z.string().min(1, 'Policy number is required').max(50),
  insuredName: z.string().min(1, 'Insured name is required').max(200),
  lineOfBusiness: z.string().min(1, 'Line of business is required').max(100),
  effectiveDate: z.string().min(1, 'Effective date is required'),
  transactionType: z.enum(['NewBusiness', 'Renewal', 'Endorsement', 'Cancellation', 'Chargeback']),
  grossPremium: z.number(),
  grossPremiumCurrency: z.string().min(1),
  commissionRate: z.number().min(0).max(100),
  commissionAmount: z.number(),
  commissionAmountCurrency: z.string().min(1),
  policyId: z.string().optional(),
});

/** Schema for disputing a line item. */
export const disputeLineItemSchema = z.object({
  reason: z.string().min(1, 'Dispute reason is required').max(2000),
});

/** Schema for adding a producer split. */
export const addProducerSplitSchema = z.object({
  producerName: z.string().min(1, 'Producer name is required').max(200),
  producerId: z.string().min(1, 'Producer ID is required'),
  splitPercentage: z.number().gt(0, 'Must be > 0').max(100, 'Must be <= 100'),
  splitAmount: z.number().min(0, 'Must be >= 0'),
  splitAmountCurrency: z.string().min(1),
});

export type CreateScheduleFormData = z.infer<typeof createScheduleSchema>;
export type UpdateScheduleFormData = z.infer<typeof updateScheduleSchema>;
export type CreateStatementFormData = z.infer<typeof createStatementSchema>;
export type AddLineItemFormData = z.infer<typeof addLineItemSchema>;
export type DisputeLineItemFormData = z.infer<typeof disputeLineItemSchema>;
export type AddProducerSplitFormData = z.infer<typeof addProducerSplitSchema>;
