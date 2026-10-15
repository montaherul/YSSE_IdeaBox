import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { CheckCircle2, Flag, ListTodo, Send, ArrowRightCircle, Pencil, Trash2 } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { usePermission } from '@/hooks/usePermission'
import { useAuth } from '@/hooks/useAuth'
import { projectUpdateSchema, outcomeSchema, toErrorMessage } from '@/lib/validation'
import {
  useProject,
  useCreateMilestone,
  useCreateTask,
  useCreateProjectUpdate,
  useVerifyOutcome,
  useCreateOutcome,
  useUpdateOutcome,
  useDeleteOutcome,
  useProjectStatus,
  useDeleteProject,
} from '@/features/projects/hooks'
import { outcomesApi } from '@/features/projects/api'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { ResourceFormDialog } from '@/features/_shared'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { Checkbox } from '@/components/ui/checkbox'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { formatDate } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

const PROJECT_STATUSES = ['Planned', 'InProgress', 'Paused', 'Completed', 'Cancelled']
const OUTCOME_KINDS = ['Collaboration', 'Venture', 'Mentorship', 'Investment', 'Other']
const OUTCOME_STATUSES = [
  'InquiryReceived',
  'DiscussionOngoing',
  'ProposalExchanged',
  'AgreementReported',
  'NotProceeding',
  'OutcomeUnknown',
]

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

