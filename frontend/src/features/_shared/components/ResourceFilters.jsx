import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

/**
 * Reusable filter bar. Pass filter controls as children and an optional
 * `onReset` handler to show a reset button.
 */
export function ResourceFilters({ children, onReset, className }) {
  return (
    <div className={cn('flex flex-wrap items-end gap-3', className)} role="group" aria-label="Filters">
      {children}
      {onReset ? (
        <Button type="button" variant="ghost" size="sm" onClick={onReset}>
          Reset
        </Button>
      ) : null}
    </div>
  )
}

export default ResourceFilters
