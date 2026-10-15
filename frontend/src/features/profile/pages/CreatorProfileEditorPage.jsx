import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { UserRoundX } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { creatorProfileSchema, toErrorMessage } from '@/lib/validation'
import {
  useCreatorProfile,
  useUpdateCreatorProfile,
  useDeleteCreatorProfile,
} from '@/features/profile/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'

export default function CreatorProfileEditorPage() {
  useDocumentTitle('Creator profile')
  const navigate = useNavigate()
  const { data: profile, isLoading } = useCreatorProfile()
  const update = useUpdateCreatorProfile()
  const remove = useDeleteCreatorProfile()
  const [confirmingDelete, setConfirmingDelete] = useState(false)

  const form = useForm({
    resolver: zodResolver(creatorProfileSchema),
    defaultValues: profile
      ? {
          alias: profile.alias ?? '',
          headline: profile.headline ?? '',
          shortBio: profile.shortBio ?? '',
          district: profile.district ?? '',
          languages: profile.languages ?? '',
          skillsOffered: profile.skillsOffered ?? '',
          helpNeeded: profile.helpNeeded ?? '',
          areasOfInterest: profile.areasOfInterest ?? '',
          links: profile.links ?? '',
          availability: profile.availability ?? '',
          photoUrl: profile.photoUrl ?? '',
        }
      : {
          alias: '',
          headline: '',
          shortBio: '',
          district: '',
          languages: '',
          skillsOffered: '',
          helpNeeded: '',
          areasOfInterest: '',
          links: '',
          availability: '',
          photoUrl: '',
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
        title="Creator profile"
        description="Share who you are so the community can collaborate with you on ideas."
        actions={
          profile && (
            <Button variant="ghost" size="sm" onClick={() => setConfirmingDelete(true)}>
              <UserRoundX className="h-4 w-4" aria-hidden="true" />
              Remove profile
            </Button>
          )
        }
      />

      <Card>
        <CardContent className="p-6">
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
            {form.formState.errors.root ? (
              <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {form.formState.errors.root.message}
              </p>
            ) : null}

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField id="creator-alias" label="Alias" required error={form.formState.errors.alias?.message}>
                <Input id="creator-alias" placeholder="How you want to be known" {...form.register('alias')} />
              </FormField>
              <FormField id="creator-headline" label="Headline (optional)" error={form.formState.errors.headline?.message}>
                <Input id="creator-headline" placeholder="Community health innovator" {...form.register('headline')} />
              </FormField>
            </div>

            <FormField id="creator-bio" label="Short bio (optional)" error={form.formState.errors.shortBio?.message}>
              <Textarea id="creator-bio" rows={4} {...form.register('shortBio')} />
            </FormField>

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField id="creator-district" label="District (optional)" error={form.formState.errors.district?.message}>
                <Input id="creator-district" {...form.register('district')} />
              </FormField>
              <FormField id="creator-languages" label="Languages (optional)" error={form.formState.errors.languages?.message}>
                <Input id="creator-languages" placeholder="English, Hausa" {...form.register('languages')} />
              </FormField>
            </div>

            <FormField id="creator-skills" label="Skills offered (optional)" error={form.formState.errors.skillsOffered?.message}>
              <Textarea id="creator-skills" rows={2} {...form.register('skillsOffered')} />
            </FormField>

            <FormField id="creator-help" label="Help needed (optional)" error={form.formState.errors.helpNeeded?.message}>
              <Textarea id="creator-help" rows={2} {...form.register('helpNeeded')} />
            </FormField>

            <FormField id="creator-interests" label="Areas of interest (optional)" error={form.formState.errors.areasOfInterest?.message}>
              <Textarea id="creator-interests" rows={2} {...form.register('areasOfInterest')} />
            </FormField>

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField id="creator-links" label="Links (optional)" error={form.formState.errors.links?.message}>
                <Input id="creator-links" placeholder="https://…" {...form.register('links')} />
              </FormField>
              <FormField id="creator-photo" label="Photo URL (optional)" error={form.formState.errors.photoUrl?.message}>
                <Input id="creator-photo" placeholder="https://…" {...form.register('photoUrl')} />
              </FormField>
            </div>

            <FormField id="creator-availability" label="Availability (optional)" error={form.formState.errors.availability?.message}>
              <Input id="creator-availability" placeholder="Weekends" {...form.register('availability')} />
            </FormField>

            <div className="flex gap-2">
              <Button type="submit" disabled={update.isPending}>
                {update.isPending ? 'Saving…' : profile ? 'Save changes' : 'Create profile'}
              </Button>
              {profile ? (
                <Button type="button" variant="outline" onClick={() => navigate(`/creator/${profile.userId}`)}>
                  View public profile
                </Button>
              ) : null}
            </div>
          </form>
        </CardContent>
      </Card>

      <ConfirmDialog
        open={confirmingDelete}
        onOpenChange={(open) => !open && setConfirmingDelete(false)}
        title="Remove your creator profile?"
        description="Your public creator profile will be deleted. This does not remove your ideas."
        confirmLabel="Remove"
        variant="destructive"
        onConfirm={() => {
          remove.mutate(undefined, {
            onSuccess: () => navigate('/app/profile'),
          })
          setConfirmingDelete(false)
        }}
      />
    </div>
  )
}