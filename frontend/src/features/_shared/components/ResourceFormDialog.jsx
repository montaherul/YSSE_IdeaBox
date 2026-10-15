import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toErrorMessage } from '@/lib/validation'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

/**
 * Generic create/edit dialog form built on react-hook-form + zod.
 *
 * `children` is a render prop receiving the form object:
 *   <ResourceFormDialog
 *     open
 *     title="Edit tag"
 *     schema={tagSchema}
 *     defaultValues={{ name, slug }}
 *     onSubmit={(values) => update.mutateAsync({ id, payload: values })}
 *     onClose={() => setEditing(null)}
 *   >
 *     {(form) => (
 *       <>
 *         <FormField id="name" label="Name" required error={form.formState.errors.name?.message}>
 *           <Input id="name" {...form.register('name')} />
 *         </FormField>
 *       </>
 *     )}
 *   </ResourceFormDialog>
 */
export function ResourceFormDialog({
  open = true,
  onOpenChange,
  onClose,
  title,
  description,
  schema,
  defaultValues,
  onSubmit,
  submitLabel = 'Save',
  pendingLabel = 'Saving…',
  cancelLabel = 'Cancel',
  children,
  contentClassName,
}) {
  const form = useForm({
    resolver: schema ? zodResolver(schema) : undefined,
    defaultValues,
  })

  const { isSubmitting, errors } = form.formState

  useEffect(() => {
    if (open) form.reset(defaultValues)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open])

  const submit = form.handleSubmit(async (values) => {
    try {
      await onSubmit?.(values, form)
      onClose?.()
    } catch (error) {
      form.setError('root', { message: toErrorMessage(error) })
    }
  })

  const close = () => {
    onClose?.()
  }

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        onOpenChange?.(next)
        if (!next) close()
      }}
    >
      <DialogContent className={contentClassName}>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          {description ? <DialogDescription>{description}</DialogDescription> : null}
        </DialogHeader>

        <form onSubmit={submit} noValidate className="space-y-4">
          {errors.root ? (
            <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {errors.root.message}
            </p>
          ) : null}

          {typeof children === 'function' ? children(form) : children}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={close}>
              {cancelLabel}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? pendingLabel : submitLabel}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export default ResourceFormDialog
