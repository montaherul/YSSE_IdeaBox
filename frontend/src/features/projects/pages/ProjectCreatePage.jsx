import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery } from '@tanstack/react-query'
import { Check, FolderKanban, Search } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { projectSchema, toErrorMessage } from '@/lib/validation'
import { useCreateProject, useProjectLeadCandidates } from '@/features/projects/hooks'
import { ideasApi } from '@/features/ideas/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { UserAvatar } from '@/components/shared/UserAvatar'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { EmptyState } from '@/components/shared/EmptyState'
import { cn } from '@/lib/utils'

const LEAD_ROLES = new Set(['Entrepreneur', 'Investor'])

function IdeaPicker({ form }) {
  const [search, setSearch] = useState('')
  const ideaId = form.watch('ideaId')

  const { data: selectedIdea, isError: selectedIdeaError } = useQuery({
    queryKey: ['ideas', 'id', ideaId],
    queryFn: () => ideasApi.byId(Number(ideaId)),
    enabled: Boolean(ideaId),
    retry: false,
  })

  const { data: results, isFetching } = useQuery({
    queryKey: ['ideas', 'search', search],
    queryFn: () => ideasApi.list({ search, pageSize: 8 }),
    enabled: search.trim().length >= 2,
  })

  if (ideaId) {
    return (
      <div className="flex flex-wrap items-center justify-between gap-3 rounded-lg border bg-muted/30 p-3">
        <div className="flex items-center gap-2">
          <FolderKanban className="h-4 w-4 text-primary" aria-hidden="true" />
          <span className="text-sm font-medium">
            {selectedIdea?.title ?? (selectedIdeaError ? `Idea #${ideaId} (not available)` : 'Loading idea…')}
          </span>
        </div>
        <Button
          type="button"
          size="sm"
          variant="outline"
          onClick={() => form.setValue('ideaId', '', { shouldValidate: false })}
        >
          Change idea
        </Button>
      </div>
    )
  }

  const items = results?.items ?? []

  return (
    <div className="space-y-3">
      <div className="relative">
        <Search
          className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
          aria-hidden="true"
        />
        <Input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search approved ideas by title"
          aria-label="Search ideas"
          className="pl-9"
        />
      </div>

      {search.trim().length < 2 ? (
        <p className="text-sm text-muted-foreground">Type at least 2 characters to search published ideas.</p>
      ) : isFetching ? (
        <p className="text-sm text-muted-foreground">Searching…</p>
      ) : items.length === 0 ? (
        <p className="text-sm text-muted-foreground">No published ideas match this search.</p>
      ) : (
        <ul className="space-y-2" aria-label="Idea search results">
          {items.map((idea) => (
            <li key={idea.id}>
              <button
                type="button"
                onClick={() => form.setValue('ideaId', idea.id, { shouldValidate: true })}
                className="w-full rounded-lg border p-3 text-left text-sm transition-colors hover:bg-accent focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              >
                <span className="font-medium">{idea.title}</span>
                {idea.shortSummary ? (
                  <span className="mt-0.5 block text-muted-foreground">{idea.shortSummary}</span>
                ) : null}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

function LeadPicker({ form }) {
  const [search, setSearch] = useState('')
  const leadUserId = form.watch('leadUserId')
  const { data: candidates, isFetching } = useProjectLeadCandidates(search)

  const items = candidates ?? []
  const selected = items.find((candidate) => String(candidate.userId) === String(leadUserId))

  if (leadUserId) {
    return (
      <div className="flex flex-wrap items-center justify-between gap-3 rounded-lg border bg-muted/30 p-3">
        <div className="flex items-center gap-2">
          <UserAvatar name={selected?.displayName ?? 'Lead'} className="h-8 w-8" />
          <div>
            <p className="text-sm font-medium">{selected?.displayName ?? `User #${leadUserId}`}</p>
            {selected ? (
              <p className="text-xs text-muted-foreground">
                {(selected.roles ?? []).filter((role) => LEAD_ROLES.has(role)).join(' · ')}
              </p>
            ) : null}
          </div>
        </div>
        <Button
          type="button"
          size="sm"
          variant="outline"
          onClick={() => form.setValue('leadUserId', '', { shouldValidate: false })}
        >
          Change lead
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      <div className="relative">
        <Search
          className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
          aria-hidden="true"
        />
        <Input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search entrepreneurs and investors by name"
          aria-label="Search project leads"
          className="pl-9"
        />
      </div>

      {isFetching && items.length === 0 ? (
        <p className="text-sm text-muted-foreground">Searching…</p>
      ) : items.length === 0 ? (
        <EmptyState
          title="No candidates found"
          description="Users holding the Entrepreneur or Investor role appear here."
        />
      ) : (
        <ul className="max-h-72 space-y-2 overflow-y-auto" aria-label="Project lead candidates">
          {items.map((candidate) => (
            <li key={candidate.userId}>
              <button
                type="button"
                onClick={() => form.setValue('leadUserId', candidate.userId, { shouldValidate: true })}
                className={cn(
                  'flex w-full items-center justify-between gap-2 rounded-lg border p-3 text-left text-sm transition-colors hover:bg-accent',
                  'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                )}
              >
                <span className="flex items-center gap-2">
                  <UserAvatar name={candidate.displayName} className="h-8 w-8" />
                  <span>
                    <span className="block font-medium">{candidate.displayName}</span>
                    <span className="block text-xs text-muted-foreground">
                      {(candidate.roles ?? []).filter((role) => LEAD_ROLES.has(role)).join(' · ')}
                    </span>
                  </span>
                </span>
                <Check className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

export default function ProjectCreatePage() {
  useDocumentTitle('Create a project')
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const create = useCreateProject()

  const form = useForm({
    resolver: zodResolver(projectSchema),
    defaultValues: {
      ideaId: searchParams.get('ideaId') ?? '',
      leadUserId: '',
      name: '',
      objectives: '',
    },
  })

  const onSubmit = async (values) => {
    try {
      const created = await create.mutateAsync(values)
      // Backend returns Result<int>: the unwrapped payload is the new project id.
      const newId = created?.id ?? created
      form.reset()
      if (newId) navigate(`/projects/${newId}`)
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <PageHeader
        title="Create a project"
        description="Select an approved idea and the entrepreneur or investor who will lead the project."
      />

      <Card>
        <CardContent className="p-6">
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-6">
            {form.formState.errors.root ? (
              <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {form.formState.errors.root.message}
              </p>
            ) : null}

            <section className="space-y-3">
              <h2 className="text-base font-semibold">Idea</h2>
              <IdeaPicker form={form} />
              {form.formState.errors.ideaId ? (
                <p role="alert" className="text-sm text-destructive">
                  {form.formState.errors.ideaId.message || 'Select an idea for this project.'}
                </p>
              ) : null}
            </section>

            <section className="space-y-3">
              <h2 className="text-base font-semibold">Project lead</h2>
              <LeadPicker form={form} />
              {form.formState.errors.leadUserId ? (
                <p role="alert" className="text-sm text-destructive">
                  {form.formState.errors.leadUserId.message}
                </p>
              ) : null}
            </section>

            <FormField id="project-name" label="Project name" required error={form.formState.errors.name?.message}>
              <Input id="project-name" maxLength={200} {...form.register('name')} />
            </FormField>

            <FormField id="project-objectives" label="Objectives (optional)" error={form.formState.errors.objectives?.message}>
              <Textarea id="project-objectives" rows={5} maxLength={2000} {...form.register('objectives')} />
            </FormField>

            <div className="flex flex-wrap gap-3">
              <Button type="submit" disabled={create.isPending}>
                {create.isPending ? 'Creating…' : 'Create project'}
              </Button>
              <Button type="button" variant="outline" onClick={() => navigate(-1)}>
                Cancel
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
