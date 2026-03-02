import { api } from '@/lib/api';
import type { LoginRequest, LoginResponse, User } from '@/types/api';

/**
 * Authentication service for login, logout, and user management.
 */
export const authService = {
  /**
   * Authenticates a user with email and password.
   */
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/login', data);
    return response.data;
  },

  /**
   * Logs out the current user.
   */
  logout: async (): Promise<void> => {
    await api.post('/auth/logout');
  },

  /**
   * Refreshes the access token using a refresh token.
   */
  refreshToken: async (refreshToken: string): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/refresh', { refreshToken });
    return response.data;
  },

  /**
   * Gets the current authenticated user's profile.
   */
  getCurrentUser: async (): Promise<User> => {
    const response = await api.get<User>('/auth/me');
    return response.data;
  },

  /**
   * Initiates a password reset flow.
   */
  forgotPassword: async (email: string, tenantId?: string): Promise<void> => {
    await api.post('/auth/forgot-password', { email, tenantId });
  },

  /**
   * Completes the password reset with a new password.
   */
  resetPassword: async (data: {
    token: string;
    email: string;
    newPassword: string;
    confirmPassword: string;
    tenantId?: string;
  }): Promise<void> => {
    await api.post('/auth/reset-password', data);
  },
};
