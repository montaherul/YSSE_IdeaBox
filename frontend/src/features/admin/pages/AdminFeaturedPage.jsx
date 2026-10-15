import { Star } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { ideasApi } from '@/features/ideas/api'
import { useSetIdeaFeatured } from '@/features/ideas/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Badge } from '@/components/ui/badge'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'
import { formatCompact, formatDate } from '@/lib/format'

/** Admin curation of homepage "Featured ideas" (server-side listing + feature toggle). */
export default function AdminFeaturedPage() {
  useDocumentTitle('Featured ideas')
  const setFeatured = useSetIdeaFeatured()

  return (
    <div className="space-y-6">
      <PageHeader
        title="Featured ideas"
        description="Hand-pick published ideas to highlight on the homepage. Featuring never changes an idea's status or funding eligibility."
      />

      <ServerDataTable
        queryKey={['admin', 'featured-ideas']}
        queryFn={(params) => ideasApi.list({ ...params, status: 'Published' })}
        pageSize={DEFAULT_PAGE_SIZE}
        searchLabel="Search published ideas"
        emptyIcon={Star}
        emptyTitle="No published ideas"
        emptyDescription="Publish an idea first, then feature it here."
        columns={[
          {
            field: 'title',
            title: 'Idea',
            headerSort: false,
            formatter: (cell) => {
              const row = cell.getData()
              const wrap = document.createElement('div')
              wrap.className = 'min-w-0'
              const link = document.createElement('a')
              link.href = `/ideas/${row.slug ?? row.id}`
              link.textContent = row.title ?? ''
              link.className = 'font-medium hover:text-primary'
              const meta = document.createElement('div')
              meta.className = 'text-xs text-muted-foreground'
              meta.textContent = row.categoryName ?? ''
              wrap.append(link, meta)
              return wrap
            },
          },
          {
            field: 'isFeatured',
            title: 'Featured',
            width: 110,
            headerSort: false,
            formatter: (cell) =>
              cell.getValue() ? (
                <Badge variant="secondary">Featured</Badge>
              ) : (
                <span className="text-xs text-muted-foreground">—</span>
              ),
          },
          {
            field: 'voteCount',
            title: 'Votes',
            width: 90,
            headerSort: false,
            hozAlign: 'right',
            formatter: (cell) => formatCompact(cell.getValue() ?? 0),
          },
          {
            field: 'status',
            title: 'Status',
            width: 130,
            headerSort: false,
            formatter: (cell) => <StatusBadge status={cell.getValue()} />,
          },
          {
            field: 'publishedAt',
            title: 'Published',
            width: 150,
            headerSort: false,
            formatter: (cell) => formatDate(cell.getValue(), { short: true }),
          },
        ]}
        actions={[
          {
            key: 'feature',
            label: 'Feature',
            visible: (row) => !row.isFeatured,
          },
          {
            key: 'unfeature',
            label: 'Unfeature',
            variant: 'destructive',
            visible: (row) => Boolean(row.isFeatured),
          },
        ]}
        onAction={(action, row) =>
          setFeatured.mutate({ id: row.id, featured: action === 'feature' })
        }
      />
    </div>
  )
}
