import { useState } from 'react'
import { SlidersHorizontal, Save } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAdminSettings, useUpdateAdminSettings } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'

function SettingField({ label, value, type = 'text', onChange }) {
  const id = `setting-${label.replace(/\s+/g, '-').toLowerCase()}`
  return (
    <div className="flex flex-col gap-1.5">
      <label htmlFor={id} className="text-sm font-medium">{label}</label>
      {type === 'boolean' ? (
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            id={id}
            checked={Boolean(value)}
            onChange={(e) => onChange(e.target.checked)}
            className="h-4 w-4"
          />
          Enabled
        </label>
      ) : (
        <input
          id={id}
          type={type}
          value={value ?? ''}
          onChange={(e) => onChange(type === 'number' ? Number(e.target.value) : e.target.value)}
          className="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm"
        />
      )}
    </div>
  )
}

const SETTING_META = {
  allowGuestReviews: { label: 'Allow anonymous guest reviews', type: 'boolean' },
  requireScreening: { label: 'Require screening before publishing', type: 'boolean' },
  votingEnabled: { label: 'Voting enabled', type: 'boolean' },
  marketplaceEnabled: { label: 'Marketplace enabled', type: 'boolean' },
  entrepreneurshipEnabled: { label: 'Entrepreneur & inquiries enabled', type: 'boolean' },
  maxIdeasPerUser: { label: 'Max ideas per user', type: 'number' },
  platformNotice: { label: 'Platform notice', type: 'textarea' },
}

export default function AdminSettingsPage() {
  useDocumentTitle('Settings')
  const { data, isLoading, isError, refetch } = useAdminSettings()
  const update = useUpdateAdminSettings()
  const [draft, setDraft] = useState(null)

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState onRetry={refetch} />

  const settings = draft ?? data ?? {}
  const keys = Object.keys(settings)

  const setSetting = (key, value) => setDraft({ ...settings, [key]: value })

  const submit = () => {
    update.mutate(draft ?? settings)
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <PageHeader title="Platform settings" description="Global configuration. Empty values keep server defaults." />

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <SlidersHorizontal className="h-4 w-4" aria-hidden="true" />
            Configuration
          </CardTitle>
        </CardHeader>
        <CardContent>
          {keys.length === 0 ? (
            <p className="text-sm text-muted-foreground">No settings are currently exposed by the API. Configure defaults on the server instead.</p>
          ) : (
            <div className="grid gap-4 sm:grid-cols-2">
              {keys.map((key) => {
                const meta = SETTING_META[key] ?? { label: key, type: 'text' }
                return (
                  <SettingField
                    key={key}
                    label={meta.label}
                    type={meta.type === 'textarea' ? 'text' : meta.type}
                    value={settings[key]}
                    onChange={(v) => setSetting(key, v)}
                  />
                )
              })}
            </div>
          )}
          {keys.length > 0 ? (
            <div className="mt-6">
              <Button onClick={submit} disabled={update.isPending}>
                <Save className="h-4 w-4" aria-hidden="true" />
                {update.isPending ? 'Saving…' : 'Save settings'}
              </Button>
            </div>
          ) : null}
        </CardContent>
      </Card>
    </div>
  )
}