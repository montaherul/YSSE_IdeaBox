import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Bell, CheckCheck } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import {
  useMarkNotificationRead,
  useMarkAllNotificationsRead,
  useDeleteNotification,
} from '@/features/notifications/hooks'
import { notificationsApi } from '@/features/notifications/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { timeAgo } from '@/lib/format'
import { DEFAULT_PAGE_SIZE, NOTIFICATION_TYPE_LABELS } from '@/config/constants'

const NOTIFICATIONS_QUERY_KEY = ['notifications', 'list']

const fetchNotifications = (params) => notificationsApi.list(params)

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
            formatter: (cell) => (
              <span
                aria-hidden="true"
                className={`inline-block h-2 w-2 rounded-full ${
                  cell.getValue() ? 'bg-muted-foreground/40' : 'bg-primary'
                }`}
              />
            ),
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
