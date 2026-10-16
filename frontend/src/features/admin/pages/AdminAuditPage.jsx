import { BookOpen } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { adminApi } from '@/features/admin/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'
import { formatDateTime } from '@/lib/format'

export default function AdminAuditPage() {
  useDocumentTitle('Audit log')

  return (
    <div className="space-y-6">
      <PageHeader title="Audit log" description="A tamper-evident record of administrative actions (server-side pagination and search)." />

      <ServerDataTable
        queryKey={['admin', 'audit-logs']}
        queryFn={(params) => adminApi.auditLogs(params)}
        pageSize={DEFAULT_PAGE_SIZE}
        searchLabel="Search audit log"
        searchable
        emptyIcon={BookOpen}
        emptyTitle="No audit events"
        emptyDescription="Administrative actions will be recorded here."
        columns={[
          {
            field: 'createdAt',
            title: 'Time',
            headerSort: true,
            minWidth: 170,
            formatter: (cell) => formatDateTime(cell.getValue()),
          },
          {
            field: 'performedBy',
            title: 'User',
            headerSort: false,
            minWidth: 140,
            formatter: (cell) => {
              const value = cell.getValue()
              if (!value) return '—'
              return typeof value === 'object' ? (value.displayName ?? value.name ?? value.id) : value
            },
          },
          { field: 'action', title: 'Action', headerSort: true },
          {
            field: 'entity',
            title: 'Target',
            headerSort: false,
            formatter: (cell) => {
              const data = cell.getData()
              const text = data?.entity ?? data?.performedBy?.entity
              const suffix = data?.entityId ? ` #${data.entityId}` : ''
              return text ? `${text}${suffix}` : '—'
            },
          },
          {
            field: 'details',
            title: 'Details',
            headerSort: false,
            widthGrow: 2,
            formatter: (cell) => {
              const value = cell.getValue()
              if (typeof value === 'string') return value || '—'
              return value?.status ?? '—'
            },
          },
        ]}
      />
    </div>
  )
}