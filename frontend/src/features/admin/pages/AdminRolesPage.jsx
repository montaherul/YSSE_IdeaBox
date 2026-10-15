import { useState } from 'react'
import { Plus, Shield } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import {
  useCreateRole,
  useUpdateRole,
  useDeleteRole,
  useAdminRoleUsers,
} from '@/features/admin/hooks'
import { adminApi } from '@/features/admin/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { AsyncSelect } from '@/components/ui/async-select'
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

// Built-in roles created at startup. Their names are depended on by platform
// authorization logic, so they cannot be renamed or deleted (description and
// permissions remain editable). Administrators may create additional custom roles.
const SYSTEM_ROLES = [
  'Member',
  'Creator',
  'Entrepreneur',
  'Investor',
  'Mentor',
  'Moderator',
  'ScreeningReviewer',
  'Administrator',
]

const isSystemRole = (name) => SYSTEM_ROLES.includes(name)

const toPermissionOption = (permission) => ({
  value: permission.id,
  label: permission.code,
  description: permission.description,
})

// Server-side search/pagination for the permission picker so the catalog stays
// bounded even as it grows. Defined at module scope for a stable identity.
const loadPermissionOptions = async ({ search, page, pageSize }) => {
  const result = await adminApi.permissionsPaged({
    page,
    pageSize,
    ...(search ? { search } : {}),
  })
  return {
    options: (result.items ?? []).map(toPermissionOption),
    hasNextPage: result.hasNextPage,
  }
}

