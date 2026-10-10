import { X } from 'lucide-react'

/** Simple removable tag chip (ideas, categories, marketplace). */
export function Tag({ children, onRemove, className }) {
  return (
    <span
      className={`inline-flex items-center gap-1 rounded-full bg-secondary px-2.5 py-0.5 text-xs font-medium text-secondary-foreground ${className ?? ''}`}
    >
      {children}
      {onRemove ? (
        <button
          type="button"
          onClick={onRemove}
          className="rounded-full p-0.5 hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          aria-label={`Remove ${children}`}
        >
          <X className="h-3 w-3" aria-hidden="true" />
        </button>
      ) : null}
    </span>
  )
}