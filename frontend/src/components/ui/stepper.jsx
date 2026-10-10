import { Check } from 'lucide-react'
import { cn } from '@/lib/utils'

/**
 * Presentational, accessible multi-step indicator.
 *
 * @param {Array<{ id: string|number, title: string, description?: string }>} steps
 * @param {number} current  Zero-based index of the active step.
 * @param {(index: number) => void} [onStepClick]  Optional: jump to a completed/known step.
 */
export function Stepper({ steps, current, onStepClick, className }) {
  return (
    <ol className={cn('flex flex-col gap-3 sm:flex-row sm:items-start sm:gap-4', className)}>
      {steps.map((step, index) => {
        const isComplete = index < current
        const isCurrent = index === current
        const canJump = Boolean(onStepClick) && index <= current

        return (
          <li key={step.id ?? index} className="flex flex-1 items-start gap-3 sm:flex-col sm:items-stretch sm:gap-2">
            <button
              type="button"
              disabled={!canJump}
              onClick={canJump ? () => onStepClick(index) : undefined}
              aria-current={isCurrent ? 'step' : undefined}
              className={cn(
                'flex flex-1 items-start gap-3 rounded-lg px-1 py-1 text-left transition-colors sm:flex-col',
                canJump ? 'cursor-pointer' : 'cursor-default',
              )}
            >
              <span
                className={cn(
                  'flex h-8 w-8 shrink-0 items-center justify-center rounded-full border text-sm font-semibold',
                  isComplete && 'border-primary bg-primary text-primary-foreground',
                  isCurrent && 'border-primary text-primary',
                  !isComplete && !isCurrent && 'border-border text-muted-foreground',
                )}
                aria-hidden="true"
              >
                {isComplete ? <Check className="h-4 w-4" /> : index + 1}
              </span>
              <span className="min-w-0">
                <span
                  className={cn(
                    'block text-sm font-medium',
                    isCurrent ? 'text-foreground' : 'text-muted-foreground',
                  )}
                >
                  {step.title}
                </span>
                {step.description ? (
                  <span className="mt-0.5 block text-xs text-muted-foreground">{step.description}</span>
                ) : null}
              </span>
            </button>
            {index < steps.length - 1 ? (
              <span
                className={cn(
                  'hidden h-px flex-1 self-center bg-border sm:block',
                  isComplete && 'bg-primary',
                )}
                aria-hidden="true"
              />
            ) : null}
          </li>
        )
      })}
    </ol>
  )
}

export default Stepper
