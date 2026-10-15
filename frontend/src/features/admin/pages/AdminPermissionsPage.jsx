import { useState } from 'react'
import { Plus, Key } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { adminApi } from '@/features/admin/api'
import { useCreatePermission, useUpdatePermission, useDeletePermission } from '@/features/admin/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'
import { paginateCollection } from '@/lib/table'

function PermissionDialog({ permission, onClose }) {
  const isEdit = Boolean(permission)
  const create = useCreatePermission()
  const update = useUpdatePermission()
  const [code, setCode] = useState(permission?.code ?? '')
  const [description, setDescription] = useState(permission?.description ?? '')
  const [error, setError] = useState('')

  const submit = async (e) => {
    e.preventDefault()
    if (!code.trim()) {
      setError('Permission code is required.')
      return
    }
    try {
      if (isEdit) {
        await update.mutateAsync({ id: permission.id, payload: { code: code.trim(), description: description.trim() || undefined } })
      } else {
        await create.mutateAsync({ code: code.trim(), description: description.trim() || undefined })
      }
      onClose()
    } catch (err) {
      setError(err?.message ?? 'Could not save permission.')
    }
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle id="permission-dialog-title">{isEdit ? 'Edit permission' : 'Create permission'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error ? <p role="alert" className="text-sm text-destructive">{error}</p> : null}
          <FormField id="permission-code" label="Code" required error={error}>
            <Input
              id="permission-code"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              placeholder="e.g., ideas.create"
              disabled={isEdit}
            />
            <p className="text-xs text-muted-foreground">Format: resource.action (e.g., ideas.create, users.read)</p>
          </FormField>
          <FormField id="permission-description" label="Description (optional)">
            <Textarea id="permission-description" rows={3} value={description} onChange={(e) => setDescription(e.target.value)} placeholder="What this permission allows" />
          </FormField>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
            <Button type="submit" disabled={create.isPending || update.isPending}>
              {isEdit ? (update.isPending ? 'Saving…' : 'Save') : create.isPending ? 'Creating…' : 'Create'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export default function AdminPermissionsPage() {
  useDocumentTitle('Permissions')
  const [dialogPermission, setDialogPermission] = useState(null)
  const [deletePermission, setDeletePermission] = useState(null)

  const deletePermissionMutation = useDeletePermission()

  return (
    <div className="space-y-6">
      <PageHeader
        title="Permissions"
        description="Manage fine-grained permissions and assign them to roles."
        actions={
          <Button onClick={() => setDialogPermission({})}>
            <Plus className="mr-2 h-4 w-4" aria-hidden="true" />
            New permission
          </Button>
        }
      />

      <ServerDataTable
        queryKey={['admin', 'permissions']}
        queryFn={async (params) => paginateCollection(await adminApi.permissions({}), params, {
          searchFields: ['code', 'description'],
        })}
        pageSize={DEFAULT_PAGE_SIZE}
        searchLabel="Search permissions"
        emptyIcon={Key}
        emptyTitle="No permissions found"
        emptyDescription="Create a permission to get started."
        columns={[
          {
            field: 'code',
            title: 'Code',
            headerSort: true,
            formatter: (cell) => (
              <span className="font-mono text-sm">{cell.getValue()}</span>
            ),
          },
          {
            field: 'description',
            title: 'Description',
            headerSort: false,
            widthGrow: 2,
            formatter: (cell) => cell.getValue() ?? 'No description',
          },
          {
            field: 'roleCount',
            title: 'Roles',
            headerSort: true,
            width: 80,
            hozAlign: 'center',
            formatter: (cell) => cell.getValue() ?? 0,
          },
        ]}
        actions={[
          { key: 'edit', label: 'Edit' },
          { key: 'delete', label: 'Delete', variant: 'destructive' },
        ]}
        onAction={(action, row) => {
          if (action === 'edit') setDialogPermission(row)
          if (action === 'delete') setDeletePermission(row)
        }}
      />

      {dialogPermission !== null ? (
        <PermissionDialog
          key={dialogPermission?.id ?? 'new'}
          permission={dialogPermission || undefined}
          onClose={() => setDialogPermission(null)}
        />
      ) : null}

      <ConfirmDialog
        open={Boolean(deletePermission)}
        onOpenChange={(open) => !open && setDeletePermission(null)}
        title="Delete permission?"
        description={deletePermission ? `This will permanently delete "${deletePermission.code}". Permissions assigned to roles cannot be deleted.` : ''}
        confirmLabel="Delete"
        variant="destructive"
        onConfirm={() => deletePermission && deletePermissionMutation.mutate(deletePermission.id)}
      />
    </div>
  )
}
