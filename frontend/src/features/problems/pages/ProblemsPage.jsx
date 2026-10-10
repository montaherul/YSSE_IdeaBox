import { useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { AlertTriangle, Plus, ThumbsUp, Sparkles } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { useProblemList, useEndorseProblem } from '../hooks'
import { useDebounce } from '@/hooks/useDebounce'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { SearchInput } from '@/components/shared/SearchInput'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { formatDate, formatCompact } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

export default function ProblemsPage() {
  useDocumentTitle('Problems')
  const { isAuthenticated } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const [search, setSearch] = useState(searchParams.get('search') ?? '')
  const debounced = useDebounce(search, 300)
  const page = Number(searchParams.get('page')) || 1
  const endorse = useEndorseProblem()

  const { data, isLoading, isError, refetch } = useProblemList({
    search: debounced || undefined,
    page,
    pageSize: DEFAULT_PAGE_SIZE,
  })

  const items = data?.items ?? []
  const total = data?.total ?? 0

  const setPage = (next) => {
    const params = new URLSearchParams(searchParams)
    if (next <= 1) params.delete('page')
    else params.set('page', String(next))
    setSearchParams(params)
  }

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

      <SearchInput
        id="problem-search"
        label="Search problems"
        value={search}
        onChange={(e) => {
          setSearch(e.target.value)
          setPage(1)
        }}
        onClear={() => setSearch('')}
        placeholder="Search problems…"
      />

      {isLoading ? <ListSkeleton count={6} /> : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState
          icon={Sparkles}
          title="No problems yet"
          description="Be the first to describe a problem that needs a solution."
        />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((problem) => (
              <Card key={problem.id} className="flex flex-col">
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
            ))}
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={total} onChange={setPage} />
        </>
      ) : null}
    </div>
  )
}
