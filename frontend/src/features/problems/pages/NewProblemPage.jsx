import { useNavigate, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { problemSchema, toErrorMessage } from '@/lib/validation'
import { useCreateProblem } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'

export default function NewProblemPage() {
  useDocumentTitle('Report a problem')
  const navigate = useNavigate()
  const create = useCreateProblem()
  const [searchParams] = useSearchParams()

  const form = useForm({
    resolver: zodResolver(problemSchema),
    defaultValues: {
      title: searchParams.get('title') ?? '',
      whoAffected: '',
      coarseLocation: '',
      whyItMatters: '',
      urgency: '',
      photoUrl: '',
    },
  })

  const onSubmit = async (values) => {
    try {
      const created = await create.mutateAsync(values)
      toast.success('Problem reported. Thank you!')
      const id = created?.id ?? created?.problemId
      navigate(id ? `/problems/${id}` : '/problems')
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <PageHeader
        title="Report a problem"
        description="Describe a real challenge so the community can propose solutions."
      />

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
              <Button type="submit" disabled={create.isPending}>
                {create.isPending ? 'Submitting…' : 'Submit problem'}
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
