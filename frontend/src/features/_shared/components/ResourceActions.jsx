import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

/**
 * Reusable row/card action buttons.
 *
 * `actions` is a declarative array:
 *   { key, label, icon?, variant?, size?, onClick, visible?, disabled? }
 * Hidden when `visible` is explicitly `false`.
 */
export function ResourceActions({ actions = [], size = 'sm', className }) {
  const visible = actions.filter((action) => action.visible !== false)
  if (visible.length === 0) return null

  return (
    <div className={cn('flex flex-wrap items-center gap-2', className)}>
      {visible.map((action) => {
        const Icon = action.icon
        return (
          <Button
            key={action.key ?? action.label}
            type="button"
            size={action.size ?? size}
            variant={action.variant ?? 'outline'}
            onClick={action.onClick}
            disabled={action.disabled}
          >
            {Icon ? <Icon className="h-4 w-4" aria-hidden="true" /> : null}
            {action.label}
          </Button>
        )
      })}
    </div>
  )
}

export default ResourceActions
