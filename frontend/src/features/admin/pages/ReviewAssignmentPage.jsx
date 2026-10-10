import { useParams, Link } from 'react-router-dom'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useIdeaById, useScreeningDecision } from '@/features/ideas/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { formatDate, sanitizeText } from '@/lib/format'

/** Single screening assignment (spec area: Review). */
export default function ReviewAssignmentPage() {
  const { ideaId } = useParams()
  useDocumentTitle('Review assignment')
  const { data: idea, isLoading, isError, refetch } = useIdeaById(ideaId)
  const decision = useScreeningDecision(ideaId)

  const submit = (value) => decision.mutate({ decision: value })

  if (isLoading) return <PageSkeleton lines={4} />
  if (isError || !idea) return <ErrorState onRetry={refetch} />

  return (
    <div className="space-y-6">
      <PageHeader title="Review assignment" description={idea.title ?? 'Screen this idea.'} />

      <Card>
        <CardContent className="space-y-3 p-5">
          <div className="flex flex-wrap items-center gap-2">
            <h2 className="font-semibold">{idea.title}</h2>
            <StatusBadge status={idea.status} />
          </div>
          {idea.shortSummary ? (
            <p className="text-sm text-muted-foreground">{sanitizeText(idea.shortSummary)}</p>
          ) : null}
          {idea.createdAt ? (
            <p className="text-xs text-muted-foreground">
              Submitted {formatDate(idea.createdAt)}
            </p>
          ) : null}
        </CardContent>
      </Card>

      <div className="flex flex-wrap gap-2">
        {idea.slug ? (
          <Button asChild variant="ghost">
            <Link to={`/ideas/${idea.slug}`}>View idea</Link>
          </Button>
        ) : null}
        <Button
          variant="outline"
          onClick={() => submit('RequestRevision')}
          disabled={decision.isPending}
        >
          Request revision
        </Button>
        <Button variant="outline" onClick={() => submit('Reject')} disabled={decision.isPending}>
          Reject
        </Button>
        <Button onClick={() => submit('Approve')} disabled={decision.isPending}>
          Approve
        </Button>
      </div>
    </div>
  )
}