function RoleDialog({ role, onClose }) {
  const isEdit = Boolean(role)
  const create = useCreateRole()
  const update = useUpdateRole()
  const roleIsSystem = isSystemRole(role?.name)
  const [name, setName] = useState(role?.name ?? '')
  const [description, setDescription] = useState(role?.description ?? '')
  const [selectedPermissions, setSelectedPermissions] = useState(
    () => role?.permissions?.map(toPermissionOption) ?? [],
  )
  const [error, setError] = useState('')

  const submit = async (e) => {
    e.preventDefault()
    if (!name.trim()) {
      setError('Role name is required.')
      return
    }
    try {
      const payload = {
        name: name.trim(),
        description: description.trim() || undefined,
        permissionIds: selectedPermissions.map((option) => option.value),
      }
      if (isEdit) {
        await update.mutateAsync({ id: role.id, payload })
      } else {
        await create.mutateAsync(payload)
      }
      onClose()
    } catch (err) {
      setError(err?.message ?? 'Could not save role.')
    }
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle id="role-dialog-title">{isEdit ? 'Edit role' : 'Create role'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error ? <p role="alert" className="text-sm text-destructive">{error}</p> : null}
          <FormField
            id="role-name"
            label="Name"
            required
            hint={roleIsSystem ? 'Built-in role names cannot be changed.' : 'e.g. Content Curator'}
          >
            <Input
              id="role-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Role name"
              maxLength={100}
              disabled={roleIsSystem}
              required
            />
          </FormField>
          <FormField id="role-description" label="Description (optional)">
            <Textarea
              id="role-description"
              rows={3}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Optional description"
            />
          </FormField>
          <FormField
            id="role-permissions"
            label="Permissions"
            hint="Search and select the permissions this role grants."
          >
            <AsyncSelect
              id="role-permissions"
              multiple
              value={selectedPermissions}
              onChange={setSelectedPermissions}
              loadOptions={loadPermissionOptions}
              getOptionValue={(option) => option.value}
              getOptionLabel={(option) => option.label}
              placeholder="Select permissions"
              searchPlaceholder="Search permissions…"
              emptyMessage="No permissions found."
              renderOption={(option) => (
                <span className="flex w-full items-center gap-2">
                  <span className="font-mono text-sm font-medium">{option.label}</span>
                  {option.description ? (
                    <span className="ml-auto truncate text-xs text-muted-foreground">
                      {option.description}
                    </span>
                  ) : null}
                </span>
              )}
            />
          </FormField>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={create.isPending || update.isPending}>
              {isEdit ? (update.isPending ? 'Saving…' : 'Save') : create.isPending ? 'Creating…' : 'Create'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function RoleDetailsDialog({ role, onClose }) {
  const { data, isLoading } = useAdminRoleUsers(role.id, { page: 1, pageSize: 50 })
  const users = data?.items ?? []

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle id="role-details-title">{role.name}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <p className="mb-1 text-sm font-medium">Description</p>
            <p className="text-sm text-muted-foreground">{role.description || 'No description'}</p>
          </div>
          <div>
            <p className="mb-2 text-sm font-medium">Permissions ({role.permissions?.length ?? 0})</p>
            {role.permissions?.length ? (
              <div className="flex flex-wrap gap-1.5">
                {role.permissions.map((p) => (
                  <span key={p.id} className="rounded-full bg-secondary px-2 py-0.5 font-mono text-xs">
                    {p.code}
                  </span>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No permissions assigned.</p>
            )}
          </div>
          <div>
            <p className="mb-2 text-sm font-medium">Users ({role.userCount ?? 0})</p>
            {isLoading ? (
              <p className="text-sm text-muted-foreground">Loading…</p>
            ) : users.length === 0 ? (
              <p className="text-sm text-muted-foreground">No users with this role.</p>
            ) : (
              <ul className="space-y-1 text-sm">
                {users.map((u) => (
                  <li key={u.id} className="flex items-center justify-between gap-3">
                    <span className="truncate">{u.displayName || u.email || `#${u.id}`}</span>
                    <span className="truncate text-muted-foreground">{u.email}</span>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={onClose}>
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

export default function AdminRolesPage() {
  useDocumentTitle('Roles')
  const [dialogRole, setDialogRole] = useState(null)
  const [detailsRole, setDetailsRole] = useState(null)
  const [deleteRole, setDeleteRole] = useState(null)

  const deleteRoleMutation = useDeleteRole()

  return (
    <div className="space-y-6">
      <PageHeader
        title="Roles"
        description="Manage platform roles and their permissions."
        actions={
          <Button onClick={() => setDialogRole({})}>
            <Plus className="mr-2 h-4 w-4" aria-hidden="true" />
            New role
          </Button>
        }
      />

      <ServerDataTable
        queryKey={['admin', 'roles']}
        queryFn={async (params) => paginateCollection(await adminApi.roles({}), params, {
          searchFields: ['name', 'description'],
        })}
        pageSize={DEFAULT_PAGE_SIZE}
        searchLabel="Search roles"
        emptyIcon={Shield}
        emptyTitle="No roles found"
        emptyDescription="Create a role to get started."
        columns={[
          {
            field: 'name',
            title: 'Name',
            headerSort: true,
            formatter: (cell) => {
              const val = cell.getValue()
              const isSystem = isSystemRole(val)
              return (
                <>
                  <span className="truncate font-medium">{val}</span>
                  {isSystem && (
                    <span className="ml-2 inline-flex items-center rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">
                      System
                    </span>
                  )}
                </>
              )
            },
          },
          {
            field: 'description',
            title: 'Description',
            headerSort: false,
            widthGrow: 2,
            formatter: (cell) => cell.getValue() ?? 'No description',
          },
          {
            field: 'permissions',
            title: 'Permissions',
            headerSort: false,
            width: 120,
            formatter: (cell) => (cell.getValue()?.length ?? 0),
          },
          {
            field: 'userCount',
            title: 'Users',
            headerSort: true,
            width: 80,
            hozAlign: 'right',
          },
        ]}
        actions={[
          { key: 'view', label: 'View' },
          { key: 'edit', label: 'Edit' },
          { key: 'delete', label: 'Delete', variant: 'destructive', visible: (row) => !isSystemRole(row.name) },
        ]}
        onAction={(action, row) => {
          if (action === 'view') setDetailsRole(row)
          if (action === 'edit') setDialogRole(row)
          if (action === 'delete') setDeleteRole(row)
        }}
      />

      {dialogRole !== null ? (
        <RoleDialog
          key={dialogRole?.id ?? 'new'}
          role={dialogRole || undefined}
          onClose={() => setDialogRole(null)}
        />
      ) : null}

      {detailsRole ? <RoleDetailsDialog role={detailsRole} onClose={() => setDetailsRole(null)} /> : null}

      <ConfirmDialog
        open={Boolean(deleteRole)}
        onOpenChange={(open) => !open && setDeleteRole(null)}
        title="Delete role?"
        description={deleteRole ? `This will permanently delete "${deleteRole.name}". Roles assigned to users cannot be deleted.` : ''}
        confirmLabel="Delete"
        variant="destructive"
        onConfirm={() => deleteRole && deleteRoleMutation.mutate(deleteRole.id)}
      />
    </div>
  )
}
