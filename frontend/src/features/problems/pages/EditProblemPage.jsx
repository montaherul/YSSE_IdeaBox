import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { problemSchema, toErrorMessage } from '@/lib/validation'
import { useProblem, useUpdateProblem } from '@/features/problems/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'

export default function EditProblemPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { data: problem, isLoading, isError, error } = useProblem(id)
  const update = useUpdateProblem()
  useDocumentTitle(problem?.title ? `Edit ${problem.title}` : 'Edit problem')

  const form = useForm({
    resolver: zodResolver(problemSchema),
    defaultValues: {
      title: problem?.title ?? '',
      whoAffected: problem?.whoAffected ?? '',
      coarseLocation: problem?.coarseLocation ?? '',
      whyItMatters: problem?.whyItMatters ?? '',
      urgency: problem?.urgency ?? '',
      photoUrl: problem?.photoUrl ?? '',
    },
  })

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState message={error?.message} />
  if (!problem) return <ErrorState title="Problem not found" message="This problem may have been closed or removed." />

  const onSubmit = async (values) => {
    try {
      await update.mutateAsync({ id: problem.id, payload: values })
      navigate(`/problems/${problem.id}`)
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <PageHeader title="Edit problem" description="Update the problem details for the community." />

      <Card>
        <CardContent className="p-6">
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
            {form.formState.errors.root ? (
              <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {form.formState.errors.root.message}
              </p>
            ) : null}

            <FormField id="problem-title" label="Title" required hint="Up to 150 characters" error={form.formState.errors.title?.message}>
              <Input id="problem-title" maxLength={150} {...form.register('title')} />
            </FormField>

            <FormField id="problem-who" label="Who is affected?" required error={form.formState.errors.whoAffected?.message}>
              <Textarea id="problem-who" rows={3} {...form.register('whoAffected')} />
            </FormField>

            <FormField id="problem-why" label="Why does it matter?" required error={form.formState.errors.whyItMatters?.message}>
              <Textarea id="problem-why" rows={5} {...form.register('whyItMatters')} />
            </FormField>

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField id="problem-location" label="Location (optional)" error={form.formState.errors.coarseLocation?.message}>
                <Input id="problem-location" {...form.register('coarseLocation')} />
              </FormField>
              <FormField id="problem-urgency" label="Urgency (optional)" error={form.formState.errors.urgency?.message}>
                <Input id="problem-urgency" placeholder="e.g. High, Medium, Low" {...form.register('urgency')} />
              </FormField>
            </div>

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