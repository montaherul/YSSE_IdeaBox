import { Toaster as SonnerToaster } from 'sonner'
import { useUiStore } from '@/store/ui-store'

/** Sonner toaster bound to the app theme (from the UI store). */
export function Toaster() {
  const theme = useUiStore((s) => s.theme)
  return (
    <SonnerToaster
      theme={theme === 'dark' ? 'dark' : 'light'}
      position={theme === 'dark' ? 'top-right' : 'top-right'}
      richColors
      closeButton
    />
  )
}