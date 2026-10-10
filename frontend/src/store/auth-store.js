import { create } from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'

/**
 * Client-side auth state only (hydration tokens + the authenticated user).
 * Server state (profiles, ideas, …) lives exclusively in TanStack Query.
 * Tokens are persisted to localStorage; passwords never are.
 */

const initialState = {
  user: null,
  accessToken: null,
  refreshToken: null,
  isAuthenticated: false,
}

export const useAuthStore = create(
  persist(
    (set, get) => ({
      ...initialState,

      setAuth: (accessToken, refreshToken, user) =>
        set({
          accessToken,
          refreshToken,
          user: user ?? get().user,
          isAuthenticated: Boolean(accessToken),
        }),

      setUser: (user) => set({ user }),

      logout: () => set({ ...initialState }),
    }),
    {
      name: 'ysse-idea-box.auth',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
      onRehydrateStorage: () => (state) => {
        if (state) {
          state.isAuthenticated = Boolean(state.accessToken)
        }
      },
      version: 1,
    },
  ),
)