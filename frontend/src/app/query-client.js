import { QueryClient } from '@tanstack/react-query'

/**
 * Central TanStack Query client. Server state lives ONLY here (rule.md §4).
 * Reasonable defaults for the Idea Box: short stale times for public lists,
 * no refetch on window focus (low bandwidth), single retry.
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      gcTime: 5 * 60_000,
      retry: 1,
      refetchOnWindowFocus: false,
      refetchOnReconnect: false,
    },
    mutations: {
      retry: 0,
    },
  },
})