import axios, { type AxiosError, type InternalAxiosRequestConfig } from "axios";

/**
 * Base API URL - uses relative path to work with Vite proxy in development
 * In production, set VITE_API_URL to the full API URL
 */
const API_URL = import.meta.env.VITE_API_URL || "";

/**
 * Default tenant ID for development (Demo Insurance Agency)
 * In production, this would be resolved from subdomain or user selection
 */
const DEFAULT_TENANT_ID = "00000000-0000-0000-0000-000000000001";

/**
 * Axios instance configured for the IBS API.
 * Tokens are stored in httpOnly cookies — withCredentials ensures they are
 * sent automatically on every request without any JavaScript access to them.
 */
export const api = axios.create({
  baseURL: `${API_URL}/api/v1`,
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
    "X-Tenant-Id": DEFAULT_TENANT_ID,
  },
});

/**
 * Response interceptor to handle transparent token refresh on 401.
 */
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean;
    };

    // Handle 401 Unauthorized — attempt silent token refresh via cookie
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // POST to /refresh — the ibs_refresh_token cookie is sent automatically
        await axios.post(`${API_URL}/api/v1/auth/refresh`, null, {
          withCredentials: true,
        });

        // Retry the original request — new ibs_access_token cookie is now set
        return api(originalRequest);
      } catch {
        // Refresh failed — redirect to login
        window.location.href = "/login";
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);

/**
 * API error response type — backend returns { error: string, errors?: ... }
 */
export interface ApiError {
  error: string;
  errors?: Record<string, string[]>;
}

/**
 * Extracts the human-readable error message from an API error response.
 */
export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ApiError | undefined;
    return data?.error || error.message;
  }
  if (error instanceof Error) {
    return error.message;
  }
  return "An unexpected error occurred";
}
