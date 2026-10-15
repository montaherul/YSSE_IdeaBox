import { useState } from 'react'
import { FolderTree, Plus } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { categorySchema } from '@/lib/validation'
import { useCategories, useCreateCategory, useUpdateCategory, useDeleteCategory } from '@/features/categories/hooks'
import { ResourceFormDialog } from '@/features/_shared'
import { ListingLayout } from '@/components/shared/ListingLayout'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { CardSkeleton } from '@/components/shared/LoadingSkeleton'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { FormField } from '@/components/ui/form-field'

function CategoryDialog({ category, onClose }) {
  const isEdit = Boolean(category)
  const create = useCreateCategory()
  const update = useUpdateCategory()

  const onSubmit = async (values) => {
    if (isEdit) await update.mutateAsync({ id: category.id, payload: values })
    else await create.mutateAsync(values)
  }

  return (
    <ResourceFormDialog
      open
      onClose={onClose}
      title={isEdit ? 'Edit category' : 'New category'}
      schema={categorySchema}
      defaultValues={{
        name: category?.name ?? '',
        slug: category?.slug ?? '',
        description: category?.description ?? '',
        isActive: category?.isActive ?? true,
      }}
      onSubmit={onSubmit}
      submitLabel={isEdit ? 'Save' : 'Create'}
      pendingLabel={isEdit ? 'Saving…' : 'Creating…'}
    >
      {(form) => (
        <>
          <FormField id="cat-name" label="Name" required error={form.formState.errors.name?.message}>
            <Input id="cat-name" {...form.register('name')} />
          </FormField>
          <FormField id="cat-slug" label="Slug (optional)" error={form.formState.errors.slug?.message}>
            <Input id="cat-slug" {...form.register('slug')} />
          </FormField>
          <FormField id="cat-desc" label="Description (optional)" error={form.formState.errors.description?.message}>
            <Input id="cat-desc" {...form.register('description')} />
          </FormField>
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" {...form.register('isActive')} className="h-4 w-4" aria-label="Active category" />
            Active category
          </label>
        </>
      )}
    </ResourceFormDialog>
  )
}

export default function AdminCategoriesPage() {
  useDocumentTitle('Categories')
  const [dialog, setDialog] = useState(null)
  const [deleting, setDeleting] = useState(null)
  const { data, isLoading, isError, refetch } = useCategories()
  const remove = useDeleteCategory()

  const items = data?.items ?? data ?? []

  return (
    <div className="space-y-6">
      <ListingLayout
        title="Categories"
        description="Organise ideas into categories."
        createAction={
          <Button size="sm" onClick={() => setDialog(null)}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            New category
          </Button>
        }
      >

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2">
          <CardSkeleton />
          <CardSkeleton />
        </div>
      ) : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState icon={FolderTree} title="No categories" description="Create the first category to organise ideas." />
      ) : null}

      {items.length > 0 ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((category) => (
            <Card key={category.id ?? category.categoryId}>
              <CardContent className="space-y-2 p-5">
                <div className="flex items-center justify-between gap-2">
                  <h2 className="font-semibold">{category.name}</h2>
                  <StatusBadge status={category.isActive ? 'True' : 'False'} label={category.isActive ? 'Active' : 'Inactive'} />
                </div>
                {category.description ? <p className="text-sm text-muted-foreground">{category.description}</p> : null}
                <div className="flex gap-2 pt-2">
                  <Button size="sm" variant="outline" onClick={() => setDialog(category)}>Edit</Button>
                  <Button size="sm" variant="ghost" onClick={() => setDeleting(category)}>Delete</Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : null}

      </ListingLayout>

      {dialog !== undefined ? <CategoryDialog category={dialog} onClose={() => setDialog(undefined)} /> : null}

      <ConfirmDialog
        open={Boolean(deleting)}
        onOpenChange={(open) => !open && setDeleting(null)}
        title="Delete category?"
        description={deleting ? `This removes "${deleting.name}". Ideas keep their data.` : ''}
        confirmLabel="Delete"
        onConfirm={() => deleting && remove.mutate(deleting.id ?? deleting.categoryId)}
      />
    </div>
  )
}