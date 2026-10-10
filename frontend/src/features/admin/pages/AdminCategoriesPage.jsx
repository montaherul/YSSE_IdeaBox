import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { FolderTree, Plus } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { categorySchema, toErrorMessage } from '@/lib/validation'
import { useCategories, useCreateCategory, useUpdateCategory, useDeleteCategory } from '@/features/categories/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { CardSkeleton } from '@/components/shared/LoadingSkeleton'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { FormField } from '@/components/ui/form-field'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

function CategoryDialog({ category, onClose }) {
  const isEdit = Boolean(category)
  const create = useCreateCategory()
  const update = useUpdateCategory()

  const form = useForm({
    resolver: zodResolver(categorySchema),
    defaultValues: {
      name: category?.name ?? '',
      slug: category?.slug ?? '',
      description: category?.description ?? '',
      isActive: category?.isActive ?? true,
    },
  })

  const onSubmit = async (values) => {
    try {
      if (isEdit) await update.mutateAsync({ id: category.id, payload: values })
      else await create.mutateAsync(values)
      onClose()
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle id="category-dialog-title">{isEdit ? 'Edit category' : 'New category'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
          {form.formState.errors.root ? (
            <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{form.formState.errors.root.message}</p>
          ) : null}
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
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
            <Button type="submit" disabled={create.isPending || update.isPending}>{isEdit ? 'Save' : 'Create'}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
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
      <PageHeader
        title="Categories"
        description="Organise ideas into categories."
        actions={
          <Button size="sm" onClick={() => setDialog(null)}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            New category
          </Button>
        }
      />

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