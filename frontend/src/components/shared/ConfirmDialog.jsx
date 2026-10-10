import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'

/**
 * Accessible confirmation dialog wrapping the Radix AlertDialog.
 * Prevents destructive mistakes (deletions, withdrawals, suspensions).
 */
export function ConfirmDialog({
  open,
  onOpenChange,
  title = 'Are you sure?',
  description,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  variant = 'destructive',
  destructive,
  onConfirm,
}) {
  const confirmClassName =
    variant === 'destructive' || destructive
      ? 'bg-destructive text-destructive-foreground hover:bg-destructive/90'
      : ''
  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle id="confirm-dialog-title">{title}</AlertDialogTitle>
          {description ? (
            <AlertDialogDescription id="confirm-dialog-description">
              {description}
            </AlertDialogDescription>
          ) : null}
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>{cancelLabel}</AlertDialogCancel>
          <AlertDialogAction
            className={confirmClassName}
            onClick={() => {
              onConfirm?.()
              onOpenChange?.(false)
            }}
          >
            {confirmLabel}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}