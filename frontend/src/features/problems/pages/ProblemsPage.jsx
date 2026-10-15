import { Link } from 'react-router-dom'
import { AlertTriangle, Plus, ThumbsUp, Sparkles } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { useEndorseProblem } from '@/features/problems/hooks'
import { problemsApi } from '@/features/problems/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { ResourceCardGrid } from '@/features/_shared'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { formatDate, formatCompact } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

export default function ProblemsPage() {
  useDocumentTitle('Problems')
  const { isAuthenticated } = useAuth()
  const endorse = useEndorseProblem()

  return (
    <div className="space-y-6">
      <PageHeader
        title="Problems"
        description="Real challenges shared by the community — propose ideas that solve them."
        actions={
          isAuthenticated ? (
            <Button asChild>
              <Link to="/problems/new">
                <Plus className="h-4 w-4" aria-hidden="true" />
                Report a problem
              </Link>
            </Button>
          ) : null
        }
      />

      <ResourceCardGrid
        queryKey={['problems', 'list']}
        queryFn={(params) => problemsApi.list(params)}
        getKey={(problem) => problem.id}
        pageSize={DEFAULT_PAGE_SIZE}
        searchable
        searchLabel="Search problems"
        emptyIcon={Sparkles}
        emptyTitle="No problems yet"
        emptyDescription="Be the first to describe a problem that needs a solution."
        renderCard={(problem) => (
          <Card className="flex h-full flex-col">
            <CardContent className="flex flex-1 flex-col gap-3 p-5">
              <div className="flex items-center justify-between gap-2">
                <StatusBadge status={problem.status} />
                {problem.urgency ? (
                  <span className="inline-flex items-center gap-1 text-xs text-muted-foreground">
                    <AlertTriangle className="h-3.5 w-3.5" aria-hidden="true" />
                    {problem.urgency}
                  </span>
                ) : null}
              </div>
              <h2 className="text-base font-semibold leading-snug">
                <Link to={`/problems/${problem.id}`} className="hover:text-primary">
                  {problem.title}
                </Link>
              </h2>
              <p className="line-clamp-3 text-sm text-muted-foreground">{problem.whyItMatters}</p>
              <div className="mt-auto flex items-center justify-between pt-2 text-xs text-muted-foreground">
                <span className="inline-flex items-center gap-1">
                  <ThumbsUp className="h-3.5 w-3.5" aria-hidden="true" />
                  {formatCompact(problem.endorsementCount ?? 0)} endorsements
                </span>
                <time dateTime={problem.createdAt}>{formatDate(problem.createdAt, { short: true })}</time>
              </div>
              {isAuthenticated ? (
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => endorse.mutate(problem.id)}
                  disabled={endorse.isPending}
                >
                  <ThumbsUp className="h-4 w-4" aria-hidden="true" />
                  Endorse
                </Button>
              ) : null}
            </CardContent>
          </Card>
        )}
      />
    </div>
  )
}
