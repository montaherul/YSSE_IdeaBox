import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { entrepreneurProfileSchema, toErrorMessage } from '@/lib/validation'
import { useEntrepreneurProfile, useUpdateEntrepreneurProfile } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

const KINDS = ['SME', 'Startup', 'Investor', 'NGO', 'Education', 'Other']

export default function EntrepreneurProfilePage() {
  useDocumentTitle('Business profile')
  const { data: profile, isLoading } = useEntrepreneurProfile()
  const update = useUpdateEntrepreneurProfile()

  const form = useForm({
    resolver: zodResolver(entrepreneurProfileSchema),
    defaultValues: profile
      ? {
          kind: profile.kind ?? '',
          organization: profile.organization ?? '',
          industry: profile.industry ?? '',
          description: profile.description ?? '',
          website: profile.website ?? '',
          coarseLocation: profile.coarseLocation ?? '',
          interestAreas: profile.interestAreas ?? '',
          expertise: profile.expertise ?? '',
          preferredCollaborationTypes: profile.preferredCollaborationTypes ?? '',
        }
      : {
          kind: '',
          organization: '',
          industry: '',
          description: '',
          website: '',
          coarseLocation: '',
          interestAreas: '',
          expertise: '',
          preferredCollaborationTypes: '',
        },
  })

  if (isLoading) return <PageSkeleton lines={6} />

  const onSubmit = async (values) => {
    try {
      await update.mutateAsync(values)
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <PageHeader
        title="Business profile"
        description="Tell the community what your organization does so we can match you with relevant ideas."
      />

      {profile?.status && profile.status !== 'Approved' ? (
        <p className="rounded-lg border bg-warning/10 px-4 py-3 text-sm text-warning">
          Your profile is {profile.status}. It will be reviewed by the moderation team before you can participate fully.
        </p>
      ) : null}

      <Card>
        <CardContent className="p-6">
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
            {form.formState.errors.root ? (
              <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {form.formState.errors.root.message}
              </p>
            ) : null}

            <FormField id="ent-kind" label="Profile type" required error={form.formState.errors.kind?.message}>
              <Controller
                control={form.control}
                name="kind"
                render={({ field }) => (
                  <Select value={field.value ?? ''} onValueChange={field.onChange}>
                    <SelectTrigger id="ent-kind" aria-label="Profile type">
                      <SelectValue placeholder="Choose a profile type" />
                    </SelectTrigger>
                    <SelectContent>
                      {KINDS.map((k) => (
                        <SelectItem key={k} value={k}>{k}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
            </FormField>

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField id="ent-org" label="Organization" required error={form.formState.errors.organization?.message}>
                <Input id="ent-org" {...form.register('organization')} />
              </FormField>
              <FormField id="ent-industry" label="Industry" required error={form.formState.errors.industry?.message}>
                <Input id="ent-industry" {...form.register('industry')} />
              </FormField>
            </div>

            <FormField id="ent-desc" label="Description" error={form.formState.errors.description?.message}>
              <Textarea id="ent-desc" rows={4} {...form.register('description')} />
            </FormField>

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField id="ent-website" label="Website (optional)" error={form.formState.errors.website?.message}>
                <Input id="ent-website" placeholder="https://…" {...form.register('website')} />
              </FormField>
              <FormField id="ent-location" label="Location (optional)" error={form.formState.errors.coarseLocation?.message}>
                <Input id="ent-location" {...form.register('coarseLocation')} />
              </FormField>
            </div>

            <FormField id="ent-interests" label="Interest areas (optional)" error={form.formState.errors.interestAreas?.message}>
              <Textarea id="ent-interests" rows={2} {...form.register('interestAreas')} />
            </FormField>

            <FormField id="ent-expertise" label="Expertise (optional)" error={form.formState.errors.expertise?.message}>
              <Textarea id="ent-expertise" rows={2} {...form.register('expertise')} />
            </FormField>

            <FormField id="ent-collab" label="Preferred collaboration types (optional)" error={form.formState.errors.preferredCollaborationTypes?.message}>
              <Input id="ent-collab" {...form.register('preferredCollaborationTypes')} />
            </FormField>

            <Button type="submit" disabled={update.isPending}>
              {update.isPending ? 'Saving…' : profile ? 'Save changes' : 'Create profile'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}