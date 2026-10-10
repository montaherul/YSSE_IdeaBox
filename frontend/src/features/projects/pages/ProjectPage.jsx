import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { CheckCircle2, Flag, ListTodo, Send } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { usePermission } from '@/hooks/usePermission'
import { projectUpdateSchema, toErrorMessage } from '@/lib/validation'
import {
  useProject,
  useCreateMilestone,
  useCreateTask,
  useCreateProjectUpdate,
  useIdeaOutcomes,
  useVerifyOutcome,
} from '../hooks'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { Checkbox } from '@/components/ui/checkbox'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { formatDate } from '@/lib/format'

function AddMilestone({ projectId }) {
  const [open, setOpen] = useState(false)
  const create = useCreateMilestone()
  const form = useForm({
    defaultValues: { title: '', description: '' },
  })
  if (!open) {
    return (
      <Button size="sm" variant="outline" onClick={() => setOpen(true)}>
        + Milestone
      </Button>
    )
  }
  return (
    <form
      onSubmit={form.handleSubmit((values) => create.mutateAsync({ projectId, payload: values }))}
      className="space-y-2 border-t pt-3"
    >
      <FormField id={`milestone-title-${projectId}`} label="Milestone title" error={form.formState.errors.title?.message}>
        <Input id={`milestone-title-${projectId}`} {...form.register('title')} />
      </FormField>
      <FormField id={`milestone-desc-${projectId}`} label="Description (optional)">
        <Textarea id={`milestone-desc-${projectId}`} rows={2} {...form.register('description')} />
      </FormField>
      <div className="flex gap-2">
        <Button type="submit" size="sm">Add</Button>
        <Button type="button" size="sm" variant="outline" onClick={() => setOpen(false)}>Cancel</Button>
      </div>
    </form>
  )
}

function AddTask({ projectId }) {
  const [open, setOpen] = useState(false)
  const create = useCreateTask()
  const form = useForm({
    defaultValues: { title: '', description: '' },
  })
  if (!open) {
    return (
      <Button size="sm" variant="outline" onClick={() => setOpen(true)}>
        + Task
      </Button>
    )
  }
  return (
    <form
      onSubmit={form.handleSubmit((values) => {
        create.mutateAsync({ projectId, payload: values })
        setOpen(false)
      })}
      className="space-y-2 border-t pt-3"
    >
      <FormField id={`task-title-${projectId}`} label="Task title" error={form.formState.errors.title?.message}>
        <Input id={`task-title-${projectId}`} {...form.register('title')} />
      </FormField>
      <FormField id={`task-desc-${projectId}`} label="Description (optional)">
        <Textarea id={`task-desc-${projectId}`} rows={2} {...form.register('description')} />
      </FormField>
      <div className="flex gap-2">
        <Button type="submit" size="sm">Add</Button>
        <Button type="button" size="sm" variant="outline" onClick={() => setOpen(false)}>Cancel</Button>
      </div>
    </form>
  )
}

