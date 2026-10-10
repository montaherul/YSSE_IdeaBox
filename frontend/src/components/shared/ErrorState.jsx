import { WifiOff, AlertTriangle } from 'lucide-react'
import { Button } from '@/components/ui/button'

/** Error state with an optional retry. Never shows raw internals for safety. */
export function ErrorState({
  title = 'Something went wrong',
  message = 'We could not load this page. This may be a temporary problem.',
  onRetry,
  compact = false,
  suppressIcon = 'wifi',
}) {
  const Icon = suppressIcon === 'wifi' ? WifiOff : AlertTriangle
  return (
    <div
      role="alert"
      className={`flex flex-col items-center justify-center rounded-xl border bg-card p-8 text-center ${
        compact ? 'p-6' : ''
      }`}
    >
      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-muted">
        <Icon className="h-6 w-6 text-muted-foreground" aria-hidden="true" />
      </div>
      <h3 className="mt-4 text-base font-semibold">{title}</h3>
      <p className="mt-1 max-w-md text-sm text-muted-foreground">{message}</p>
      {onRetry ? (
        <Button variant="outline" className="mt-4" onClick={onRetry}>
          Try again
        </Button>
      ) : null}
    </div>
  )
}