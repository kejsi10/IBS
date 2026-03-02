import { create } from "zustand";
import { persist } from "zustand/middleware";

/**
 * User data stored in the auth state
 */
export interface User {
  id: string;
  tenantId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

/**
 * Auth store state interface
 */
interface AuthState {
  /** Current authenticated user */
  user: User | null;
  /** Whether the user is authenticated */
  isAuthenticated: boolean;
  /** Set the current user */
  setUser: (user: User | null) => void;
  /** Log out the user */
  logout: () => void;
}

/**
 * Auth store for managing authentication state
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,

      setUser: (user) =>
        set({
          user,
          isAuthenticated: user !== null,
        }),

      logout: () => {
        // Tokens are httpOnly cookies cleared by the server on /auth/logout.
        // Only the in-memory profile state needs to be cleared here.
        set({
          user: null,
          isAuthenticated: false,
        });
      },
    }),
    {
      name: "auth-storage",
      partialize: (state) => ({
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
