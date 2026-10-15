import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { projectEditSchema, toErrorMessage } from '@/lib/validation'
import { useProject, useUpdateProject } from '@/features/projects/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'

export default function EditProjectPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { data: project, isLoading, isError, error } = useProject(id)
  const update = useUpdateProject()
  useDocumentTitle(project?.name ? `Edit ${project.name}` : 'Edit project')

  const form = useForm({
    resolver: zodResolver(projectEditSchema),
    defaultValues: {
      name: project?.name ?? '',
      objectives: project?.objectives ?? '',
    },
    values: project ? { name: project.name ?? '', objectives: project.objectives ?? '' } : undefined,
  })

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState message={error?.message} />
  if (!project) return <ErrorState title="Project not found" message="This project may have been removed." />

  const onSubmit = async (values) => {
    try {
      await update.mutateAsync({ id: project.id, payload: values })
      navigate(`/projects/${project.id}`)
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <PageHeader title="Edit project" description="Update the project name and objectives." />

      <Card>
        <CardContent className="p-6">
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
            {form.formState.errors.root ? (
              <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {form.formState.errors.root.message}
              </p>
            ) : null}

            <FormField id="project-name" label="Project name" required error={form.formState.errors.name?.message}>
              <Input id="project-name" maxLength={200} {...form.register('name')} />
            </FormField>

            <FormField id="project-objectives" label="Objectives (optional)" error={form.formState.errors.objectives?.message}>
              <Textarea id="project-objectives" rows={5} maxLength={2000} {...form.register('objectives')} />
            </FormField>

            <div className="flex flex-wrap gap-3">
              <Button type="submit" disabled={update.isPending}>
                {update.isPending ? 'Saving…' : 'Save changes'}
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
