import { QueryClientProvider } from '@tanstack/react-query'
import { queryClient } from './query-client'
import { TooltipProvider } from '@/components/ui/tooltip'
import { Toaster } from '@/components/ui/toaster'

/** Application providers: React Query + Radix tooltip + Sonner toaster. */
export function Providers({ children }) {
  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider delayDuration={200}>{children}</TooltipProvider>
      <Toaster />
    </QueryClientProvider>
  )
}