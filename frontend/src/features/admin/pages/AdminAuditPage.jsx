import { useState } from 'react'
import { BookOpen } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuditLogs } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { formatDateTime } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

export default function AdminAuditPage() {
  useDocumentTitle('Audit log')
  const [page, setPage] = useState(1)
  const { data, isLoading, isError, refetch } = useAuditLogs({ page, pageSize: DEFAULT_PAGE_SIZE })

  const items = data?.items ?? []

  return (
    <div className="space-y-6">
      <PageHeader title="Audit log" description="A tamper-evident record of administrative actions." />

      {isLoading ? <PageSkeleton lines={6} /> : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState icon={BookOpen} title="No audit events" description="Administrative actions will be recorded here." />
      ) : null}

      {items.length > 0 ? (
        <>
          <div className="overflow-x-auto rounded-xl border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Time</TableHead>
                  <TableHead>User</TableHead>
                  <TableHead>Action</TableHead>
                  <TableHead>Target</TableHead>
                  <TableHead>Details</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {items.map((log) => (
                  <TableRow key={log.id}>
                    <TableCell className="whitespace-nowrap text-xs">{formatDateTime(log.createdAt)}</TableCell>
                    <TableCell className="whitespace-nowrap">{log.performedBy?.displayName ?? log.performedByName ?? log.performedBy ?? '—'}</TableCell>
                    <TableCell className="whitespace-nowrap">{log.action}</TableCell>
                    <TableCell className="whitespace-nowrap">{log.entity ?? '—'}{log.entityId ? ` #${log.entityId}` : ''}</TableCell>
                    <TableCell className="max-w-xs truncate text-muted-foreground">{typeof log.details === 'string' ? log.details : log.status ?? '—'}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={data?.total ?? 0} onChange={setPage} />
        </>
      ) : null}
    </div>
  )
}