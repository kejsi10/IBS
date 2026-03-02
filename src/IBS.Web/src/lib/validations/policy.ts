import { z } from 'zod';
import { lineOfBusinessOptions } from './carrier';

/** Billing type options */
export const billingTypeOptions = ['DirectBill', 'AgencyBill'] as const;

/** Payment plan options */
export const paymentPlanOptions = [
  'Annual',
  'SemiAnnual',
  'Quarterly',
  'Monthly',
] as const;

/** Cancellation type options matching backend CancellationType enum */
export const cancellationTypeOptions = [
  'InsuredRequest',
  'CarrierUnderwriting',
  'NonPayment',
  'FlatCancel',
  'Misrepresentation',
] as const;

/**
 * Schema for the client selection step of policy wizard.
 */
export const policyWizardClientStepSchema = z.object({
  clientId: z.string().min(1, 'Client is required'),
});

/**
 * Schema for the carrier selection step of policy wizard.
 */
export const policyWizardCarrierStepSchema = z.object({
  carrierId: z.string().min(1, 'Carrier is required'),
  productId: z.string().min(1, 'Product is required'),
});

/**
 * Schema for the policy details step of policy wizard.
 */
export const policyWizardDetailsStepSchema = z.object({
  policyNumber: z
    .string()
    .max(50, 'Policy number must be 50 characters or less')
    .optional(),
  effectiveDate: z.string().min(1, 'Effective date is required'),
  expirationDate: z.string().min(1, 'Expiration date is required'),
  billingType: z.enum(billingTypeOptions),
  paymentPlan: z.enum(paymentPlanOptions),
  description: z
    .string()
    .max(1000, 'Description must be 1000 characters or less')
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
 * Schema for adding a coverage.
 */
export const addCoverageSchema = z.object({
  type: z.string().min(1, 'Coverage type is required'),
  limit: z.number().min(0, 'Limit must be 0 or greater'),
  deductible: z.number().min(0, 'Deductible must be 0 or greater').optional(),
  premium: z.number().min(0, 'Premium must be 0 or greater'),
  description: z
    .string()
    .max(500, 'Description must be 500 characters or less')
    .optional(),
});

/**
 * Schema for creating a full policy (combined from wizard steps).
 */
export const createPolicySchema = z.object({
  clientId: z.string().min(1, 'Client is required'),
  carrierId: z.string().min(1, 'Carrier is required'),
  productId: z.string().min(1, 'Product is required'),
  lineOfBusiness: z.enum(lineOfBusinessOptions),
  policyNumber: z
    .string()
    .max(50, 'Policy number must be 50 characters or less')
    .optional(),
  effectiveDate: z.string().min(1, 'Effective date is required'),
  expirationDate: z.string().min(1, 'Expiration date is required'),
  billingType: z.enum(billingTypeOptions),
  paymentPlan: z.enum(paymentPlanOptions),
  description: z
    .string()
    .max(1000, 'Description must be 1000 characters or less')
    .optional(),
  coverages: z.array(addCoverageSchema).optional(),
});

/**
 * Schema for cancelling a policy.
 */
export const cancelPolicySchema = z.object({
  cancellationType: z.enum(cancellationTypeOptions),
  cancellationDate: z.string().min(1, 'Cancellation date is required'),
  reason: z
    .string()
    .max(500, 'Reason must be 500 characters or less')
    .optional(),
});

/**
 * Schema for renewing a policy.
 */
export const renewPolicySchema = z.object({
  newEffectiveDate: z.string().min(1, 'New effective date is required'),
  newExpirationDate: z.string().min(1, 'New expiration date is required'),
  premiumChange: z.number().optional(),
}).refine(
  (data) => {
    return new Date(data.newExpirationDate) > new Date(data.newEffectiveDate);
  },
  {
    message: 'New expiration date must be after new effective date',
    path: ['newExpirationDate'],
  }
);

/**
 * Schema for adding an endorsement.
 */
export const addEndorsementSchema = z.object({
  type: z.string().min(1, 'Endorsement type is required'),
  effectiveDate: z.string().min(1, 'Effective date is required'),
  description: z
    .string()
    .min(1, 'Description is required')
    .max(1000, 'Description must be 1000 characters or less'),
  premiumChange: z.number().optional(),
});

/** Type for policy wizard client step form data */
export type PolicyWizardClientStepFormData = z.infer<typeof policyWizardClientStepSchema>;

/** Type for policy wizard carrier step form data */
export type PolicyWizardCarrierStepFormData = z.infer<typeof policyWizardCarrierStepSchema>;

/** Type for policy wizard details step form data */
export type PolicyWizardDetailsStepFormData = z.infer<typeof policyWizardDetailsStepSchema>;

/** Type for add coverage form data */
export type AddCoverageFormData = z.infer<typeof addCoverageSchema>;

/** Type for create policy form data */
export type CreatePolicyFormData = z.infer<typeof createPolicySchema>;

/** Type for cancel policy form data */
export type CancelPolicyFormData = z.infer<typeof cancelPolicySchema>;

/** Type for renew policy form data */
export type RenewPolicyFormData = z.infer<typeof renewPolicySchema>;

/** Type for add endorsement form data */
export type AddEndorsementFormData = z.infer<typeof addEndorsementSchema>;
