import * as React from 'react'
import { cn } from '@/lib/utils'
import { Label } from './label'

/**
 * Small labelled field wrapper used with react-hook-form:
 * renders an accessible label, the control, a hint and an inline error
 * (WCAG rule.md §3 — "errors beside the relevant fields").
 */
function FormField({ id, label, required = false, hint, error, className, children }) {
  const errorId = id ? `${id}-error` : undefined
  const hintId = id ? `${id}-hint` : undefined

  return (
    <div className={cn('space-y-1.5', className)}>
      {label ? (
        <Label htmlFor={id}>
          {label}
          {required ? (
            <span className="ml-0.5 text-destructive" aria-hidden="true">
              *
            </span>
          ) : null}
        </Label>
      ) : null}

      {React.cloneElement(children, {
        id,
        ...(hintId ? { 'aria-describedby': error ? `${errorId} ${hintId}` : hintId } : {}),
        ...(error ? { 'aria-invalid': true } : {}),
      })}

      {error ? (
        <p id={errorId} role="alert" className="text-sm text-destructive">
          {error}
        </p>
      ) : null}

      {hint && !error ? (
        <p id={hintId} className="text-xs text-muted-foreground">
          {hint}
        </p>
      ) : null}
    </div>
  )
}

export { FormField }