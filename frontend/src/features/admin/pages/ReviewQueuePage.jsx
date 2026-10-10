import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { ScreeningQueue } from '@/features/ideas/components/ScreeningQueue'
import { PageHeader } from '@/components/shared/PageHeader'

/** Reviewer-facing screening queue (spec area: Review). */
export default function ReviewQueuePage() {
  useDocumentTitle('Review queue')
  return (
    <div className="space-y-6">
      <PageHeader
        title="Review queue"
        description="Ideas awaiting a screening decision. Approve, reject or request a revision."
      />
      <ScreeningQueue />
    </div>
  )
}