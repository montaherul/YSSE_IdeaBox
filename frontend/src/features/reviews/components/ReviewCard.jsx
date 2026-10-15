import { Link } from 'react-router-dom'
import { Card, CardContent } from '@/components/ui/card'
import { UserAvatar } from '@/components/shared/UserAvatar'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { ResourceActions } from '@/features/_shared'
import { formatDate } from '@/lib/format'

function StarRating({ value }) {
  if (!value) return null
  return (
    <p aria-label={`Rated ${value} out of 5`} className="text-warning">
      {'★'.repeat(value)}
      <span className="text-muted-foreground" aria-hidden="true">
        {'★'.repeat(Math.max(0, 5 - value))}
      </span>
    </p>
  )
}

/**
 * Presentational review card.
 *
 * Props:
 * - review: the review record (author, rating?, body, createdAt, status?, idea…)
 * - actions?: declarative ResourceActions descriptors for owner/moderator actions
 * - showAuthor/showIdea/showStatus: toggle which header pieces render
 */
export function ReviewCard({
  review,
  actions = [],
  showAuthor = true,
  showIdea = false,
  showStatus = true,
  className,
}) {
  if (!review) return null

  const { body, rating, createdAt, displayName, authorName, status, ideaId, ideaTitle, ideaSlug } =
    review
  const author = displayName ?? authorName ?? 'Anonymous'
  const ideaTarget = ideaSlug ? `/ideas/${ideaSlug}` : ideaId ? `/ideas/id/${ideaId}` : null

  return (
    <Card className={className}>
      <CardContent className="space-y-3 p-5">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div className="min-w-0 space-y-1">
            {showAuthor ? (
              <div className="flex items-center gap-2">
                <UserAvatar name={author} className="h-8 w-8" />
                <span className="text-sm font-medium">{author}</span>
              </div>
            ) : null}
            {showIdea ? (
              ideaTarget ? (
                <Link
                  to={ideaTarget}
                  className="text-sm font-medium text-primary hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                  {ideaTitle ?? `Idea #${ideaId}`}
                </Link>
              ) : (
                <span className="text-sm font-medium">{ideaTitle ?? `Idea #${ideaId}`}</span>
              )
            ) : null}
          </div>
          <div className="flex shrink-0 items-center gap-2">
            {showStatus && status ? <StatusBadge status={status} /> : null}
            {createdAt ? (
              <time className="text-xs text-muted-foreground" dateTime={createdAt}>
                {formatDate(createdAt, { short: true })}
              </time>
            ) : null}
          </div>
        </div>

        <StarRating value={rating} />

        <p className="whitespace-pre-line text-sm text-muted-foreground">{body}</p>

        {actions.length > 0 ? (
          <div className="flex justify-end border-t pt-3">
            <ResourceActions actions={actions} />
          </div>
        ) : null}
      </CardContent>
    </Card>
  )
}

export default ReviewCard
