import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { LogOut, Save, Settings2, UserRound } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { memberProfileSchema, toErrorMessage } from '@/lib/validation'
import { useMemberProfile, useUpdateMemberProfile } from '../hooks'
import {
  useNotificationPreferences,
  useUpdateNotificationPreference,
} from '@/features/notifications/hooks'
import { useLogout } from '@/features/auth/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'

export default function SettingsPage() {
  useDocumentTitle('Settings')
  const [confirmLogout, setConfirmLogout] = useState(false)
  const { data: profile, isLoading } = useMemberProfile()
  const update = useUpdateMemberProfile()
  const prefs = useNotificationPreferences()
  const updatePref = useUpdateNotificationPreference()
  const logout = useLogout()

  const form = useForm({
    resolver: zodResolver(memberProfileSchema),
    defaultValues: {
      displayName: profile?.displayName ?? '',
      biography: profile?.biography ?? '',
      skills: profile?.skills ?? '',
      interests: profile?.interests ?? '',
      imageUrl: profile?.imageUrl ?? '',
      preferredLanguage: profile?.preferredLanguage ?? 'en',
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

  const prefItems = prefs.data?.items ?? prefs.data ?? []

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <PageHeader title="Settings" description="Manage your profile, notifications and account." />

      <Tabs defaultValue="profile">
        <TabsList>
          <TabsTrigger value="profile">
            <UserRound className="mr-1.5 h-4 w-4" aria-hidden="true" />
            Profile
          </TabsTrigger>
          <TabsTrigger value="notifications">
            <Settings2 className="mr-1.5 h-4 w-4" aria-hidden="true" />
            Notifications
          </TabsTrigger>
          <TabsTrigger value="account">Account</TabsTrigger>
        </TabsList>

        <TabsContent value="profile" className="pt-4">
          <Card>
            <CardContent className="p-6">
              <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
                {form.formState.errors.root ? (
                  <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                    {form.formState.errors.root.message}
                  </p>
                ) : null}
                <FormField id="profile-name" label="Display name" required error={form.formState.errors.displayName?.message}>
                  <Input id="profile-name" {...form.register('displayName')} />
                </FormField>
                <FormField id="profile-bio" label="Biography (optional)" error={form.formState.errors.biography?.message}>
                  <Textarea id="profile-bio" rows={4} {...form.register('biography')} />
                </FormField>
                <div className="grid gap-4 sm:grid-cols-2">
                  <FormField id="profile-skills" label="Skills (optional)" error={form.formState.errors.skills?.message}>
                    <Input id="profile-skills" {...form.register('skills')} />
                  </FormField>
                  <FormField id="profile-interests" label="Interests (optional)" error={form.formState.errors.interests?.message}>
                    <Input id="profile-interests" {...form.register('interests')} />
                  </FormField>
                </div>
                <FormField id="profile-image" label="Photo URL (optional)" error={form.formState.errors.imageUrl?.message}>
                  <Input id="profile-image" {...form.register('imageUrl')} placeholder="https://…" />
                </FormField>
                <Button type="submit" disabled={update.isPending}>
                  <Save className="h-4 w-4" aria-hidden="true" />
                  {update.isPending ? 'Saving…' : 'Save profile'}
                </Button>
              </form>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="notifications" className="pt-4">
          <Card>
            <CardHeader>
              <CardTitle>Notification preferences</CardTitle>
            </CardHeader>
            <CardContent>
              {prefItems.length === 0 ? (
                <p className="text-sm text-muted-foreground">No notification preferences available.</p>
              ) : (
                <ul className="divide-y">
                  {prefItems.map((pref) => (
                    <li key={pref.type ?? pref.id} className="flex items-center justify-between gap-3 py-3">
                      <div>
                        <p className="text-sm font-medium">{pref.label ?? pref.type}</p>
                        {pref.description ? <p className="text-xs text-muted-foreground">{pref.description}</p> : null}
                      </div>
                      <label className="relative inline-flex items-center">
                        <input
                          type="checkbox"
                          className="peer sr-only"
                          checked={Boolean(pref.enabled)}
                          onChange={(e) => updatePref.mutate({ type: pref.type, enabled: e.target.checked })}
                        />
                        <span className="h-6 w-11 rounded-full bg-muted transition-colors after:absolute after:left-0.5 after:top-0.5 after:h-5 after:w-5 after:rounded-full after:bg-white after:shadow peer-checked:bg-primary peer-checked:after:translate-x-5 peer-focus-visible:ring-2 peer-focus-visible:ring-ring" aria-hidden="true" />
                        <span className="sr-only">Toggle {pref.label ?? pref.type}</span>
                      </label>
                    </li>
                  ))}
                </ul>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="account" className="pt-4">
          <Card>
            <CardHeader>
              <CardTitle>Account</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-muted-foreground">
                Sign out of Idea Box on this device.
              </p>
              <Button variant="outline" onClick={() => setConfirmLogout(true)}>
                <LogOut className="h-4 w-4" aria-hidden="true" />
                Sign out
              </Button>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <ConfirmDialog
        open={confirmLogout}
        onOpenChange={setConfirmLogout}
        title="Sign out?"
        description="You will need to sign in again to access the workspace."
        confirmLabel="Sign out"
        variant="default"
        onConfirm={() => logout.mutate()}
      />
    </div>
  )
}