function ProjectStatusSelector({ project, canManage }) {
  const change = useProjectStatus()
  if (!canManage) return null
  return (
    <div className="flex items-center gap-2" role="group" aria-label="Change project status">
      <span className="text-sm text-muted-foreground">Status</span>
      <Select
        value={project.status}
        onValueChange={(status) => change.mutate({ id: project.id, payload: { status } })}
        disabled={change.isPending}
      >
        <SelectTrigger className="h-8 w-36" aria-label="Change project status">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {PROJECT_STATUSES.map((s) => (
            <SelectItem key={s} value={s} disabled={s === project.status}>
              {s}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  )
}

function ReportOutcome({ ideaId }) {
  const [open, setOpen] = useState(false)
  const create = useCreateOutcome()
  const form = useForm({
    resolver: zodResolver(outcomeSchema),
    defaultValues: { ideaId, kind: 'Collaboration', status: 'AgreementReported', description: '' },
  })
  if (!open) {
    return (
      <Button size="sm" variant="outline" onClick={() => setOpen(true)}>
        <ArrowRightCircle className="h-4 w-4" aria-hidden="true" />
        Report outcome
      </Button>
    )
  }
  return (
    <form
      onSubmit={form.handleSubmit(async (values) => {
        try {
          await create.mutateAsync(values)
          setOpen(false)
          form.reset()
        } catch (err) {
          form.setError('root', { message: toErrorMessage(err) })
        }
      })}
      noValidate
      className="space-y-3 rounded-lg border p-4"
    >
      {form.formState.errors.root ? (
        <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
          {form.formState.errors.root.message}
        </p>
      ) : null}
      <div className="grid gap-3 sm:grid-cols-2">
        <FormField id="outcome-kind" label="Kind" required error={form.formState.errors.kind?.message}>
          <Controller
            control={form.control}
            name="kind"
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger id="outcome-kind" aria-label="Outcome kind">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {OUTCOME_KINDS.map((kind) => (
                    <SelectItem key={kind} value={kind}>{kind}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          />
        </FormField>
        <FormField id="outcome-status" label="Stage" required error={form.formState.errors.status?.message}>
          <Controller
            control={form.control}
            name="status"
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger id="outcome-status" aria-label="Outcome stage">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {OUTCOME_STATUSES.map((status) => (
                    <SelectItem key={status} value={status}>{status}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          />
        </FormField>
      </div>
      <FormField id="outcome-description" label="Details (optional)" error={form.formState.errors.description?.message}>
        <Textarea id="outcome-description" rows={3} {...form.register('description')} />
      </FormField>
      <div className="flex gap-2">
        <Button type="submit" size="sm" disabled={create.isPending}>{create.isPending ? 'Saving…' : 'Report'}</Button>
        <Button type="button" size="sm" variant="outline" onClick={() => setOpen(false)}>Cancel</Button>
      </div>
    </form>
  )
}

function OutcomesTable({ ideaId, canVerify }) {
  const [editing, setEditing] = useState(null)
  const [pendingDelete, setPendingDelete] = useState(null)
  const update = useUpdateOutcome()
  const remove = useDeleteOutcome()
  const verify = useVerifyOutcome()

  const handleAction = (key, row) => {
    if (key === 'edit') setEditing(row)
    if (key === 'verify') verify.mutate({ id: row.id, payload: {} })
    if (key === 'delete') setPendingDelete(row)
  }

  return (
    <>
      <ServerDataTable
        queryKey={['outcomes', 'idea', ideaId]}
        queryFn={(params) => outcomesApi.ideaOutcomes(ideaId, params)}
        pageSize={DEFAULT_PAGE_SIZE}
        searchable={false}
        emptyTitle="No outcomes reported yet"
        emptyDescription="Report outcomes as collaborations progress."
        columns={[
          { field: 'kind', title: 'Kind', headerSort: false, minWidth: 130 },
          { field: 'status', title: 'Stage', headerSort: false, minWidth: 150 },
          { field: 'description', title: 'Details', headerSort: false, widthGrow: 2,
            formatter: (cell) => cell.getValue() || '—' },
          { field: 'isVerified', title: 'Verified', headerSort: false, width: 100,
            formatter: (cell) => (cell.getValue() ? 'Yes' : 'No') },
          { field: 'createdAt', title: 'Reported', headerSort: false, minWidth: 130,
            formatter: (cell) => formatDate(cell.getValue()) },
        ]}
        actions={[
          { key: 'verify', label: 'Verify', visible: () => canVerify },
          { key: 'edit', label: 'Edit' },
          { key: 'delete', label: 'Delete', variant: 'destructive' },
        ]}
        onAction={handleAction}
      />

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onOpenChange={(open) => !open && setPendingDelete(null)}
        title="Delete this outcome?"
        description="The reported outcome will be permanently removed."
        confirmLabel="Delete"
        onConfirm={() => {
          if (pendingDelete) remove.mutate(pendingDelete.id)
          setPendingDelete(null)
        }}
      />

      <ResourceFormDialog
        open={Boolean(editing)}
        title="Edit outcome"
        schema={outcomeSchema}
        defaultValues={{
          ideaId,
          kind: editing?.kind ?? OUTCOME_KINDS[0],
          status: editing?.status ?? OUTCOME_STATUSES[0],
          description: editing?.description ?? '',
        }}
        submitLabel="Save"
        onClose={() => setEditing(null)}
        onSubmit={(values) => update.mutateAsync({ id: editing.id, payload: values })}
      >
        {(form) => (
          <>
            <div className="grid gap-3 sm:grid-cols-2">
              <FormField id="outcome-edit-kind" label="Kind" required error={form.formState.errors.kind?.message}>
                <Select value={form.watch('kind')} onValueChange={(v) => form.setValue('kind', v, { shouldValidate: true })}>
                  <SelectTrigger id="outcome-edit-kind" aria-label="Outcome kind">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {OUTCOME_KINDS.map((kind) => (
                      <SelectItem key={kind} value={kind}>{kind}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FormField>
              <FormField id="outcome-edit-status" label="Stage" required error={form.formState.errors.status?.message}>
                <Select value={form.watch('status')} onValueChange={(v) => form.setValue('status', v, { shouldValidate: true })}>
                  <SelectTrigger id="outcome-edit-status" aria-label="Outcome stage">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {OUTCOME_STATUSES.map((status) => (
                      <SelectItem key={status} value={status}>{status}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FormField>
            </div>
            <FormField id="outcome-edit-description" label="Details (optional)" error={form.formState.errors.description?.message}>
              <Textarea id="outcome-edit-description" rows={3} {...form.register('description')} />
            </FormField>
          </>
        )}
      </ResourceFormDialog>
    </>
  )
}

export default function ProjectPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { can, canPermission } = usePermission()
  const { user } = useAuth()
  const canManage = can('admin') || can('moderator')
  const canCreateProject = canPermission('projects.create')
  const { data: project, isLoading, isError, error, refetch } = useProject(id)
  const addUpdate = useCreateProjectUpdate()
  const remove = useDeleteProject()
  const [confirmingDelete, setConfirmingDelete] = useState(false)
  useDocumentTitle(project?.name ? `${project.name} — Project` : 'Project')

  const form = useForm({
    resolver: zodResolver(projectUpdateSchema),
    defaultValues: { body: '', evidenceUrl: '', isPublic: true },
  })

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState message={error?.message} onRetry={refetch} />
  if (!project) return <ErrorState title="Project not found" message="This project may have been removed." />

  const isLead = Boolean(user?.id && project.leadUserId && user.id === project.leadUserId)
  const canEdit = canManage || isLead

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
          <div className="flex flex-wrap items-center gap-3">
            {canCreateProject ? (
              <Button asChild size="sm" variant="outline">
                <Link to={`/projects/new?ideaId=${project.ideaId}`}>Create project</Link>
              </Button>
            ) : null}
            {canEdit ? (
              <Button asChild size="sm" variant="outline">
                <Link to={`/projects/${project.id}/edit`}>
                  <Pencil className="h-4 w-4" aria-hidden="true" />
                  Edit
                </Link>
              </Button>
            ) : null}
            {canEdit ? (
              <Button size="sm" variant="outline" onClick={() => setConfirmingDelete(true)}>
                <Trash2 className="h-4 w-4" aria-hidden="true" />
                Delete
              </Button>
            ) : null}
            <StatusBadge status={project.status} />
            <ProjectStatusSelector project={project} canManage={canManage} />
          </div>
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

      {canManage ? (
        <Card>
          <CardHeader className="flex items-center justify-between gap-2">
            <CardTitle>Outcomes</CardTitle>
            <ReportOutcome ideaId={project.ideaId} />
          </CardHeader>
          <CardContent>
            <OutcomesTable ideaId={project.ideaId} canVerify={can('admin') || can('moderator') || can('reviewer')} />
          </CardContent>
        </Card>
      ) : null}

      <FacilitationNotice />

      <ConfirmDialog
        open={confirmingDelete}
        onOpenChange={setConfirmingDelete}
        title="Delete this project?"
        description="This project will be removed for everyone. This cannot be undone."
        confirmLabel="Delete project"
        onConfirm={() => {
          remove.mutate(project.id, {
            onSuccess: () => navigate(project.idea?.slug ? `/ideas/${project.idea.slug}` : '/'),
          })
        }}
      />
    </div>
  )
}