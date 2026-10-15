import { useDeferredValue, useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Search, X, Sparkles } from 'lucide-react'
import { useIdeaList } from '@/features/ideas/hooks'
import { useCategories } from '@/features/categories/hooks'
import { useDebounce } from '@/hooks/useDebounce'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { IdeaCard } from '@/features/ideas/components/IdeaCard'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { BoundaryNotice } from '@/components/shared/BoundaryNotice'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

const SORT_OPTIONS = [
  { value: 'newest', label: 'Newest first' },
  { value: 'mostVotes', label: 'Most votes' },
  { value: 'mostReviews', label: 'Most reviews' },
]

export default function DiscoverPage() {
  useDocumentTitle('Discover ideas')
  const [searchParams, setSearchParams] = useSearchParams()
  const [searchInput, setSearchInput] = useState(searchParams.get('q') ?? '')
  const deferredInput = useDeferredValue(searchInput)
  const debouncedSearch = useDebounce(deferredInput, 300)

  const categoryId = searchParams.get('category') || undefined
  const status = searchParams.get('status') || 'Published'
  const sort = searchParams.get('sort') || 'newest'
  const tag = searchParams.get('tag') || undefined
  const page = Number(searchParams.get('page')) || 1

  const { data: categories } = useCategories()
  const { data, isLoading, isError, error, refetch } = useIdeaList(
    {
      search: debouncedSearch || undefined,
      categoryId: categoryId ? Number(categoryId) : undefined,
      status,
      sort,
      tag,
      page,
      pageSize: DEFAULT_PAGE_SIZE,
    },
    true,
  )

  useEffect(() => {
    const next = new URLSearchParams(searchParams)
    if (debouncedSearch) next.set('q', debouncedSearch)
    else next.delete('q')
    next.delete('page')
    const nextString = next.toString()
    if (nextString !== searchParams.toString()) setSearchParams(next, { replace: true })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch])

  const setParam = (key, value) => {
    const next = new URLSearchParams(searchParams)
    if (value === undefined || value === null || value === '') next.delete(key)
    else next.set(key, String(value))
    if (key !== 'page') next.delete('page')
    setSearchParams(next)
  }

  const items = data?.items ?? data?.data ?? []
  const total = data?.total ?? data?.totalCount ?? 0

  return (
    <div className="space-y-6">
      <PageHeader
        title="Discover ideas"
        description="Browse ideas from the YSSE community. Search, filter and find something to support."
      />

      <div className="flex flex-col gap-4 rounded-xl border bg-card p-4 shadow-sm">
        <div className="flex flex-col gap-3 md:flex-row md:items-center">
          <div className="relative flex-1">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
            <Input
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              placeholder="Search ideas, keywords, tags…"
              aria-label="Search ideas"
              className="pl-9"
            />
          </div>

          <div className="flex flex-col gap-3 sm:flex-row">
            <Select value={categoryId ? String(categoryId) : 'all'} onValueChange={(v) => setParam('category', v === 'all' ? undefined : v)}>
              <SelectTrigger className="w-full sm:w-44" aria-label="Filter by category">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All categories</SelectItem>
                {(categories?.items ?? categories ?? []).map((c) => (
                  <SelectItem key={c.id ?? c.categoryId} value={String(c.id ?? c.categoryId)}>
                    {c.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select value={sort} onValueChange={(v) => setParam('sort', v)}>
              <SelectTrigger className="w-full sm:w-44" aria-label="Sort ideas">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {SORT_OPTIONS.map((o) => (
                  <SelectItem key={o.value} value={o.value}>
                    {o.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        {(tag || debouncedSearch) ? (
          <div className="flex flex-wrap items-center gap-2 text-sm">
            <span className="text-muted-foreground">Active filters:</span>
            {tag ? (
              <span className="inline-flex items-center gap-1 rounded-full bg-secondary px-2.5 py-0.5 text-xs font-medium">
                #{tag}
                <button
                  type="button"
                  aria-label="Remove tag filter"
                  className="rounded-full p-0.5 hover:bg-muted"
                  onClick={() => setParam('tag', undefined)}
                >
                  <X className="h-3 w-3" aria-hidden="true" />
                </button>
              </span>
            ) : null}
            {debouncedSearch ? (
              <span className="inline-flex items-center gap-1 rounded-full bg-secondary px-2.5 py-0.5 text-xs font-medium">
                "{debouncedSearch}"
                <button
                  type="button"
                  aria-label="Clear search"
                  className="rounded-full p-0.5 hover:bg-muted"
                  onClick={() => {
                    setSearchInput('')
                    setParam('q', undefined)
                  }}
                >
                  <X className="h-3 w-3" aria-hidden="true" />
                </button>
              </span>
            ) : null}
            <Button
              variant="ghost"
              size="sm"
              onClick={() => {
                setSearchInput('')
                setSearchParams({ status: 'Published', sort })
              }}
            >
              Clear all
            </Button>
          </div>
        ) : null}
      </div>

      {isLoading ? <ListSkeleton count={6} /> : null}

      {isError ? (
        <ErrorState onRetry={refetch} message={error?.message} />
      ) : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState
          icon={Sparkles}
          title="No ideas found"
          description="Try a different search term or clear your filters to see more ideas."
          action={
            <Button variant="outline" onClick={() => setSearchParams({ status: 'Published' })}>
              Clear filters
            </Button>
          }
        />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((idea) => (
              <IdeaCard key={idea.id ?? idea.slug} idea={idea} />
            ))}
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={total} onChange={(p) => setParam('page', p)} />
        </>
      ) : null}

      <BoundaryNotice />
    </div>
  )
}