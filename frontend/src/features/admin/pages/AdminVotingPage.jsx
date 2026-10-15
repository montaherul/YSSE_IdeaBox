import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Megaphone, Play, Plus, Pencil, Trash2, Trophy, X } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { usePermission } from '@/hooks/usePermission'
import { votingPeriodSchema, toErrorMessage } from '@/lib/validation'
import { useVotingPeriods, useCreateVotingPeriod, useUpdateVotingPeriod, useDeleteVotingPeriod, useActivateVotingPeriod, useCloseVotingPeriod, useAnnounceMonthlyTop } from '@/features/voting/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { CardSkeleton } from '@/components/shared/LoadingSkeleton'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { FormField } from '@/components/ui/form-field'
import { Card, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { formatDate } from '@/lib/format'

function toLocalInput(date) {
  if (!date) return ''
  const d = new Date(date)
  const pad = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function PeriodDialog({ period, onClose }) {
  const isEdit = Boolean(period)
  const create = useCreateVotingPeriod()
  const update = useUpdateVotingPeriod()
  const submit = isEdit ? update : create
  const form = useForm({
    resolver: zodResolver(votingPeriodSchema),
    defaultValues: period
      ? {
          name: period.name ?? '',
          startAt: toLocalInput(period.startAt),
          endAt: toLocalInput(period.endAt),
          visibility: period.visibility ?? 'HiddenUntilClose',
          minimumAccountAgeDays: period.minimumAccountAgeDays ?? 7,
          isActive: period.isActive ?? true,
        }
      : { name: '', startAt: '', endAt: '', visibility: 'HiddenUntilClose', minimumAccountAgeDays: 7, isActive: true },
  })

  const onSubmit = async (values) => {
    try {
      if (isEdit) {
        await update.mutateAsync({ id: period.id, payload: values })
      } else {
        await create.mutateAsync(values)
      }
      onClose()
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle id="voting-period-title">{isEdit ? 'Edit voting period' : 'New voting period'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
          {form.formState.errors.root ? (
            <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{form.formState.errors.root.message}</p>
          ) : null}
          <FormField id="period-name" label="Name" required error={form.formState.errors.name?.message}>
            <Input id="period-name" {...form.register('name')} />
          </FormField>
          <div className="grid gap-4 sm:grid-cols-2">
            <FormField id="period-start" label="Start" required error={form.formState.errors.startAt?.message}>
              <Input id="period-start" type="datetime-local" {...form.register('startAt')} />
            </FormField>
            <FormField id="period-end" label="End" required error={form.formState.errors.endAt?.message}>
              <Input id="period-end" type="datetime-local" {...form.register('endAt')} />
            </FormField>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
            <Button type="submit" disabled={submit.isPending}>{isEdit ? 'Save' : 'Create'}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export default function AdminVotingPage() {
  useDocumentTitle('Voting periods')
  const [dialog, setDialog] = useState(null)
  const [confirmingDelete, setConfirmingDelete] = useState(null)
  const { data, isLoading, isError, refetch } = useVotingPeriods()
  const activate = useActivateVotingPeriod()
  const close = useCloseVotingPeriod()
  const remove = useDeleteVotingPeriod()
  const announce = useAnnounceMonthlyTop()
  const { canPermission } = usePermission()

  const items = data?.items ?? data ?? []

  return (
    <div className="space-y-6">
      <PageHeader
        title="Voting periods"
        description="Manage idea voting cycles and leadersboards."
        actions={
          <div className="flex items-center gap-2">
            {canPermission('votingperiods.manage') ? (
              <Button
                size="sm"
                variant="outline"
                disabled={announce.isPending}
                onClick={() => announce.mutate({})}
              >
                <Megaphone className="h-4 w-4" aria-hidden="true" />
                Announce appreciation
              </Button>
            ) : null}
            <Button size="sm" onClick={() => setDialog({ mode: 'create' })}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              New period
            </Button>
          </div>
        }
      />

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2">
          <CardSkeleton />
          <CardSkeleton />
        </div>
      ) : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState icon={Trophy} title="No voting periods" description="Create a voting period to run a leaderboard cycle." />
      ) : null}

      {items.length > 0 ? (
        <div className="space-y-3">
          {items.map((period) => (
            <Card key={period.id}>
              <CardHeader className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <CardTitle className="text-base">{period.name}</CardTitle>
                  <p className="mt-1 text-xs text-muted-foreground">
                    {formatDate(period.startAt)} → {formatDate(period.endAt)}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <StatusBadge status={period.isActive ? 'Open' : 'Closed'} label={period.isActive ? 'Active' : 'Closed'} />
                  <Button size="sm" variant="outline" onClick={() => setDialog({ mode: 'edit', period })}>
                    <Pencil className="h-4 w-4" aria-hidden="true" />
                    Edit
                  </Button>
                  <Button size="sm" variant="outline" onClick={() => setConfirmingDelete(period)}>
                    <Trash2 className="h-4 w-4" aria-hidden="true" />
                    Delete
                  </Button>
                  {!period.isActive ? (
                    <Button size="sm" variant="outline" onClick={() => activate.mutate(period.id)}>
                      <Play className="h-4 w-4" aria-hidden="true" />
                      Activate
                    </Button>
                  ) : (
                    <Button size="sm" variant="outline" onClick={() => close.mutate(period.id)}>
                      <X className="h-4 w-4" aria-hidden="true" />
                      Close
                    </Button>
                  )}
                </div>
              </CardHeader>
            </Card>
          ))}
        </div>
      ) : null}

      {dialog?.mode === 'create' ? (
        <PeriodDialog onClose={() => setDialog(null)} />
      ) : dialog?.mode === 'edit' ? (
        <PeriodDialog period={dialog.period} onClose={() => setDialog(null)} />
      ) : null}

      <ConfirmDialog
        open={Boolean(confirmingDelete)}
        onOpenChange={(open) => !open && setConfirmingDelete(null)}
        title="Delete this voting period?"
        description={confirmingDelete ? `"${confirmingDelete.name}" will be permanently removed if it has no recorded votes.` : ''}
        confirmLabel="Delete period"
        onConfirm={() => {
          if (confirmingDelete) remove.mutate(confirmingDelete.id)
        }}
      />
    </div>
  )
}