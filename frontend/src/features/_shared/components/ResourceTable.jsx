import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { cn } from '@/lib/utils'

/**
 * Enhanced server-side table: the existing `ServerDataTable`
 * (server pagination/sort/search) plus an optional filter bar and toolbar.
 *
 * Extra props over ServerDataTable:
 * - filters:  a node rendered above the table (e.g. <ResourceFilters>…</ResourceFilters>)
 * - toolbar:  a node rendered on the same row as the search box (e.g. a "New" button)
 */
export function ResourceTable({ filters, toolbar, className, ...tableProps }) {
  return (
    <div className={cn('space-y-4', className)}>
      {filters ? filters : null}
      {toolbar ? <div className="flex flex-wrap items-center justify-between gap-3">{toolbar}</div> : null}
      <ServerDataTable {...tableProps} />
    </div>
  )
}

export default ResourceTable
