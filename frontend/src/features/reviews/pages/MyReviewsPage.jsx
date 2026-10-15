import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { MessageSquare, Pencil, Trash2 } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useUpdateReview, useDeleteReview } from '@/features/reviews/hooks'
import { reviewsApi } from '@/features/reviews/api'
import { ResourceCardGrid } from '@/features/_shared'
import { ReviewCard } from '@/features/reviews/components/ReviewCard'
import { ReviewForm } from '@/features/reviews/components/ReviewForm'
import { PageHeader } from '@/components/shared/PageHeader'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'

export default function MyReviewsPage() {
  useDocumentTitle('My reviews')
  const queryClient = useQueryClient()
  const [editing, setEditing] = useState(null)
  const [pendingDelete, setPendingDelete] = useState(null)

  const update = useUpdateReview(editing?.ideaId)
  const remove = useDeleteReview(pendingDelete?.ideaId)

  const refreshMine = () => queryClient.invalidateQueries({ queryKey: ['reviews', 'mine'] })

  return (
    <div className="space-y-6">
      <PageHeader title="My reviews" description="Feedback you have shared with the community." />

      <ResourceCardGrid
        queryKey={['reviews', 'mine']}
        queryFn={(params) => reviewsApi.mine(params)}
        getKey={(review) => review.id}
        searchable={false}
        emptyIcon={MessageSquare}
        emptyTitle="You have not written any reviews"
        emptyDescription="Visit an idea page to share your feedback with the creator."
        gridClassName="grid gap-4 lg:grid-cols-2"
        renderCard={(review) => (
          <ReviewCard
            review={review}
            showAuthor={false}
            showIdea
            actions={[
              { key: 'edit', label: 'Edit', icon: Pencil, onClick: () => setEditing(review) },
              {
                key: 'delete',
                label: 'Delete',
                icon: Trash2,
                variant: 'ghost',
                onClick: () => setPendingDelete(review),
              },
            ]}
          />
        )}
      />

      {editing ? (
        <ReviewForm
          open
          title="Edit review"
          submitLabel="Save"
          pendingLabel="Saving…"
          defaultValues={{ body: editing.body ?? '', rating: editing.rating ?? null }}
          onClose={() => setEditing(null)}
          onSubmit={async (values) => {
            await update.mutateAsync({ id: editing.id, payload: values })
            refreshMine()
          }}
        />
      ) : null}

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onOpenChange={(open) => !open && setPendingDelete(null)}
        title="Delete this review?"
        description="Your review will be removed from the idea page."
        confirmLabel="Delete"
        onConfirm={async () => {
          if (!pendingDelete) return
          await remove.mutateAsync(pendingDelete.id)
          refreshMine()
          setPendingDelete(null)
        }}
      />
    </div>
  )
}
