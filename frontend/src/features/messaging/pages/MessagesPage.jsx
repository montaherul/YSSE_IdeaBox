import { useState } from 'react'
import { Link } from 'react-router-dom'
import { MessageSquare } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useConversations } from '../hooks'
import { UserAvatar } from '@/components/shared/UserAvatar'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { CardSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { timeAgo } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

export default function MessagesPage() {
  useDocumentTitle('Messages')
  const [page, setPage] = useState(1)
  const { data, isLoading, isError, refetch } = useConversations({ page, pageSize: DEFAULT_PAGE_SIZE })

  const items = data?.items ?? []
  const total = data?.total ?? 0

  return (
    <div className="space-y-6">
      <PageHeader title="Messages" description="Your private conversations with community members." />

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2">
          <CardSkeleton />
          <CardSkeleton />
        </div>
      ) : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState
          icon={MessageSquare}
          title="No conversations yet"
          description="Start a conversation from an idea or inquiry."
        />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="space-y-3">
            {items.map((conversation) => {
              const other = conversation.otherParticipant ?? conversation.participant
              const unread = Number(conversation.unreadCount ?? 0)
              return (
                <Link key={conversation.id} to={`/messages/${conversation.id}`} className="block rounded-xl border bg-card p-4 shadow-sm transition-shadow hover:shadow-md">
                  <div className="flex items-center gap-4">
                    <UserAvatar name={other?.displayName} src={other?.imageUrl} className="h-10 w-10" />
                    <div className="min-w-0 flex-1">
                      <p className="flex items-center justify-between gap-2 font-medium">
                        <span className="truncate">{other?.displayName ?? 'Member'}</span>
                        {conversation.updatedAt ? <span className="shrink-0 text-xs font-normal text-muted-foreground">{timeAgo(conversation.updatedAt)}</span> : null}
                      </p>
                      <p className="mt-0.5 truncate text-sm text-muted-foreground">{conversation.lastMessage ?? 'No messages yet'}</p>
                    </div>
                    {unread > 0 ? (
                      <span className="inline-flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1.5 text-xs font-semibold text-primary-foreground" aria-label={`${unread} unread messages`}>
                        {unread}
                      </span>
                    ) : null}
                  </div>
                </Link>
              )
            })}
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={total} onChange={setPage} />
        </>
      ) : null}
    </div>
  )
}