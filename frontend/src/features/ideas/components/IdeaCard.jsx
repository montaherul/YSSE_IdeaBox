import { Link } from 'react-router-dom'
import { Heart, MessageSquare, Eye } from 'lucide-react'
import { cn } from '@/lib/utils'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Tag } from '@/components/shared/Tag'
import { UserAvatar as SharedUserAvatar } from '@/components/shared/Avatar'
import { formatCompact, timeAgo, sanitizeText } from '@/lib/format'

const slugify = (value) =>
  String(value ?? '')
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')

/** Compact card for an idea in any listing (Discover, My Ideas, similar). */
export function IdeaCard({
  idea,
  to,
  onTagClick,
  showAuthor = true,
  className,
}) {
  const {
    id,
    slug,
    title,
    shortSummary,
    categoryName,
    categorySlug,
    status,
    stage,
    voteCount,
    reviewCount,
    viewCount,
    createdAt,
    author,
    tags = [],
  } = idea ?? {}

  const target = to ?? (slug ? `/ideas/${slug}` : null)
  const categoryHref = categorySlug ?? (categoryName ? slugify(categoryName) : null)

  return (
    <article
      className={cn(
        'flex flex-col gap-3 rounded-xl border bg-card p-5 shadow-sm transition-shadow hover:shadow-md',
        className,
      )}
    >
      <div className="flex items-center justify-between gap-2">
        <div className="flex min-w-0 items-center gap-2">
          {categoryName ? (
            categoryHref ? (
              <Link
                to={`/categories/${categoryHref}`}
                className="rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary transition-colors hover:bg-primary/20 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              >
                {categoryName}
              </Link>
            ) : (
              <span className="rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary">
                {categoryName}
              </span>
            )
          ) : null}
          {stage ? <span className="text-xs text-muted-foreground">{stage}</span> : null}
        </div>
        <StatusBadge status={status} />
      </div>

      <h3 className="text-base font-semibold leading-snug">
        {target ? (
          <Link to={target} className="line-clamp-2 rounded hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
            {title}
          </Link>
        ) : (
          <span className="line-clamp-2">{title}</span>
        )}
      </h3>

      {shortSummary ? (
        <p className="line-clamp-3 text-sm text-muted-foreground">{sanitizeText(shortSummary)}</p>
      ) : null}

      {tags.length > 0 ? (
        <div className="flex flex-wrap gap-1.5">
          {tags.slice(0, 4).map((tag) => (
            <Tag key={tag}>
              {onTagClick ? (
                <button
                  type="button"
                  className="rounded-full px-2.5 py-0.5 text-xs hover:bg-muted"
                  onClick={(e) => {
                    e.preventDefault()
                    onTagClick?.(tag)
                  }}
                >
                  #{tag}
                </button>
              ) : (
                <span className="text-xs">#{tag}</span>
              )}
            </Tag>
          ))}
        </div>
      ) : null}

      <div className="mt-auto flex items-center justify-between gap-2 pt-2">
        <div className="flex items-center gap-3 text-xs text-muted-foreground" aria-label="Idea statistics">
          <span className="inline-flex items-center gap-1">
            <Heart className="h-3.5 w-3.5" aria-hidden="true" />
            {formatCompact(voteCount ?? 0)}
          </span>
          <span className="inline-flex items-center gap-1">
            <MessageSquare className="h-3.5 w-3.5" aria-hidden="true" />
            {formatCompact(reviewCount ?? 0)}
          </span>
          <span className="inline-flex items-center gap-1">
            <Eye className="h-3.5 w-3.5" aria-hidden="true" />
            {formatCompact(viewCount ?? 0)}
          </span>
        </div>
        {createdAt ? <time className="text-xs text-muted-foreground" dateTime={createdAt}>{timeAgo(createdAt)}</time> : null}
      </div>

      {showAuthor && author ? (
        <div className="flex items-center gap-2 border-t pt-3 text-sm">
          {author.displayName || author.name ? (
            <SharedUserAvatar name={author.displayName || author.name} size="sm" />
          ) : null}
          <span className="truncate text-muted-foreground">
            {author.displayName || author.name || 'Unknown author'}
          </span>
        </div>
      ) : null}
    </article>
  )
}

export default IdeaCard