export default function ProjectPage() {
  const { id } = useParams()
  const { can } = usePermission()
  const canManage = can('admin') || can('moderator') || can('helper')
  const { data: project, isLoading, isError, error, refetch } = useProject(id)
  const addUpdate = useCreateProjectUpdate()
  const { data: outcomes } = useIdeaOutcomes(project?.ideaId, Boolean(project?.ideaId && can('helper')))
  const verifyOutcome = useVerifyOutcome()
  useDocumentTitle(project?.name ? `${project.name} — Project` : 'Project')

  const form = useForm({
    resolver: zodResolver(projectUpdateSchema),
    defaultValues: { body: '', evidenceUrl: '', isPublic: true },
  })

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState message={error?.message} onRetry={refetch} />
  if (!project) return <ErrorState title="Project not found" message="This project may have been removed." />

  const milestones = project.milestones ?? []
  const tasks = project.tasks ?? []
  const updates = project.updates ?? []

  const onSubmitUpdate = async (values) => {
    try {
      await addUpdate.mutateAsync({ projectId: project.id, payload: values })
      form.reset()
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={project.name}
        description={project.idea?.title ? `Part of "${project.idea.title}"` : 'Community project'}
        actions={
          canManage ? (
            <StatusBadge status={project.status} />
          ) : null
        }
      />

      {project.idea?.slug ? (
        <p className="text-sm">
          <Link to={`/ideas/${project.idea.slug}`} className="text-primary hover:underline">
            View the original idea
          </Link>
        </p>
      ) : null}

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <ListTodo className="h-4 w-4" aria-hidden="true" />
              Milestones
            </CardTitle>
            {canManage ? <AddMilestone projectId={project.id} /> : null}
          </CardHeader>
          <CardContent className="space-y-2">
            {milestones.length === 0 ? (
              <p className="text-sm text-muted-foreground">No milestones yet.</p>
            ) : (
              milestones.map((m) => (
                <div key={m.id} className="flex items-start gap-2 rounded-lg border p-3">
                  <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-primary" aria-hidden="true" />
                  <div>
                    <p className="text-sm font-medium">{m.title}</p>
                    {m.description ? <p className="text-sm text-muted-foreground">{m.description}</p> : null}
                    {m.dueAt ? <p className="text-xs text-muted-foreground">Due {formatDate(m.dueAt, { short: true })}</p> : null}
                  </div>
                </div>
              ))
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Flag className="h-4 w-4" aria-hidden="true" />
              Tasks
            </CardTitle>
            {canManage ? <AddTask projectId={project.id} /> : null}
          </CardHeader>
          <CardContent className="space-y-2">
            {tasks.length === 0 ? (
              <p className="text-sm text-muted-foreground">No tasks yet.</p>
            ) : (
              tasks.map((t) => (
                <div key={t.id} className="flex items-start gap-2 rounded-lg border p-3">
                  {t.isDone ? <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-success" aria-hidden="true" /> : <Flag className="mt-0.5 h-4 w-4 shrink-0 text-muted-foreground" aria-hidden="true" />}
                  <div>
                    <p className="text-sm font-medium">{t.title}</p>
                    {t.description ? <p className="text-sm text-muted-foreground">{t.description}</p> : null}
                  </div>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Progress updates</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {canManage ? (
            <form onSubmit={form.handleSubmit(onSubmitUpdate)} noValidate className="space-y-3 rounded-lg border p-4">
              {form.formState.errors.root ? (
                <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {form.formState.errors.root.message}
                </p>
              ) : null}
              <FormField id="update-body" label="Share an update" required error={form.formState.errors.body?.message}>
                <Textarea id="update-body" rows={3} {...form.register('body')} />
              </FormField>
              <div className="flex flex-wrap items-center gap-4">
                <div className="flex-1 min-w-52">
                  <FormField id="update-evidence" label="Evidence URL (optional)" error={form.formState.errors.evidenceUrl?.message}>
                    <Input id="update-evidence" {...form.register('evidenceUrl')} />
                  </FormField>
                </div>
                <label className="flex items-center gap-2 text-sm">
                  <Controller
                    control={form.control}
                    name="isPublic"
                    render={({ field }) => (
                      <Checkbox
                        checked={Boolean(field.value)}
                        onCheckedChange={field.onChange}
                        aria-label="Public update"
                      />
                    )}
                  />
                  Public update
                </label>
                <Button type="submit" size="sm" disabled={addUpdate.isPending}>
                  <Send className="h-4 w-4" aria-hidden="true" />
                  Post
                </Button>
              </div>
            </form>
          ) : null}

          {updates.length === 0 ? (
            <p className="text-sm text-muted-foreground">No updates yet.</p>
          ) : (
            updates.map((update) => (
              <div key={update.id} className="rounded-lg border p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <StatusBadge status={update.isPublic ? 'True' : 'False'} label={update.isPublic ? 'Public' : 'Private'} />
                  <span className="text-xs text-muted-foreground">{formatDate(update.createdAt)}</span>
                </div>
                <p className="mt-2 whitespace-pre-line text-sm">{update.body}</p>
                {update.evidenceUrl ? (
                  <a href={update.evidenceUrl} target="_blank" rel="noreferrer" className="text-sm text-primary hover:underline">
                    View evidence
                  </a>
                ) : null}
              </div>
            ))
          )}
        </CardContent>
      </Card>

      {canManage && ((outcomes?.items ?? outcomes ?? []).length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Outcomes</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {(outcomes.items ?? []).map((outcome) => (
              <div key={outcome.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg border p-3 text-sm">
                <div>
                  <p className="font-medium">{outcome.kind}</p>
                  {outcome.description ? <p className="text-muted-foreground">{outcome.description}</p> : null}
                </div>
                <div className="flex items-center gap-2">
                  <StatusBadge status={outcome.status} />
                  <Button size="sm" variant="outline" onClick={() => verifyOutcome.mutate({ id: outcome.id, payload: {} })}>
                    Mark verified
                  </Button>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      ) : null)}

      <FacilitationNotice />
    </div>
  )
}