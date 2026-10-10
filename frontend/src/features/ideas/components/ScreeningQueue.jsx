import { useState } from 'react'
import { ShieldQuestion } from 'lucide-react'
import { useScreeningQueue, useScreeningDecision } from '../hooks'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'

function ScreeningIdeaRow({ idea }) {
  const decision = useScreeningDecision(idea.id)
  return (
    <Card>
      <CardContent className="flex flex-wrap items-center gap-3 p-4">
        <div className="min-w-0 flex-1">
          <p className="truncate font-medium">{idea.title}</p>
          <p className="truncate text-sm text-muted-foreground">{idea.shortSummary}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button
            size="sm"
            variant="outline"
            onClick={() => decision.mutate({ decision: 'RequestRevision' })}
            disabled={decision.isPending}
          >
            Request revision
          </Button>
          <Button
            size="sm"
            variant="outline"
            onClick={() => decision.mutate({ decision: 'Reject' })}
            disabled={decision.isPending}
          >
            Reject
          </Button>
          <Button
            size="sm"
            onClick={() => decision.mutate({ decision: 'Approve' })}
            disabled={decision.isPending}
          >
            Approve
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}

/** Shared screening queue used by the admin moderation page and the reviewer area. */
export function ScreeningQueue() {
  const [page] = useState(1)
  const { data, isLoading, isError, refetch } = useScreeningQueue({ page, pageSize: 10 })

  const items = data?.items ?? []

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
        <ScreeningIdeaRow key={idea.id} idea={idea} />
      ))}
    </div>
  )
}