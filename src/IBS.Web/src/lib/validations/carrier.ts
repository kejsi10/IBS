import { z } from 'zod';

/**
 * Schema for creating a carrier.
 */
export const createCarrierSchema = z.object({
  name: z
    .string()
    .min(1, 'Carrier name is required')
    .max(200, 'Carrier name must be 200 characters or less'),
  code: z
    .string()
    .min(1, 'Carrier code is required')
    .max(20, 'Carrier code must be 20 characters or less')
    .regex(/^[A-Z0-9_-]+$/, 'Code must contain only uppercase letters, numbers, hyphens, and underscores'),
  amBestRating: z
    .string()
    .max(10, 'AM Best rating must be 10 characters or less')
    .optional(),
  naicCode: z
    .string()
    .max(10, 'NAIC code must be 10 characters or less')
    .optional(),
  website: z
    .string()
    .url('Invalid website URL')
    .optional()
    .or(z.literal('')),
});

/**
 * Schema for updating a carrier.
 */
export const updateCarrierSchema = z.object({
  name: z
    .string()
    .min(1, 'Carrier name is required')
    .max(200, 'Carrier name must be 200 characters or less')
    .optional(),
  amBestRating: z
    .string()
    .max(10, 'AM Best rating must be 10 characters or less')
    .optional(),
  naicCode: z
    .string()
    .max(10, 'NAIC code must be 10 characters or less')
    .optional(),
  website: z
    .string()
    .url('Invalid website URL')
    .optional()
    .or(z.literal('')),
});

/** Line of business options matching backend LineOfBusiness enum */
export const lineOfBusinessOptions = [
  'PersonalAuto',
  'Homeowners',
  'Renters',
  'PersonalUmbrella',
  'Life',
  'Health',
  'GeneralLiability',
  'CommercialProperty',
  'WorkersCompensation',
  'CommercialAuto',
  'ProfessionalLiability',
  'DirectorsAndOfficers',
  'CyberLiability',
  'BusinessOwnersPolicy',
  'CommercialUmbrella',
  'InlandMarine',
  'Surety',
] as const;

/**
 * Schema for adding a product to a carrier.
 */
export const addProductSchema = z.object({
  name: z
    .string()
    .min(1, 'Product name is required')
    .max(200, 'Product name must be 200 characters or less'),
  lineOfBusiness: z.enum(lineOfBusinessOptions, {
    message: 'Line of business is required',
  }),
  code: z
    .string()
    .max(50, 'Product code must be 50 characters or less')
    .optional(),
  description: z
    .string()
    .max(1000, 'Description must be 1000 characters or less')
    .optional(),
});

/**
 * Schema for adding an appetite to a carrier.
 */
export const addAppetiteSchema = z.object({
  lineOfBusiness: z.enum(lineOfBusinessOptions, {
    message: 'Line of business is required',
  }),
  isPreferred: z.boolean().optional(),
  minimumPremium: z
    .number()
    .min(0, 'Minimum premium must be 0 or greater')
    .optional(),
  maximumPremium: z
    .number()
    .min(0, 'Maximum premium must be 0 or greater')
    .optional(),
  targetIndustries: z
    .string()
    .max(500, 'Target industries must be 500 characters or less')
    .optional(),
  excludedIndustries: z
    .string()
    .max(500, 'Excluded industries must be 500 characters or less')
    .optional(),
  notes: z
    .string()
    .max(1000, 'Notes must be 1000 characters or less')
    .optional(),
});

/** Type for create carrier form data */
export type CreateCarrierFormData = z.infer<typeof createCarrierSchema>;

/** Type for update carrier form data */
export type UpdateCarrierFormData = z.infer<typeof updateCarrierSchema>;

/** Type for add product form data */
export type AddProductFormData = z.infer<typeof addProductSchema>;

/** Type for add appetite form data */
export type AddAppetiteFormData = z.infer<typeof addAppetiteSchema>;
