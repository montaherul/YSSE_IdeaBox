import { PageHeader } from '@/components/shared/PageHeader'
import { cn } from '@/lib/utils'

/**
 * Generic shell for listing/management pages.
 *
 * - Renders a `PageHeader` with an optional `createAction` pinned top-left
 *   (before the title) and secondary `actions` on the right.
 * - The content area is full-width so server-side tables maximise
 *   the available space instead of sitting in a narrow column.
 *
 * Props: title, description, createAction, actions, toolbar, children, className.
 */
export function ListingLayout({ title, description, createAction, actions, toolbar, children, className }) {
  return (
    <div className={cn('flex w-full flex-col gap-4', className)}>
      <PageHeader title={title} description={description} leading={createAction} actions={actions} />
      {toolbar ? <div className="flex flex-wrap items-center gap-3">{toolbar}</div> : null}
      <div className="min-w-0 w-full">{children}</div>
    </div>
  )
}

export default ListingLayout
