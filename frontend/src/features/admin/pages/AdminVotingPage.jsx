import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Play, Plus, Trophy, X } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { votingPeriodSchema, toErrorMessage } from '@/lib/validation'
import { useVotingPeriods, useCreateVotingPeriod, useActivateVotingPeriod, useCloseVotingPeriod } from '@/features/voting/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { CardSkeleton } from '@/components/shared/LoadingSkeleton'
import { StatusBadge } from '@/components/shared/StatusBadge'
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

function CreatePeriodDialog({ onClose }) {
  const create = useCreateVotingPeriod()
  const form = useForm({
    resolver: zodResolver(votingPeriodSchema),
    defaultValues: { name: '', startAt: '', endAt: '', visibility: 'HiddenUntilClose', minimumAccountAgeDays: 7, isActive: true },
  })

  const onSubmit = async (values) => {
    try {
      await create.mutateAsync(values)
      onClose()
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle id="voting-period-title">New voting period</DialogTitle>
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
            <Button type="submit" disabled={create.isPending}>Create</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export default function AdminVotingPage() {
  useDocumentTitle('Voting periods')
  const [creating, setCreating] = useState(false)
  const { data, isLoading, isError, refetch } = useVotingPeriods()
  const activate = useActivateVotingPeriod()
  const close = useCloseVotingPeriod()

  const items = data?.items ?? data ?? []

  return (
    <div className="space-y-6">
      <PageHeader
        title="Voting periods"
        description="Manage idea voting cycles and leadersboards."
        actions={
          <Button size="sm" onClick={() => setCreating(true)}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            New period
          </Button>
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

      {creating ? <CreatePeriodDialog onClose={() => setCreating(false)} /> : null}
    </div>
  )
}