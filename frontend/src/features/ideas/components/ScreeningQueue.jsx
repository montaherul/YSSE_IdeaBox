import { useState } from 'react'
import { Eye, ShieldQuestion } from 'lucide-react'
import {
  useScreeningQueue,
  useScreeningDecision,
  useScreeningDecisions,
  useIdeaById,
} from '../hooks'
import { usePermission } from '@/hooks/usePermission'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { TagList } from '@/components/shared/TagList'
import { Pagination } from '@/components/shared/Pagination'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Textarea } from '@/components/ui/textarea'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { formatDate, sanitizeText } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

function decisionPayload(decision, feedback) {
  const trimmed = feedback?.trim()
  return trimmed ? { decision, feedback: trimmed } : { decision }
}

function IdeaReviewDialog({ ideaId, onClose }) {
  const { canPermission } = usePermission()
  const canDecide = canPermission('screening.decide')
  const { data: idea, isLoading, isError } = useIdeaById(ideaId)
  const { data: decisions } = useScreeningDecisions(ideaId)
  const decision = useScreeningDecision(ideaId)
  const [feedback, setFeedback] = useState('')

  const history = decisions?.items ?? (Array.isArray(decisions) ? decisions : [])
  const tags = Array.isArray(idea?.tags) ? idea.tags : []
  const attachments = Array.isArray(idea?.attachments) ? idea.attachments : []

  const submit = (value) => {
    decision.mutate(decisionPayload(value, feedback), { onSuccess: () => onClose() })
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-h-[85vh] max-w-2xl overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{idea?.title ?? 'Idea'}</DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <PageSkeleton lines={6} />
        ) : isError || !idea ? (
          <ErrorState message="This idea could not be loaded." />
        ) : (
          <div className="space-y-5 text-sm">
            <div className="flex flex-wrap items-center gap-3">
              <StatusBadge status={idea.status} />
              {idea.stage ? <span className="text-muted-foreground">{idea.stage}</span> : null}
              {idea.categoryName || idea.category?.name ? (
                <span className="text-muted-foreground">
                  Category: {idea.categoryName ?? idea.category?.name}
                </span>
              ) : null}
            </div>

            {idea.shortSummary ? (
              <section>
                <h3 className="font-semibold">Short summary</h3>
                <p className="mt-1 whitespace-pre-line text-muted-foreground">{sanitizeText(idea.shortSummary)}</p>
              </section>
            ) : null}

            {idea.problemStatement ? (
              <section>
                <h3 className="font-semibold">Problem statement</h3>
                <p className="mt-1 whitespace-pre-line text-muted-foreground">{sanitizeText(idea.problemStatement)}</p>
              </section>
            ) : null}

            {idea.proposedSolution ? (
              <section>
                <h3 className="font-semibold">Proposed solution</h3>
                <p className="mt-1 whitespace-pre-line text-muted-foreground">{sanitizeText(idea.proposedSolution)}</p>
              </section>
            ) : null}

            {idea.targetAudience ? (
              <section>
                <h3 className="font-semibold">Target audience</h3>
                <p className="mt-1 text-muted-foreground">{idea.targetAudience}</p>
              </section>
            ) : null}

            {idea.expectedImpact ? (
              <section>
                <h3 className="font-semibold">Expected impact</h3>
                <p className="mt-1 text-muted-foreground">{idea.expectedImpact}</p>
              </section>
            ) : null}

            {tags.length ? (
              <section>
                <h3 className="font-semibold">Tags</h3>
                <TagList className="mt-2" tags={tags} />
              </section>
            ) : null}

            {attachments.length ? (
              <section>
                <h3 className="font-semibold">Attachments</h3>
                <ul className="mt-1 space-y-1 text-muted-foreground">
                  {attachments.map((attachment) => (
                    <li key={attachment.id ?? attachment.fileName}>
                      {attachment.fileName ?? attachment.name}
                    </li>
                  ))}
                </ul>
              </section>
            ) : null}

            {history.length ? (
              <section>
                <h3 className="font-semibold">Screening history</h3>
                <ul className="mt-1 space-y-2">
                  {history.map((entry, index) => (
                    <li key={entry.id ?? index} className="rounded-md border p-3">
                      <div className="flex items-center justify-between gap-2">
                        <span className="font-medium">{entry.decision ?? entry.status ?? 'Decision'}</span>
                        {entry.createdAt ? (
                          <span className="text-xs text-muted-foreground">{formatDate(entry.createdAt)}</span>
                        ) : null}
                      </div>
                      {entry.feedback ? (
                        <p className="mt-1 whitespace-pre-line text-muted-foreground">{entry.feedback}</p>
                      ) : null}
                    </li>
                  ))}
                </ul>
              </section>
            ) : null}

            {canDecide ? (
              <section className="space-y-3 rounded-lg border p-4">
                <label htmlFor={`screening-feedback-${ideaId}`} className="font-medium">
                  Feedback (required for a revision request)
                </label>
                <Textarea
                  id={`screening-feedback-${ideaId}`}
                  rows={3}
                  value={feedback}
                  onChange={(event) => setFeedback(event.target.value)}
                  placeholder="Explain the decision so the creator knows what to change."
                />
                <div className="flex flex-wrap gap-2">
                  <Button
                    variant="outline"
                    onClick={() => submit('RevisionRequired')}
                    disabled={decision.isPending}
                  >
                    Request revision
                  </Button>
                  <Button
                    variant="outline"
                    onClick={() => submit('Rejected')}
                    disabled={decision.isPending}
                  >
                    Reject
                  </Button>
                  <Button onClick={() => submit('Published')} disabled={decision.isPending}>
                    Approve
                  </Button>
                </div>
              </section>
            ) : null}
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}

function ScreeningIdeaRow({ idea, onView }) {
  const { canPermission } = usePermission()
  const decision = useScreeningDecision(idea.id)
  const canDecide = canPermission('screening.decide')

  return (
    <Card>
      <CardContent className="flex flex-wrap items-center gap-3 p-4">
        <div className="min-w-0 flex-1">
          <p className="truncate font-medium">{idea.title}</p>
          <p className="truncate text-sm text-muted-foreground">{idea.shortSummary}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button size="sm" variant="ghost" onClick={() => onView(idea.id)}>
            <Eye className="h-4 w-4" aria-hidden="true" />
            View
          </Button>
          {canDecide ? (
            <>
              <Button
                size="sm"
                variant="outline"
                onClick={() => decision.mutate(decisionPayload('RevisionRequired'))}
                disabled={decision.isPending}
              >
                Request revision
              </Button>
              <Button
                size="sm"
                variant="outline"
                onClick={() => decision.mutate(decisionPayload('Rejected'))}
                disabled={decision.isPending}
              >
                Reject
              </Button>
              <Button
                size="sm"
                onClick={() => decision.mutate(decisionPayload('Published'))}
                disabled={decision.isPending}
              >
                Approve
              </Button>
            </>
          ) : null}
        </div>
      </CardContent>
    </Card>
  )
}

/** Shared screening queue used by the admin moderation page and the reviewer area. */
export function ScreeningQueue() {
  const [page, setPage] = useState(1)
  const [selectedId, setSelectedId] = useState(null)
  const { data, isLoading, isError, refetch } = useScreeningQueue({ page, pageSize: DEFAULT_PAGE_SIZE })

  const items = data?.items ?? []
  const currentPage = data?.page ?? page
  const total = data?.total ?? 0

  if (isLoading) return <PageSkeleton lines={5} />
  if (isError) return <ErrorState onRetry={refetch} />

  if (items.length === 0) {
    return (
      <EmptyState
        icon={ShieldQuestion}
        title="Screening queue is clear"
        description="No ideas are waiting for a screening decision."
      />
    )
  }

  return (
    <div className="space-y-3">
      {items.map((idea) => (
        <ScreeningIdeaRow key={idea.id} idea={idea} onView={setSelectedId} />
      ))}

      <Pagination
        page={currentPage}
        pageSize={data?.pageSize || DEFAULT_PAGE_SIZE}
        total={total}
        onChange={setPage}
      />

      {selectedId ? (
        <IdeaReviewDialog ideaId={selectedId} onClose={() => setSelectedId(null)} />
      ) : null}
    </div>
  )
}
