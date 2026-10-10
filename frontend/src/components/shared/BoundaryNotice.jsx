import { Lightbulb, Info } from 'lucide-react'
import { BOUNDARY_NOTICE } from '@/config/constants'

/** YSSE's non-negotiable business boundary (SRS §1.1) shown wherever inquiries/outcomes/matching appear. */
export function BoundaryNotice({ className }) {
  return (
    <div
      role="note"
      aria-label="Funding boundary notice"
      className={`flex items-start gap-3 rounded-lg border border-muted bg-muted/40 p-4 text-sm text-muted-foreground ${className ?? ''}`}
    >
      <Info className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
      <div>
        <p className="flex items-center gap-1.5 font-medium text-foreground">
          <Lightbulb className="h-4 w-4 shrink-0" aria-hidden="true" />
          YSSE facilitates connections; it does not manage funding.
        </p>
        <p className="mt-1">{BOUNDARY_NOTICE}</p>
      </div>
    </div>
  )
}