import { useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { usePermission } from '@/hooks/usePermission'
import { useIdeaById, useScreeningDecision, useScreeningDecisions } from '@/features/ideas/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { TagList } from '@/components/shared/TagList'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Textarea } from '@/components/ui/textarea'
import { formatDate, sanitizeText } from '@/lib/format'

/** Single screening assignment (spec area: Review). */
export default function ReviewAssignmentPage() {
  const { ideaId } = useParams()
  useDocumentTitle('Review assignment')
  const { canPermission } = usePermission()
  const canDecide = canPermission('screening.decide')
  const { data: idea, isLoading, isError, refetch } = useIdeaById(ideaId)
  const { data: decisions } = useScreeningDecisions(ideaId)
  const decision = useScreeningDecision(ideaId)
  const [feedback, setFeedback] = useState('')

  const history = decisions?.items ?? (Array.isArray(decisions) ? decisions : [])
  const tags = Array.isArray(idea?.tags) ? idea.tags : []

  const submit = (value) => {
    const trimmed = feedback.trim()
    decision.mutate(trimmed ? { decision: value, feedback: trimmed } : { decision: value })
  }

  if (isLoading) return <PageSkeleton lines={4} />
  if (isError || !idea) return <ErrorState onRetry={refetch} />

  return (
    <div className="space-y-6">
      <PageHeader title="Review assignment" description={idea.title ?? 'Screen this idea.'} />

      <Card>
        <CardContent className="space-y-4 p-5 text-sm">
          <div className="flex flex-wrap items-center gap-2">
            <h2 className="font-semibold">{idea.title}</h2>
            <StatusBadge status={idea.status} />
            {idea.stage ? <span className="text-muted-foreground">{idea.stage}</span> : null}
            {idea.categoryName || idea.category?.name ? (
              <span className="text-muted-foreground">
                Category: {idea.categoryName ?? idea.category?.name}
              </span>
            ) : null}
          </div>

          {idea.shortSummary ? (
            <p className="text-muted-foreground">{sanitizeText(idea.shortSummary)}</p>
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

          {idea.createdAt ? (
            <p className="text-xs text-muted-foreground">Submitted {formatDate(idea.createdAt)}</p>
          ) : null}
        </CardContent>
      </Card>

      {history.length ? (
        <Card>
          <CardContent className="space-y-3 p-5 text-sm">
            <h3 className="font-semibold">Screening history</h3>
            <ul className="space-y-2">
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
          </CardContent>
        </Card>
      ) : null}

      {canDecide ? (
        <Card>
          <CardContent className="space-y-3 p-5">
            <label htmlFor="assignment-feedback" className="text-sm font-medium">
              Feedback (required for a revision request)
            </label>
            <Textarea
              id="assignment-feedback"
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
              <Button variant="outline" onClick={() => submit('Rejected')} disabled={decision.isPending}>
                Reject
              </Button>
              <Button onClick={() => submit('Published')} disabled={decision.isPending}>
                Approve
              </Button>
            </div>
          </CardContent>
        </Card>
      ) : null}

      <div className="flex flex-wrap gap-2">
        {idea.slug ? (
          <Button asChild variant="ghost">
            <Link to={`/ideas/${idea.slug}`}>View idea</Link>
          </Button>
        ) : null}
      </div>
    </div>
  )
}
