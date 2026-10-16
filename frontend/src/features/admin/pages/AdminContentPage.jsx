import { useState } from 'react'
import { FileText, Plus } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useUpdateStaticContent, useCreateStaticContent, useDeleteStaticContent } from '@/features/static/hooks'
import { staticApi } from '@/features/static/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { RichTextEditor } from '@/components/ui/rich-text-editor'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { FormField } from '@/components/ui/form-field'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'
import { formatDateTime } from '@/lib/format'

function ContentDialog({ content, onClose }) {
  const isEdit = Boolean(content)
  const create = useCreateStaticContent()
  const update = useUpdateStaticContent()
  const [key, setKey] = useState(content?.key ?? '')
  const [title, setTitle] = useState(content?.title ?? '')
  const [html, setHtml] = useState(content?.content ?? '')
  const [isActive, setIsActive] = useState(content?.isActive ?? true)
  const [error, setError] = useState('')

  const submit = async (e) => {
    e.preventDefault()
    if (!key.trim() || !title.trim() || !html.trim()) {
      setError('Key, title and content are required.')
      return
    }
    const payload = { key: key.trim(), title: title.trim(), content: html, isActive }
    try {
      if (isEdit) {
        await update.mutateAsync({ id: content.id, payload })
      } else {
        await create.mutateAsync(payload)
      }
      onClose()
    } catch (err) {
      setError(err?.message ?? 'Could not save content.')
    }
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle id="content-dialog-title">{isEdit ? `Edit "${content.key}"` : 'Create content'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error ? <p role="alert" className="text-sm text-destructive">{error}</p> : null}
          <div className="grid gap-4 sm:grid-cols-2">
            <FormField id="content-key" label="Key" required hint="Unique identifier, e.g. about-intro">
              <Input id="content-key" value={key} onChange={(e) => setKey(e.target.value)} placeholder="e.g., about-intro" />
            </FormField>
            <FormField id="content-title" label="Title" required>
              <Input id="content-title" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="e.g., About YSSE" />
            </FormField>
          </div>
          <FormField id="content-body" label="Body" required>
            <RichTextEditor value={html} onChange={setHtml} placeholder="Write the content here…" />
          </FormField>
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} className="h-4 w-4" aria-label="Active content" />
            Published
          </label>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
            <Button type="submit" disabled={create.isPending || update.isPending}>
              {isEdit ? 'Save' : 'Create'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export default function AdminContentPage() {
  useDocumentTitle('Site content')
  const [dialog, setDialog] = useState(null)
  const [deleting, setDeleting] = useState(null)
  const deleteContent = useDeleteStaticContent()

  return (
    <div className="space-y-6">
      <PageHeader
        title="Site content"
        description="Edit static pages and blocks. Content is sanitized before rendering (server-side permissions still apply)."
        actions={
          <Button size="sm" onClick={() => setDialog({})}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            New content
          </Button>
        }
      />

      <ServerDataTable
        queryKey={['admin', 'static-content']}
        queryFn={(params) => staticApi.getAll(params)}
        pageSize={DEFAULT_PAGE_SIZE}
        searchLabel="Search content"
        emptyIcon={FileText}
        emptyTitle="No content yet"
        emptyDescription="Create content such as the About or Terms page to replace hardcoded copy."
        columns={[
          { field: 'key', title: 'Key', headerSort: true },
          { field: 'title', title: 'Title', headerSort: true, widthGrow: 2 },
          { field: 'isActive', title: 'Status', headerSort: false, formatter: (cell) => {
              const active = cell.getValue()
              return (
                <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${active ? 'bg-primary/10 text-primary' : 'bg-muted text-muted-foreground'}`}>
                  {active ? 'Published' : 'Hidden'}
                </span>
              )
            } },
          { field: 'updatedAt', title: 'Updated', headerSort: true, formatter: (cell) => (cell.getValue() ? formatDateTime(cell.getValue()) : '—') },
        ]}
        actions={[
          { key: 'edit', label: 'Edit' },
          { key: 'delete', label: 'Delete', variant: 'destructive' },
        ]}
        onAction={(action, row) => {
          if (action === 'edit') setDialog(row)
          if (action === 'delete') setDeleting(row)
        }}
      />

      {dialog ? <ContentDialog key={dialog.id ?? 'new'} content={dialog} onClose={() => setDialog(null)} /> : null}

      <ConfirmDialog
        open={Boolean(deleting)}
        onOpenChange={(open) => !open && setDeleting(null)}
        title="Delete content?"
        description={deleting ? `This permanently removes "${deleting.key}". Public pages will fall back to their hardcoded content.` : ''}
        confirmLabel="Delete"
        variant="destructive"
        onConfirm={() => deleting && deleteContent.mutate(deleting.id)}
      />
    </div>
  )
}