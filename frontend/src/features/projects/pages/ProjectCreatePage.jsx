import { useNavigate, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { projectSchema, toErrorMessage } from '@/lib/validation'
import { useCreateProject } from '@/features/projects/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'

export default function ProjectCreatePage() {
  useDocumentTitle('Create a project')
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const create = useCreateProject()

  const form = useForm({
    resolver: zodResolver(projectSchema),
    defaultValues: {
      ideaId: searchParams.get('ideaId') ?? '',
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
        description="Turn an idea into a collaborative project with clear objectives."
      />

      <Card>
        <CardContent className="p-6">
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
            {form.formState.errors.root ? (
              <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {form.formState.errors.root.message}
              </p>
            ) : null}

            <FormField
              id="project-idea-id"
              label="Idea ID"
              required
              hint="The numeric id of the idea this project belongs to"
              error={form.formState.errors.ideaId?.message}
            >
              <Input
                id="project-idea-id"
                type="number"
                min="1"
                step="1"
                inputMode="numeric"
                {...form.register('ideaId')}
              />
            </FormField>

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
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
