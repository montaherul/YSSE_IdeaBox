import {
  useNotificationPreferences,
  useUpdateNotificationPreference,
} from '@/features/notifications/hooks'
import { ErrorState } from '@/components/shared/ErrorState'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Switch } from '@/components/ui/switch'
import { NOTIFICATION_TYPE_LABELS } from '@/config/constants'

const CHANNEL_LABELS = { InApp: 'In-app', Email: 'Email' }
const LOCKED_TYPE = 'AccountNotice'

export function NotificationPreferencesCard() {
  const { data, isLoading, isError, refetch } = useNotificationPreferences()
  const update = useUpdateNotificationPreference()

  const preferences = Array.isArray(data) ? data : []
  const byType = new Map()
  const types = []
  for (const preference of preferences) {
    if (!byType.has(preference.type)) {
      byType.set(preference.type, [])
      types.push(preference.type)
    }
    byType.get(preference.type).push(preference)
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Notification preferences</CardTitle>
        <CardDescription>
          Choose how you receive each type of notification. Security and account notices cannot be disabled.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        {isLoading ? <p className="text-sm text-muted-foreground">Loading preferences…</p> : null}
        {isError ? <ErrorState compact onRetry={refetch} /> : null}
        {!isLoading && !isError && types.length === 0 ? (
          <p className="text-sm text-muted-foreground">No preferences available.</p>
        ) : null}
        {!isLoading && !isError && types.length > 0 ? (
          <ul className="divide-y">
            {types.map((type) => (
              <li key={type} className="flex flex-wrap items-center justify-between gap-3 py-3">
                <span className="text-sm font-medium">{NOTIFICATION_TYPE_LABELS[type] ?? type}</span>
                <div className="flex flex-wrap items-center gap-4">
                  {byType.get(type).map((preference) => {
                    const locked = preference.type === LOCKED_TYPE
                    const typeLabel = NOTIFICATION_TYPE_LABELS[preference.type] ?? preference.type
                    const channelLabel = CHANNEL_LABELS[preference.channel] ?? preference.channel
                    return (
                      <span key={preference.channel} className="flex items-center gap-2">
                        <Switch
                          id={`pref-${preference.type}-${preference.channel}`}
                          checked={Boolean(preference.isEnabled)}
                          disabled={locked || update.isPending}
                          onCheckedChange={(checked) => {
                            if (locked) return
                            update.mutate({
                              type: preference.type,
                              channel: preference.channel,
                              isEnabled: checked,
                            })
                          }}
                          aria-label={`${typeLabel} via ${channelLabel}`}
                        />
                        <span className="text-sm text-muted-foreground">{channelLabel}</span>
                      </span>
                    )
                  })}
                </div>
              </li>
            ))}
          </ul>
        ) : null}
      </CardContent>
    </Card>
  )
}

export default NotificationPreferencesCard
