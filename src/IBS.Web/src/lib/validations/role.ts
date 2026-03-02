import { z } from 'zod';

/**
 * Schema for creating a new role.
 */
export const createRoleSchema = z.object({
  name: z
    .string()
    .min(1, 'Role name is required')
    .max(100, 'Role name must be 100 characters or less'),
  description: z
    .string()
    .max(500, 'Description must be 500 characters or less')
    .optional()
    .or(z.literal('')),
});

/**
 * Schema for updating a role.
 */
export const updateRoleSchema = z.object({
  name: z
    .string()
    .min(1, 'Role name is required')
    .max(100, 'Role name must be 100 characters or less'),
  description: z
    .string()
    .max(500, 'Description must be 500 characters or less')
    .optional()
    .or(z.literal('')),
});

/** Type for create role form data */
export type CreateRoleFormData = z.infer<typeof createRoleSchema>;

/** Type for update role form data */
export type UpdateRoleFormData = z.infer<typeof updateRoleSchema>;
