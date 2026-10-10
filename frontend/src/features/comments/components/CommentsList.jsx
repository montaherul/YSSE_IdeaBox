import { MessageSquare } from 'lucide-react'
import { useIdeaComments } from '../hooks'
import { EmptyState } from '@/components/shared/EmptyState'
import { CardSkeleton } from '@/components/shared/LoadingSkeleton'
import { timeAgo } from '@/lib/format'

/** Read-only comment list (the write form lives on the idea detail page). */
export function CommentsList({ ideaId }) {
  const { data, isLoading } = useIdeaComments(ideaId)
  const items = data?.items ?? data ?? []

  if (isLoading) return <CardSkeleton className="mt-2" />

  if (items.length === 0) {
    return <EmptyState icon={MessageSquare} compact title="No comments yet" description="Start the conversation below." />
  }

  return (
    <ul className="space-y-3">
      {items.map((comment) => (
        <li key={comment.id} className="flex items-start gap-3 rounded-lg border p-3">
          <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
            {(comment.author?.displayName ?? comment.authorName ?? '?').charAt(0).toUpperCase()}
          </div>
          <div className="min-w-0 flex-1">
            <div className="flex items-center justify-between gap-2">
              <p className="text-sm font-medium">{comment.author?.displayName ?? comment.authorName ?? 'Guest'}</p>
              <span className="text-xs text-muted-foreground">{timeAgo(comment.createdAt)}</span>
            </div>
            <p className="mt-1 whitespace-pre-line text-sm text-muted-foreground">{comment.body}</p>
          </div>
        </li>
      ))}
    </ul>
  )
}

export default CommentsList