import { Search } from 'lucide-react'
import { useResourceList } from '../hooks/useResourceList'
import { SearchInput } from '@/components/shared/SearchInput'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'
import { cn } from '@/lib/utils'

/**
 * Reusable server-paginated card grid.
 *
 * Replaces the duplicated "search + skeleton + empty + grid + pagination"
 * JSX that appears across ideas/problems/marketplace/categories pages.
 *
 * Props:
 * - queryKey/queryFn: passed to useResourceList (queryFn must return { items, total })
 * - renderCard(item, index): the card element
 * - getKey(item): optional stable key accessor
 * - searchable/searchLabel, emptyIcon/emptyTitle/emptyDescription
 * - toolbar, filters: optional nodes
 * - gridClassName: override the responsive grid classes
 */
export function ResourceCardGrid({
  queryKey,
  queryFn,
  renderCard,
  getKey,
  pageSize = DEFAULT_PAGE_SIZE,
  searchable = true,
  searchLabel = 'Search',
  emptyIcon = Search,
  emptyTitle = 'No results',
  emptyDescription = 'Try adjusting your search or filters.',
  toolbar,
  filters,
  gridClassName = 'grid gap-4 sm:grid-cols-2 lg:grid-cols-3',
  className,
  enabled = true,
}) {
  const {
    items,
    total,
    page,
    search,
    setSearch,
    setPage,
    isLoading,
    isError,
    refetch,
  } = useResourceList({ queryKey, queryFn, pageSize, enabled })

  return (
    <div className={cn('space-y-4', className)}>
      {(searchable || toolbar || filters) && (
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          {searchable ? (
            <div className="w-full sm:max-w-sm">
              <SearchInput
                id={`${queryKey?.join('-') ?? 'resource'}-search`}
                label={searchLabel}
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                onClear={() => setSearch('')}
              />
            </div>
          ) : (
            <div />
          )}
          {toolbar ? <div className="flex flex-wrap items-center gap-2">{toolbar}</div> : null}
        </div>
      )}

      {filters ? filters : null}

      {isLoading ? <ListSkeleton count={pageSize >= 6 ? 6 : pageSize} /> : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState icon={emptyIcon} title={emptyTitle} description={emptyDescription} />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <div className={gridClassName}>
          {items.map((item, index) => (
            <div key={getKey ? getKey(item) : (item.id ?? item.slug ?? index)}>
              {renderCard(item, index)}
            </div>
          ))}
        </div>
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <Pagination page={page} pageSize={pageSize} total={total} onChange={setPage} />
      ) : null}
    </div>
  )
}

export default ResourceCardGrid
