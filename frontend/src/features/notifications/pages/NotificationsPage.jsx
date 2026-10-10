import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Bell, CheckCheck } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useNotifications, useMarkNotificationRead, useMarkAllNotificationsRead } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { CardSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { timeAgo } from '@/lib/format'
import { DEFAULT_PAGE_SIZE, NOTIFICATION_TYPE_LABELS } from '@/config/constants'

function notificationHref(notification) {
  if (notification.ideaId) return `/ideas/id/${notification.ideaId}`
  if (notification.conversationId) return `/messages/${notification.conversationId}`
  if (notification.listingId) return `/marketplace/${notification.listingId}`
  if (notification.problemId) return `/problems/${notification.problemId}`
  if (notification.targetUrl) return notification.targetUrl
  return undefined
}

export default function NotificationsPage() {
  useDocumentTitle('Notifications')
  const [page, setPage] = useState(1)
  const { data, isLoading, isError, refetch } = useNotifications({ page, pageSize: DEFAULT_PAGE_SIZE })
  const read = useMarkNotificationRead()
  const readAll = useMarkAllNotificationsRead()

  const items = data?.items ?? []
  const total = data?.total ?? 0

  return (
    <div className="space-y-6">
      <PageHeader
        title="Notifications"
        description="Updates about your ideas, inquiries, messages and community activity."
        actions={
          items.some((n) => !n.isRead) ? (
            <Button size="sm" variant="outline" onClick={() => readAll.mutate()}>
              <CheckCheck className="h-4 w-4" aria-hidden="true" />
              Mark all read
            </Button>
          ) : null
        }
      />

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2">
          <CardSkeleton />
          <CardSkeleton />
        </div>
      ) : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState icon={Bell} title="No notifications" description="You are all caught up." />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="space-y-3">
            {items.map((notification) => {
              const label = NOTIFICATION_TYPE_LABELS[notification.type] ?? notification.type
              const href = notificationHref(notification)
              const body = (
                <Card className={!notification.isRead ? 'border-primary/50 bg-primary/5' : ''}>
                  <CardContent className="flex items-start gap-3 p-4">
                    <span
                      className={`mt-1.5 h-2 w-2 shrink-0 rounded-full ${notification.isRead ? 'bg-muted-foreground/40' : 'bg-primary'}`}
                      aria-hidden="true"
                    />
                    <div className="min-w-0 flex-1">
                      <p className="flex flex-wrap items-center gap-x-2 gap-y-0.5 text-sm">
                        <span className="font-medium">{label}</span>
                        {notification.title ? <span className="text-muted-foreground">· {notification.title}</span> : null}
                        <span className="ml-auto shrink-0 text-xs text-muted-foreground">{timeAgo(notification.createdAt)}</span>
                      </p>
                      {notification.body ? <p className="mt-1 whitespace-pre-line text-sm text-muted-foreground">{notification.body}</p> : null}
                    </div>
                  </CardContent>
                </Card>
              )
              const content = href ? (
                <Link to={href} className="block focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring rounded-xl" onClick={() => !notification.isRead && read.mutate(notification.id)}>
                  {body}
                </Link>
              ) : (
                <button
                  type="button"
                  className="block w-full text-left"
                  onClick={() => !notification.isRead && read.mutate(notification.id)}
                >
                  {body}
                </button>
              )
              return <div key={notification.id}>{content}</div>
            })}
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={total} onChange={setPage} />
        </>
      ) : null}
    </div>
  )
}