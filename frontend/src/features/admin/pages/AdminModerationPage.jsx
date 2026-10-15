import { useState } from 'react'
import { Flag, ShieldQuestion } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAdminReports, useResolveReport, useUpdateReport, useDeleteReport } from '@/features/admin/hooks'
import { ScreeningQueue } from '@/features/ideas/components/ScreeningQueue'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { formatDate } from '@/lib/format'

function ResolveDialog({ report, onClose }) {
  const resolve = useResolveReport()
  const [status, setStatus] = useState('Resolved')
  const [resolution, setResolution] = useState('')
  const [error, setError] = useState('')

  const submit = () => {
    if (!resolution.trim()) {
      setError('Add a short resolution note.')
      return
    }
    resolve.mutate(
      { id: report.id, payload: { status, resolution } },
      {
        onSuccess: () => onClose(),
        onError: (err) => setError(err?.message ?? 'Could not resolve the report.'),
      },
    )
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle id="resolve-report-title">Resolve report</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          {error ? <p role="alert" className="text-sm text-destructive">{error}</p> : null}
          <p className="text-sm text-muted-foreground">{report?.reason ?? ''}</p>
          <FormField id="resolve-status" label="Resolution status">
            <select id="resolve-status" className="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm" value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Resolved">Resolved</option>
              <option value="Dismissed">Dismissed</option>
              <option value="ActionTaken">Action taken</option>
            </select>
          </FormField>
          <FormField id="resolve-note" label="Resolution note">
            <Textarea id="resolve-note" rows={3} value={resolution} onChange={(e) => setResolution(e.target.value)} />
          </FormField>
        </div>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="button" onClick={submit} disabled={resolve.isPending}>Resolve report</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

function EditReportDialog({ report, onClose }) {
  const update = useUpdateReport()
  const [reason, setReason] = useState(report?.reason ?? '')
  const [details, setDetails] = useState(report?.details ?? '')
  const [status, setStatus] = useState(report?.status ?? 'Open')
  const [error, setError] = useState('')

  const submit = () => {
    if (!reason.trim()) {
      setError('A reason is required.')
      return
    }
    update.mutate(
      { id: report.id, payload: { reason, details: details || null, status } },
      {
        onSuccess: () => onClose(),
        onError: (err) => setError(err?.message ?? 'Could not update the report.'),
      },
    )
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle id="edit-report-title">Edit report</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          {error ? <p role="alert" className="text-sm text-destructive">{error}</p> : null}
          <FormField id="edit-report-reason" label="Reason">
            <Textarea id="edit-report-reason" rows={2} value={reason} onChange={(e) => setReason(e.target.value)} />
          </FormField>
          <FormField id="edit-report-details" label="Details">
            <Textarea id="edit-report-details" rows={3} value={details} onChange={(e) => setDetails(e.target.value)} />
          </FormField>
          <FormField id="edit-report-status" label="Status">
            <select id="edit-report-status" className="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm" value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Open">Open</option>
              <option value="Resolved">Resolved</option>
              <option value="Dismissed">Dismissed</option>
              <option value="ActionTaken">Action taken</option>
            </select>
          </FormField>
        </div>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="button" onClick={submit} disabled={update.isPending}>Save changes</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

export default function AdminModerationPage() {
  useDocumentTitle('Moderation')
  const [page] = useState(1)
  const [resolving, setResolving] = useState(null)
  const [editing, setEditing] = useState(null)
  const [deleting, setDeleting] = useState(null)
  const remove = useDeleteReport()
  const { data, isLoading, isError, refetch } = useAdminReports({ page, pageSize: 10 })

  const items = data?.items ?? []

  return (
    <div className="space-y-6">
      <PageHeader title="Moderation" description="Resolve user reports and screen submitted ideas." />

      <Tabs defaultValue="reports">
        <TabsList>
          <TabsTrigger value="reports">
            <Flag className="mr-1.5 h-4 w-4" aria-hidden="true" />
            Reports
          </TabsTrigger>
          <TabsTrigger value="screening">Screening queue</TabsTrigger>
        </TabsList>

        <TabsContent value="reports" className="pt-4">
          {isLoading ? <PageSkeleton lines={5} /> : null}
          {isError ? <ErrorState onRetry={refetch} /> : null}
          {!isLoading && !isError && items.length === 0 ? (
            <EmptyState icon={Flag} title="No open reports" description="Reported content will appear here." />
          ) : null}
          {items.length > 0 ? (
            <div className="space-y-3">
              {items.map((report) => (
                <Card key={report.id}>
                  <CardContent className="flex flex-wrap items-center justify-between gap-3 p-4">
                    <div className="min-w-0 flex-1">
                      <div className="flex flex-wrap items-center gap-2">
                        <StatusBadge status={report.status} />
                        <span className="text-sm font-medium">{report.targetType} #{ report.targetId}</span>
                      </div>
                      <p className="mt-1 text-sm text-muted-foreground">{report.reason}</p>
                      {report.details ? <p className="text-sm text-muted-foreground">{report.details}</p> : null}
                      <p className="text-xs text-muted-foreground">{formatDate(report.createdAt)}</p>
                    </div>
                    <div className="flex items-center gap-2">
                      <Button size="sm" variant="outline" onClick={() => setResolving(report)}>
                        Resolve
                      </Button>
                      <Button size="sm" variant="ghost" onClick={() => setEditing(report)}>
                        Edit
                      </Button>
                      <Button size="sm" variant="ghost" className="text-destructive" onClick={() => setDeleting(report)}>
                        Delete
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          ) : null}
        </TabsContent>

        <TabsContent value="screening" className="pt-4">
          <ScreeningQueue />
        </TabsContent>
      </Tabs>

      {resolving ? <ResolveDialog report={resolving} onClose={() => setResolving(null)} /> : null}
      {editing ? <EditReportDialog report={editing} onClose={() => setEditing(null)} /> : null}

      <ConfirmDialog
        open={Boolean(deleting)}
        onOpenChange={(open) => !open && setDeleting(null)}
        title="Delete report?"
        description="The report and its resolution will be permanently removed."
        confirmLabel="Delete"
        onConfirm={() => {
          if (deleting) {
            remove.mutate(deleting.id)
            setDeleting(null)
          }
        }}
      />
    </div>
  )
}