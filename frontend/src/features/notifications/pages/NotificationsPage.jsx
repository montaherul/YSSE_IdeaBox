import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Bell, CheckCheck } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import {
  useMarkNotificationRead,
  useMarkAllNotificationsRead,
  useDeleteNotification,
  useNotificationPreferences,
  useUpdateNotificationPreference,
} from '@/features/notifications/hooks'
import { notificationsApi } from '@/features/notifications/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { ErrorState } from '@/components/shared/ErrorState'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Switch } from '@/components/ui/switch'
import { timeAgo } from '@/lib/format'
import { DEFAULT_PAGE_SIZE, NOTIFICATION_TYPE_LABELS } from '@/config/constants'

const NOTIFICATIONS_QUERY_KEY = ['notifications', 'list']
const CHANNEL_LABELS = { InApp: 'In-app', Email: 'Email' }
const LOCKED_TYPE = 'AccountNotice'

const fetchNotifications = (params) => notificationsApi.list(params)

function NotificationPreferencesCard() {
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

export default function NotificationsPage() {
  useDocumentTitle('Notifications')
  const navigate = useNavigate()
  const read = useMarkNotificationRead()
  const readAll = useMarkAllNotificationsRead()
  const remove = useDeleteNotification()
  const [pendingDelete, setPendingDelete] = useState(null)

  const markRead = (notification) => {
    if (notification && !notification.isRead) read.mutate(notification.id)
  }

  const handleAction = (key, row) => {
    if (key === 'open' && row.linkUrl) {
      markRead(row)
      navigate(row.linkUrl)
    }
    if (key === 'read') markRead(row)
    if (key === 'delete') setPendingDelete(row)
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Notifications"
        description="Updates about your ideas, inquiries, messages and community activity."
      />

      <ServerDataTable
        queryKey={NOTIFICATIONS_QUERY_KEY}
        queryFn={fetchNotifications}
        pageSize={DEFAULT_PAGE_SIZE}
        searchable={false}
        emptyIcon={Bell}
        emptyTitle="No notifications"
        emptyDescription="You are all caught up."
        onRowClick={(row) => {
          if (row.linkUrl) {
            markRead(row)
            navigate(row.linkUrl)
          } else {
            markRead(row)
          }
        }}
        toolbar={
          <Button
            size="sm"
            variant="outline"
            onClick={() => readAll.mutate()}
            disabled={readAll.isPending}
          >
            <CheckCheck className="h-4 w-4" aria-hidden="true" />
            Mark all as read
          </Button>
        }
        columns={[
          {
            field: 'isRead',
            title: '',
            headerSort: false,
            width: 40,
            formatter: (cell) => {
              const dot = document.createElement('span')
              dot.className = `inline-block h-2 w-2 rounded-full ${
                cell.getValue() ? 'bg-muted-foreground/40' : 'bg-primary'
              }`
              dot.setAttribute('aria-hidden', 'true')
              return dot
            },
          },
          {
            field: 'type',
            title: 'Type',
            headerSort: false,
            minWidth: 150,
            formatter: (cell) => NOTIFICATION_TYPE_LABELS[cell.getValue()] ?? cell.getValue(),
          },
          { field: 'title', title: 'Title', headerSort: false, minWidth: 150,
            formatter: (cell) => cell.getValue() || '—' },
          { field: 'body', title: 'Details', headerSort: false, widthGrow: 2,
            formatter: (cell) => cell.getValue() || '—' },
          { field: 'createdAt', title: 'Time', headerSort: false, minWidth: 130,
            formatter: (cell) => timeAgo(cell.getValue()) },
        ]}
        actions={[
          { key: 'open', label: 'Open', visible: (row) => Boolean(row.linkUrl) },
          { key: 'read', label: 'Mark read', visible: (row) => !row.isRead },
          { key: 'delete', label: 'Delete', variant: 'destructive' },
        ]}
        onAction={handleAction}
      />

      <NotificationPreferencesCard />

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onOpenChange={(open) => !open && setPendingDelete(null)}
        title="Delete this notification?"
        description="The notification will be permanently removed."
        confirmLabel="Delete"
        onConfirm={() => {
          if (pendingDelete) {
            remove.mutate(pendingDelete.id)
            setPendingDelete(null)
          }
        }}
      />
    </div>
  )
}
