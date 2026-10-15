import { useEffect, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ArrowDown, ArrowUp, ChevronsUpDown, Search } from 'lucide-react'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { Input } from '@/components/ui/input'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'
import { cn } from '@/lib/utils'

/**
 * Generic server-side listing table (no external table library).
 *
 * Pagination, sorting and search execute on the server; only the current page
 * of rows is held in memory, so large admin datasets never load into the browser.
 *
 * Props:
 * - queryKey:  array prefix for the TanStack Query cache key
 * - queryFn:   async (params) => Promise<{ items, total }>
 * - columns:   column definitions ({ field, title, headerSort?, hozAlign?, width?, minWidth?, formatter? })
 *              `formatter(cell)` receives a lightweight cell shim with getValue(), getData() and getRow().getData().
 * - actions:   optional array of { key, label, variant?, visible?(row) } -> adds an Actions column
 * - onAction:  (key, row) => void
 * - onRowClick: (row, event) => void; ignored when the click lands on an action button
 * - searchable, searchLabel, placeholderMessage (all optional)
 * - toolbar:   optional node rendered in the header row (e.g. bulk actions)
 * - height:    optional fixed table height (number of px) -> scrollable body
 */
export function ServerDataTable({
  queryKey,
  queryFn,
  columns,
  actions,
  onAction,
  onRowClick,
  toolbar,
  pageSize = DEFAULT_PAGE_SIZE,
  searchable = true,
  searchLabel = 'Search',
  emptyIcon = Search,
  emptyTitle = 'No records found',
  emptyDescription = 'Try adjusting the search or filters.',
  height,
  className,
}) {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [sort, setSort] = useState({ field: null, direction: null })

  const searchId = `${queryKey.join('-')}-search`

  useEffect(() => {
    const timeout = setTimeout(() => setDebouncedSearch(search), 300)
    return () => clearTimeout(timeout)
  }, [search])

  const params = {
    page,
    pageSize,
    search: debouncedSearch.trim() || undefined,
    sort: sort.field ?? undefined,
    direction: sort.direction ?? undefined,
  }

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: [...queryKey, params],
    queryFn: () => queryFn(params),
    placeholderData: (prev) => prev,
  })

  const items = Array.isArray(data?.items) ? data.items : []
  const total = data?.total ?? 0

  const toggleSort = (field) => {
    setSort((prev) => {
      if (prev.field !== field) return { field, direction: 'asc' }
      if (prev.direction === 'asc') return { field, direction: 'desc' }
      return { field: null, direction: null }
    })
    setPage(1)
  }

  const handleRowClick = (row, event) => {
    if (!onRowClick) return
    if (event?.target?.closest?.('[data-action]')) return
    onRowClick(row, event)
  }

  const hasActions = Array.isArray(actions) && actions.length > 0

  return (
    <div className={cn('space-y-4', className)}>
      <div className="overflow-hidden rounded-2xl border bg-card shadow-sm">
        {searchable || toolbar ? (
          <div className="flex flex-wrap items-center justify-between gap-3 border-b bg-muted/30 px-4 py-3">
            {searchable ? (
              <div className="relative w-full max-w-xs">
                <label htmlFor={searchId} className="sr-only">
                  {searchLabel}
                </label>
                <Search
                  className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
                  aria-hidden="true"
                />
                <Input
                  id={searchId}
                  value={search}
                  onChange={(event) => {
                    setSearch(event.target.value)
                    setPage(1)
                  }}
                  placeholder={searchLabel}
                  className="h-9 border-transparent bg-background pl-9 shadow-none focus-visible:border-input"
                />
              </div>
            ) : (
              <span />
            )}
            {toolbar ? <div className="flex flex-wrap items-center gap-2">{toolbar}</div> : null}
          </div>
        ) : null}

        {isLoading ? (
          <div className="p-4">
            <PageSkeleton lines={6} />
          </div>
        ) : null}

        {isError ? (
          <div className="p-4">
            <ErrorState onRetry={refetch} />
          </div>
        ) : null}

        {!isLoading && !isError && items.length === 0 ? (
          <div className="p-4">
            <EmptyState icon={emptyIcon} title={emptyTitle} description={emptyDescription} />
          </div>
        ) : null}

        {!isLoading && !isError && items.length > 0 ? (
          <div
            className="overflow-x-auto"
            style={height ? { maxHeight: height, overflowY: 'auto' } : undefined}
          >
            <table className="w-full border-collapse text-sm">
              <thead>
                <tr>
                  {columns.map((col) => {
                    const sortable = col.headerSort !== false
                    const isSorted = sort.field === col.field
                    return (
                      <th
                        key={col.field ?? col.title}
                        scope="col"
                        style={cellWidthStyle(col)}
                        aria-sort={
                          isSorted ? (sort.direction === 'asc' ? 'ascending' : 'descending') : 'none'
                        }
                        className={cn(
                          'sticky top-0 z-10 whitespace-nowrap border-b bg-muted/80 px-4 py-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground backdrop-blur-sm',
                          headerAlignClass(col.hozAlign),
                        )}
                      >
                        {sortable ? (
                          <button
                            type="button"
                            onClick={() => toggleSort(col.field)}
                            className={cn(
                              'group/sort inline-flex items-center gap-1.5 rounded font-semibold uppercase tracking-wider transition-colors',
                              isSorted ? 'text-foreground' : 'hover:text-foreground',
                              'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                            )}
                          >
                            {col.title}
                            {isSorted && sort.direction === 'asc' ? (
                              <ArrowUp className="h-3.5 w-3.5 text-primary" aria-hidden="true" />
                            ) : isSorted && sort.direction === 'desc' ? (
                              <ArrowDown className="h-3.5 w-3.5 text-primary" aria-hidden="true" />
                            ) : (
                              <ChevronsUpDown
                                className="h-3.5 w-3.5 opacity-40 transition-opacity group-hover/sort:opacity-80"
                                aria-hidden="true"
                              />
                            )}
                          </button>
                        ) : (
                          col.title
                        )}
                      </th>
                    )
                  })}
                  {hasActions ? (
                    <th
                      scope="col"
                      className="sticky top-0 z-10 whitespace-nowrap border-b bg-muted/80 px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-muted-foreground backdrop-blur-sm"
                    >
                      Actions
                    </th>
                  ) : null}
                </tr>
              </thead>
              <tbody>
                {items.map((row, index) => (
                  <tr
                    key={row?.id ?? row?.slug ?? row?.key ?? index}
                    onClick={(event) => handleRowClick(row, event)}
                    className={cn(
                      'border-b border-border/60 transition-colors last:border-b-0',
                      index % 2 === 1 && 'bg-muted/20',
                      'hover:bg-accent/60',
                      onRowClick && 'cursor-pointer',
                    )}
                  >
                    {columns.map((col) => (
                      <td
                        key={col.field ?? col.title}
                        style={cellWidthStyle(col)}
                        className={cn(
                          'px-4 py-3.5 align-middle',
                          col.hozAlign === 'right' && 'text-right',
                          col.hozAlign === 'center' && 'text-center',
                        )}
                      >
                        {renderCellValue(col, row)}
                      </td>
                    ))}
                    {hasActions ? (
                      <td className="px-4 py-3.5 text-right align-middle">
                        <div className="flex flex-wrap items-center justify-end gap-1.5">
                          {actions.map((action) => {
                            if (action.visible && !action.visible(row)) return null
                            return (
                              <button
                                key={action.key}
                                type="button"
                                data-action={action.key}
                                onClick={(event) => {
                                  event.stopPropagation()
                                  onAction?.(action.key, row)
                                }}
                                className={cn(
                                  'inline-flex h-7 items-center gap-1 rounded-full px-3 text-xs font-medium transition-colors',
                                  'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                                  action.variant === 'destructive'
                                    ? 'bg-destructive/10 text-destructive hover:bg-destructive/20'
                                    : 'bg-primary/10 text-primary hover:bg-primary/20',
                                )}
                              >
                                {action.label}
                              </button>
                            )
                          })}
                        </div>
                      </td>
                    ) : null}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : null}

        {items.length > 0 ? (
          <div className="px-4 pb-4">
            <Pagination page={page} pageSize={pageSize} total={total} onChange={setPage} />
          </div>
        ) : null}
      </div>
    </div>
  )
}

function headerAlignClass(hozAlign) {
  if (hozAlign === 'right') return 'text-right'
  if (hozAlign === 'center') return 'text-center'
  return 'text-left'
}

function cellWidthStyle(col) {
  const style = {}
  if (col.width) style.width = typeof col.width === 'number' ? `${col.width}px` : col.width
  if (col.minWidth) style.minWidth = typeof col.minWidth === 'number' ? `${col.minWidth}px` : col.minWidth
  return style
}

/** Renders a cell using the column formatter, emulating the previous table cell API. */
function renderCellValue(col, row) {
  const value = row?.[col.field]
  if (typeof col.formatter === 'function') {
    const cell = {
      getValue: () => value,
      getData: () => row,
      getRow: () => ({ getData: () => row }),
    }
    const result = col.formatter(cell)
    if (result === null || result === undefined || result === false) return ''
    return result
  }
  if (value === null || value === undefined) return ''
  return value
}

export default ServerDataTable
