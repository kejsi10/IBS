import { z } from 'zod';

/** Phone number regex pattern */
const phoneRegex = /^\+?[\d\s\-().]+$/;

/** Email validation schema */
const emailSchema = z.string().email('Invalid email address');

/** Phone validation schema */
const phoneSchema = z
  .string()
  .regex(phoneRegex, 'Invalid phone number')
  .optional()
  .or(z.literal(''));

/**
 * Schema for creating an individual client.
 */
export const createIndividualClientSchema = z.object({
  firstName: z
    .string()
    .min(1, 'First name is required')
    .max(100, 'First name must be 100 characters or less'),
  lastName: z
    .string()
    .min(1, 'Last name is required')
    .max(100, 'Last name must be 100 characters or less'),
  email: emailSchema,
  phone: phoneSchema,
  dateOfBirth: z.string().optional(),
});

/**
 * Schema for creating a business client.
 */
export const createBusinessClientSchema = z.object({
  legalName: z
    .string()
    .min(1, 'Legal name is required')
    .max(200, 'Legal name must be 200 characters or less'),
  dba: z.string().max(200, 'DBA must be 200 characters or less').optional(),
  taxId: z
    .string()
    .max(20, 'Tax ID must be 20 characters or less')
    .optional(),
  email: emailSchema,
  phone: phoneSchema,
});

/**
 * Schema for updating a client.
 */
export const updateClientSchema = z.object({
  email: emailSchema.optional(),
  phone: phoneSchema,
});

/**
 * Schema for adding a contact to a client.
 */
export const addContactSchema = z.object({
  firstName: z
    .string()
    .min(1, 'First name is required')
    .max(100, 'First name must be 100 characters or less'),
  lastName: z
    .string()
    .min(1, 'Last name is required')
    .max(100, 'Last name must be 100 characters or less'),
  email: emailSchema.optional().or(z.literal('')),
  phone: phoneSchema,
  role: z.string().max(50, 'Role must be 50 characters or less').optional(),
  isPrimary: z.boolean().optional(),
});

/**
 * Schema for adding an address to a client.
 */
export const addAddressSchema = z.object({
  type: z.enum(['Mailing', 'Physical', 'Billing']),
  line1: z
    .string()
    .min(1, 'Address line 1 is required')
    .max(200, 'Address line 1 must be 200 characters or less'),
  line2: z
    .string()
    .max(200, 'Address line 2 must be 200 characters or less')
    .optional(),
  city: z
    .string()
    .min(1, 'City is required')
    .max(100, 'City must be 100 characters or less'),
  state: z
    .string()
    .min(1, 'State is required')
    .max(50, 'State must be 50 characters or less'),
  postalCode: z
    .string()
    .min(1, 'Postal code is required')
    .max(20, 'Postal code must be 20 characters or less'),
  country: z.string().optional(),
  isPrimary: z.boolean().optional(),
});

/** Type for individual client form data */
export type CreateIndividualClientFormData = z.infer<typeof createIndividualClientSchema>;

/** Type for business client form data */
export type CreateBusinessClientFormData = z.infer<typeof createBusinessClientSchema>;

/** Type for update client form data */
export type UpdateClientFormData = z.infer<typeof updateClientSchema>;

/** Type for add contact form data */
export type AddContactFormData = z.infer<typeof addContactSchema>;

/** Type for add address form data */
export type AddAddressFormData = z.infer<typeof addAddressSchema>;
