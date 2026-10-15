import { useState } from 'react'
import { Plus } from 'lucide-react'
import { z } from 'zod'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useCreateTag, useUpdateTag, useDeleteTag } from '@/features/admin/hooks'
import { adminApi } from '@/features/admin/api'
import { ListingLayout } from '@/components/shared/ListingLayout'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { FormField } from '@/components/ui/form-field'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'
import { formatDate } from '@/lib/format'
import { requiredText, slugField } from '@/features/_shared/validation/commonSchemas'
import { ResourceTable, ResourceFormDialog } from '@/features/_shared'

const tagSchema = z.object({
  name: requiredText(1, 120, 'Name'),
  slug: slugField(120),
})

export default function AdminTagsPage() {
  useDocumentTitle('Tags')
  const [dialogTag, setDialogTag] = useState(null)
  const [deleteTag, setDeleteTag] = useState(null)
  const create = useCreateTag()
  const update = useUpdateTag()
  const deleteTagMutation = useDeleteTag()
  const isEdit = Boolean(dialogTag && dialogTag.id)

  const onSubmit = async (values) => {
    if (isEdit) {
      await update.mutateAsync({ id: dialogTag.id, payload: values })
    } else {
      await create.mutateAsync(values)
    }
  }

  return (
    <div className="space-y-6">
      <ListingLayout
        title="Tags"
        description="Manage tags used across ideas (server-side pagination, sorting and search)."
        createAction={
          <Button size="sm" onClick={() => setDialogTag({ name: '', slug: '' })}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            New tag
          </Button>
        }
      >

      <ResourceTable
        queryKey={['admin', 'tags']}
        queryFn={(params) => adminApi.tags(params)}
        pageSize={DEFAULT_PAGE_SIZE}
        searchLabel="Search tags"
        emptyIcon={Plus}
        emptyTitle="No tags found"
        emptyDescription="Create your first tag or try a different search."
        columns={[
          { field: 'name', title: 'Name', headerSort: true },
          { field: 'slug', title: 'Slug', headerSort: true },
          { field: 'usageCount', title: 'Ideas', headerSort: true, hozAlign: 'right', formatter: (cell) => cell.getValue() ?? 0 },
          { field: 'createdAt', title: 'Created', headerSort: true, formatter: (cell) => formatDate(cell.getValue(), { short: true }) },
        ]}
        actions={[
          { key: 'edit', label: 'Edit' },
          { key: 'delete', label: 'Delete', variant: 'destructive' },
        ]}
        onAction={(action, row) => {
          if (action === 'edit') setDialogTag(row)
          if (action === 'delete') setDeleteTag(row)
        }}
      />
      </ListingLayout>

      {dialogTag ? (
        <ResourceFormDialog
          open
          onClose={() => setDialogTag(null)}
          title={isEdit ? 'Edit tag' : 'Create tag'}
          schema={tagSchema}
          defaultValues={{
            name: dialogTag?.name ?? '',
            slug: dialogTag?.slug ?? '',
          }}
          onSubmit={onSubmit}
          submitLabel={isEdit ? 'Save' : 'Create'}
          pendingLabel={isEdit ? 'Saving…' : 'Creating…'}
        >
          {(form) => (
            <>
              <FormField id="tag-name" label="Name" required error={form.formState.errors.name?.message}>
                <Input id="tag-name" {...form.register('name')} placeholder="e.g., Artificial Intelligence" />
              </FormField>
              <FormField id="tag-slug" label="Slug (optional)" hint="Auto-generated from name if empty" error={form.formState.errors.slug?.message}>
                <Input id="tag-slug" {...form.register('slug')} placeholder="e.g., artificial-intelligence" />
              </FormField>
            </>
          )}
        </ResourceFormDialog>
      ) : null}

      <ConfirmDialog
        open={Boolean(deleteTag)}
        onOpenChange={(open) => !open && setDeleteTag(null)}
        title="Delete tag?"
        description={deleteTag ? `This will permanently delete "${deleteTag.name}". Tags in use cannot be deleted.` : ''}
        confirmLabel="Delete"
        variant="destructive"
        onConfirm={() => deleteTag && deleteTagMutation.mutate(deleteTag.id)}
      />
    </div>
  )
}