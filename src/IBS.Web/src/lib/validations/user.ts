import { z } from 'zod';

/** Phone number regex pattern */
const phoneRegex = /^\+?[\d\s\-().]+$/;

/**
 * Schema for creating a new user.
 */
export const createUserSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z
    .string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[A-Z]/, 'Password must contain an uppercase letter')
    .regex(/[a-z]/, 'Password must contain a lowercase letter')
    .regex(/[0-9]/, 'Password must contain a number')
    .regex(/[^A-Za-z0-9]/, 'Password must contain a special character'),
  firstName: z
    .string()
    .min(1, 'First name is required')
    .max(100, 'First name must be 100 characters or less'),
  lastName: z
    .string()
    .min(1, 'Last name is required')
    .max(100, 'Last name must be 100 characters or less'),
  title: z
    .string()
    .max(100, 'Title must be 100 characters or less')
    .optional()
    .or(z.literal('')),
  phoneNumber: z
    .string()
    .regex(phoneRegex, 'Invalid phone number')
    .optional()
    .or(z.literal('')),
});

/**
 * Schema for updating a user's profile.
 */
export const updateUserProfileSchema = z.object({
  firstName: z
    .string()
    .min(1, 'First name is required')
    .max(100, 'First name must be 100 characters or less'),
  lastName: z
    .string()
    .min(1, 'Last name is required')
    .max(100, 'Last name must be 100 characters or less'),
  title: z
    .string()
    .max(100, 'Title must be 100 characters or less')
    .optional()
    .or(z.literal('')),
  phoneNumber: z
    .string()
    .regex(phoneRegex, 'Invalid phone number')
    .optional()
    .or(z.literal('')),
});

/** Type for create user form data */
export type CreateUserFormData = z.infer<typeof createUserSchema>;

/** Type for update user profile form data */
export type UpdateUserProfileFormData = z.infer<typeof updateUserProfileSchema>;
