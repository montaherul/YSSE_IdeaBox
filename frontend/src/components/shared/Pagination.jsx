import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { formatNumber } from '@/lib/format'

/**
 * Server-side pagination controls. Status is conveyed by text + arrows,
 * not colour alone (WCAG).
 */
export function Pagination({ page = 1, pageSize = 20, total = 0, onChange, className }) {
  const totalPages = Math.max(1, Math.ceil((total || 0) / Math.max(1, pageSize || 1)))
  const hasPrev = page > 1
  const hasNext = page < totalPages
  const from = total === 0 ? 0 : (page - 1) * pageSize + 1
  const to = Math.min(page * pageSize, total)

  return (
    <nav
      aria-label="Pagination"
      className={`flex flex-col items-center justify-between gap-3 border-t pt-4 sm:flex-row ${className ?? ''}`}
    >
      <p className="text-sm text-muted-foreground">
        Showing <span className="font-medium text-foreground">{formatNumber(from)}</span>–
        <span className="font-medium text-foreground">{formatNumber(to)}</span> of{' '}
        <span className="font-medium text-foreground">{formatNumber(total)}</span>
      </p>
      <div className="flex items-center gap-2">
        <Button
          variant="outline"
          size="sm"
          onClick={() => onChange?.(page - 1)}
          disabled={!hasPrev}
          aria-label="Previous page"
        >
          <ChevronLeft className="h-4 w-4" aria-hidden="true" />
          Previous
        </Button>
        <span className="text-sm text-muted-foreground" aria-live="polite">
          Page <span className="font-medium text-foreground">{formatNumber(page)}</span> of{' '}
          {formatNumber(totalPages)}
        </span>
        <Button
          variant="outline"
          size="sm"
          onClick={() => onChange?.(page + 1)}
          disabled={!hasNext}
          aria-label="Next page"
        >
          Next
          <ChevronRight className="h-4 w-4" aria-hidden="true" />
        </Button>
      </div>
    </nav>
  )
}