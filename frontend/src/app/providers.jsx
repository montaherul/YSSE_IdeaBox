import { useEffect } from 'react'
import { QueryClientProvider } from '@tanstack/react-query'
import { queryClient } from './query-client'
import { TooltipProvider } from '@/components/ui/tooltip'
import { Toaster } from '@/components/ui/toaster'
import { useLocaleStore } from '@/store/locale-store'
import { useUiStore } from '@/store/ui-store'
import { useAuthStore } from '@/store/auth-store'
import { useAuthMe } from '@/features/auth/hooks'
import { NotificationHub } from '@/hooks/useNotificationHub'

/**
 * Refreshes the authenticated user (roles + permissions) from the server on load.
 * The persisted session may predate newer fields, so this keeps permission-gated
 * UI accurate without requiring a re-login. Runs only when a session exists; the
 * API stays the authoritative authorization boundary.
 */
function AuthSessionSync() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  useAuthMe({ enabled: isAuthenticated })
  return null
}

/** Application providers: React Query + Radix tooltip + Sonner toaster. */
export function Providers({ children }) {
  useLocaleStore.getState().initLocale()
  const { hydrateTheme } = useUiStore.getState()

  useEffect(() => {
    hydrateTheme()
  }, [])

  return (
    <QueryClientProvider client={queryClient}>
      <AuthSessionSync />
      <TooltipProvider delayDuration={200}>{children}</TooltipProvider>
      <NotificationHub />
      <Toaster />
    </QueryClientProvider>
  )
}