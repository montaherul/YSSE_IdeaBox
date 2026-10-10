import { useState } from 'react'
import { Link } from 'react-router-dom'
import { MessageSquare } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useMyReviews, useDeleteReview } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { CardSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { formatDate } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

export default function MyReviewsPage() {
  useDocumentTitle('My reviews')
  const [page, setPage] = useState(1)
  const [pendingDelete, setPendingDelete] = useState(null)
  const { data, isLoading, isError, refetch } = useMyReviews({ page, pageSize: DEFAULT_PAGE_SIZE })
  const remove = useDeleteReview(pendingDelete?.ideaId)

  const items = data?.items ?? []
  const total = data?.total ?? 0

  return (
    <div className="space-y-6">
      <PageHeader title="My reviews" description="Feedback you have shared with the community." />

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
          title="You have not written any reviews"
          description="Visit an idea page to share your feedback with the creator."
        />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="space-y-4">
            {items.map((review) => (
              <Card key={review.id}>
                <CardContent className="space-y-3 p-5">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <div className="flex items-center gap-3 text-sm">
                      {review.ideaTitle ? (
                        <Link to={`/ideas/${review.ideaSlug ?? review.ideaId}`} className="font-medium text-primary hover:underline">
                          {review.ideaTitle}
                        </Link>
                      ) : (
                        <span className="font-medium">Idea #{review.ideaId}</span>
                      )}
                      <StatusBadge status={review.status} />
                    </div>
                    <span className="text-xs text-muted-foreground">{formatDate(review.createdAt, { short: true })}</span>
                  </div>
                  {review.rating ? (
                    <p aria-label={`Rating: ${review.rating} out of 5`} className="text-sm">
                      {'★'.repeat(review.rating)}
                      <span className="text-muted-foreground" aria-hidden="true">
                        {'★'.repeat(5 - review.rating)}
                      </span>
                    </p>
                  ) : null}
                  <p className="whitespace-pre-line text-sm text-muted-foreground">{review.body}</p>
                  <div className="flex justify-end">
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => setPendingDelete(review)}
                      disabled={remove.isPending}
                    >
                      Delete review
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={total} onChange={setPage} />
        </>
      ) : null}

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onOpenChange={(open) => !open && setPendingDelete(null)}
        title="Delete this review?"
        description="Your review will be removed from the idea page."
        confirmLabel="Delete"
        onConfirm={() => pendingDelete && remove.mutate(pendingDelete.id)}
      />
    </div>
  )
}