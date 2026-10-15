import { Star } from 'lucide-react'
import { reviewSchema } from '@/lib/validation'
import { ResourceFormDialog } from '@/features/_shared'
import { FormField } from '@/components/ui/form-field'
import { Textarea } from '@/components/ui/textarea'

function RatingInput({ value, onChange, disabled }) {
  const current = value ?? 0
  return (
    <div className="flex items-center gap-1" role="group" aria-label="Rating (1 to 5 stars)">
      {[1, 2, 3, 4, 5].map((n) => (
        <button
          key={n}
          type="button"
          disabled={disabled}
          aria-label={`${n} star${n > 1 ? 's' : ''}`}
          aria-pressed={value === n}
          onClick={() => onChange(value === n ? null : n)}
          className={`rounded p-0.5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:opacity-50 ${
            n <= current ? 'text-warning' : 'text-muted-foreground'
          }`}
        >
          <Star
            className="h-5 w-5"
            fill={n <= current ? 'currentColor' : 'none'}
            aria-hidden="true"
          />
        </button>
      ))}
    </div>
  )
}

/**
 * Create/edit review form built on the shared ResourceFormDialog + reviewSchema.
 * Call `onSubmit(values, form)`; throw from it to surface a root error.
 */
export function ReviewForm({
  open = true,
  onClose,
  title = 'Your review',
  description,
  defaultValues,
  onSubmit,
  submitLabel = 'Post review',
  pendingLabel = 'Posting…',
}) {
  return (
    <ResourceFormDialog
      open={open}
      onClose={onClose}
      title={title}
      description={description}
      schema={reviewSchema}
      defaultValues={{ body: '', rating: null, ...defaultValues }}
      onSubmit={onSubmit}
      submitLabel={submitLabel}
      pendingLabel={pendingLabel}
    >
      {(form) => (
        <>
          <FormField
            id="review-body"
            label="Your review"
            required
            hint="At least 10 characters"
            error={form.formState.errors.body?.message}
          >
            <Textarea id="review-body" rows={4} {...form.register('body')} />
          </FormField>
          <RatingInput
            value={form.watch('rating')}
            onChange={(next) =>
              form.setValue('rating', next, { shouldValidate: true, shouldDirty: true })
            }
            disabled={form.formState.isSubmitting}
          />
        </>
      )}
    </ResourceFormDialog>
  )
}

export default ReviewForm
