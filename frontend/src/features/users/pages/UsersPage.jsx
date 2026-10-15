import { useState } from 'react'
import { Ban, Plus, Users } from 'lucide-react'
import { z } from 'zod'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { usePermission } from '@/hooks/usePermission'
import {
  useSuspendUser,
  useReactivateUser,
  useAssignRole,
  useRemoveRole,
  useCreateUser,
  useUpdateUser,
  useDeleteUser,
  useUserDetails,
  USERS_QUERY_KEY,
} from '@/features/users/hooks'
import { usersApi } from '@/features/users/api'
import { useAdminRoles } from '@/features/admin/hooks'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { ResourceFormDialog } from '@/features/_shared'
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'
import { formatDate } from '@/lib/format'

const userCreateSchema = z.object({
  email: z.string().trim().min(1, 'Email is required').email('Enter a valid email').max(256),
  displayName: z.string().trim().min(1, 'Name is required').max(120),
  password: z.string().min(8, 'At least 8 characters').max(128),
  phoneNumber: z.string().max(32).optional().or(z.literal('')),
  role: z.string().optional().or(z.literal('')),
})

const userEditSchema = z.object({
  displayName: z.string().trim().min(1, 'Name is required').max(120),
  email: z.string().trim().min(1, 'Email is required').email('Enter a valid email').max(256),
  phoneNumber: z.string().max(32).optional().or(z.literal('')),
  isEmailVerified: z.boolean().optional(),
})

function isSuspended(user) {
  return Boolean(user?.isSuspended)
}

/** Combined role management: current roles with remove buttons plus an assign form. */
function UserRolesDialog({ user, onClose }) {
  const assign = useAssignRole()
  const removeRole = useRemoveRole()
  const { data: allRoles } = useAdminRoles({})
  const roleOptions = (allRoles ?? []).map((r) => r.name)
  const [roles, setRoles] = useState(user.roles ?? [])
  const [role, setRole] = useState('')
  const [error, setError] = useState('')

  const add = async () => {
    if (!role) {
      setError('Choose a role to assign.')
      return
    }
    try {
      await assign.mutateAsync({
        id: user.id,
        payload: { role, justification: `Administrator assignment for ${user.displayName ?? user.email ?? user.id}` },
      })
      setRoles((prev) => Array.from(new Set([...prev, role])))
      setRole('')
      setError('')
    } catch (err) {
      setError(err?.message ?? 'Could not assign role.')
    }
  }

  const remove = async (value) => {
    try {
      await removeRole.mutateAsync({ id: user.id, role: value })
      setRoles((prev) => prev.filter((r) => r !== value))
      setError('')
    } catch (err) {
      setError(err?.message ?? 'Could not remove role.')
    }
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle id="manage-roles-title">Roles for {user.displayName ?? user.email}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          {error ? <p role="alert" className="text-sm text-destructive">{error}</p> : null}

          <div>
            <p className="mb-2 text-sm font-medium">Current roles</p>
            {roles.length === 0 ? (
              <p className="text-sm text-muted-foreground">No roles assigned.</p>
            ) : (
              <div className="flex flex-wrap gap-2">
                {roles.map((r) => (
                  <span key={r} className="inline-flex items-center gap-1 rounded-full bg-secondary px-2 py-0.5 text-xs">
                    {r}
                    <button
                      type="button"
                      aria-label={`Remove role ${r}`}
                      className="rounded-full p-0.5 hover:bg-muted"
                      onClick={() => remove(r)}
                      disabled={removeRole.isPending}
                    >
                      &times;
                    </button>
                  </span>
                ))}
              </div>
            )}
          </div>

          <FormField id="assign-role" label="Assign a role">
            <div className="flex gap-2">
              <Select value={role} onValueChange={setRole}>
                <SelectTrigger id="assign-role" aria-label="Role" className="flex-1">
                  <SelectValue placeholder="Choose a role" />
                </SelectTrigger>
                <SelectContent>
                  {roleOptions.map((r) => (
                    <SelectItem key={r} value={r}>{r}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Button type="button" onClick={add} disabled={assign.isPending || !role}>
                Assign
              </Button>
            </div>
          </FormField>
        </div>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={onClose}>Close</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

function UserEditDialog({ user, onClose }) {
  const update = useUpdateUser()

  const onSubmit = async (values) => {
    await update.mutateAsync({
      id: user.id,
      payload: {
        displayName: values.displayName.trim(),
        email: values.email.trim(),
        phoneNumber: values.phoneNumber?.trim() || undefined,
        isEmailVerified: Boolean(values.isEmailVerified),
      },
    })
  }

  return (
    <ResourceFormDialog
      open
      onClose={onClose}
      title="Edit user"
      description="Update the account's profile and verification status."
      schema={userEditSchema}
      defaultValues={{
        displayName: user.displayName ?? '',
        email: user.email ?? '',
        phoneNumber: user.phoneNumber ?? '',
        isEmailVerified: Boolean(user.isEmailVerified),
      }}
      onSubmit={onSubmit}
      submitLabel="Save changes"
      pendingLabel="Saving…"
    >
      {(form) => (
        <>
          <FormField id="edit-display-name" label="Display name" required error={form.formState.errors.displayName?.message}>
            <Input id="edit-display-name" maxLength={120} {...form.register('displayName')} />
          </FormField>
          <FormField id="edit-email" label="Email" required error={form.formState.errors.email?.message}>
            <Input id="edit-email" type="email" maxLength={256} {...form.register('email')} />
          </FormField>
          <FormField id="edit-phone" label="Phone (optional)" error={form.formState.errors.phoneNumber?.message}>
            <Input id="edit-phone" maxLength={32} {...form.register('phoneNumber')} />
          </FormField>
          <FormField id="edit-verified" label="Email verification">
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" className="h-4 w-4 rounded border-input" {...form.register('isEmailVerified')} />
              Mark this account&apos;s email as verified
            </label>
          </FormField>
        </>
      )}
    </ResourceFormDialog>
  )
}

function UserDetailsDialog({ user, onClose }) {
  const { data, isLoading, isError } = useUserDetails(user.id)
  const detail = data ?? user

  const rows = [
    ['Name', detail.displayName || '—'],
    ['Email', detail.email || '—'],
    ['Phone', detail.phoneNumber || '—'],
    ['Status', detail.isSuspended ? `Suspended${detail.suspensionReason ? ` — ${detail.suspensionReason}` : ''}` : 'Active'],
    ['Email verified', detail.isEmailVerified ? 'Yes' : 'No'],
    ['Roles', Array.isArray(detail.roles) && detail.roles.length ? detail.roles.join(', ') : '—'],
    ['Joined', detail.createdAt ? formatDate(detail.createdAt) : '—'],
  ]

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle id="user-details-title">User details</DialogTitle>
        </DialogHeader>
        {isLoading ? (
          <p className="text-sm text-muted-foreground">Loading…</p>
        ) : isError ? (
          <p role="alert" className="text-sm text-destructive">Could not load user details.</p>
        ) : (
          <div className="space-y-2 text-sm">
            {rows.map(([label, value]) => (
              <div key={label} className="flex justify-between gap-4">
                <span className="text-muted-foreground">{label}</span>
                <span className="break-words text-right">{value}</span>
              </div>
            ))}
          </div>
        )}
        <DialogFooter>
          <Button type="button" variant="outline" onClick={onClose}>Close</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

export default function UsersPage() {
  useDocumentTitle('Users')
  const { canPermission } = usePermission()
  const canCreate = canPermission('users.create')
  const canUpdate = canPermission('users.update')
  const canDelete = canPermission('users.delete')
  const canSuspend = canPermission('users.suspend')
  const canManageRoles = canPermission('users.manage_roles')

  const [rolesFor, setRolesFor] = useState(null)
  const [suspendFor, setSuspendFor] = useState(null)
  const [suspendReason, setSuspendReason] = useState('')
  const [creating, setCreating] = useState(false)
  const [editFor, setEditFor] = useState(null)
  const [detailsFor, setDetailsFor] = useState(null)
  const [deleteFor, setDeleteFor] = useState(null)

  const suspend = useSuspendUser()
  const reactivate = useReactivateUser()
  const create = useCreateUser()
  const remove = useDeleteUser()
  const { data: allRoles } = useAdminRoles({})
  const roleOptions = (allRoles ?? []).map((r) => r.name)

  const onCreate = async (values) => {
    const { role, phoneNumber, ...rest } = values
    await create.mutateAsync({
      ...rest,
      phoneNumber: phoneNumber || undefined,
      roles: role ? [role] : [],
    })
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Users</h1>
          <p className="text-muted-foreground">Server-side listing of accounts. Manage roles and account status.</p>
        </div>
        {canCreate ? (
          <Button size="sm" onClick={() => setCreating(true)}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            New user
          </Button>
        ) : null}
      </div>

      <ServerDataTable
        queryKey={USERS_QUERY_KEY}
        queryFn={(params) => usersApi.list(params)}
        pageSize={DEFAULT_PAGE_SIZE}
        searchable={false}
        emptyIcon={Users}
        emptyTitle="No users found"
        emptyDescription="Try a different search."
        columns={[
          {
            field: 'displayName',
            title: 'Name',
            headerSort: true,
            formatter: (cell) => cell.getValue() ?? cell.getRow().getData().email ?? '—',
          },
          { field: 'email', title: 'Email', headerSort: true },
          {
            field: 'roles',
            title: 'Roles',
            headerSort: false,
            formatter: (cell) => (Array.isArray(cell.getValue()) ? cell.getValue().join(', ') : '—'),
          },
          {
            field: 'isSuspended',
            title: 'Status',
            headerSort: true,
            formatter: (cell) => (cell.getValue() ? 'Suspended' : 'Active'),
          },
          {
            field: 'createdAt',
            title: 'Joined',
            headerSort: true,
            formatter: (cell) => formatDate(cell.getValue(), { short: true }),
          },
        ]}
        actions={[
          { key: 'view', label: 'View' },
          { key: 'edit', label: 'Edit', visible: () => canUpdate },
          { key: 'roles', label: 'Manage roles', visible: () => canManageRoles },
          { key: 'suspend', label: 'Suspend', variant: 'destructive', visible: (row) => canSuspend && !isSuspended(row) },
          { key: 'reactivate', label: 'Reactivate', visible: (row) => canSuspend && isSuspended(row) },
          { key: 'delete', label: 'Delete', variant: 'destructive', visible: () => canDelete },
        ]}
        onAction={(action, row) => {
          if (action === 'view') setDetailsFor(row)
          if (action === 'edit') setEditFor(row)
          if (action === 'roles') setRolesFor(row)
          if (action === 'suspend') {
            setSuspendFor(row)
            setSuspendReason('')
          }
          if (action === 'reactivate') reactivate.mutate(row.id)
          if (action === 'delete') setDeleteFor(row)
        }}
      />

      {rolesFor ? <UserRolesDialog user={rolesFor} onClose={() => setRolesFor(null)} /> : null}

      {detailsFor ? <UserDetailsDialog user={detailsFor} onClose={() => setDetailsFor(null)} /> : null}

      {editFor ? <UserEditDialog user={editFor} onClose={() => setEditFor(null)} /> : null}

      {creating ? (
        <ResourceFormDialog
          open
          onClose={() => setCreating(false)}
          title="Create user"
          description="Creates an account with a temporary password. The user can sign in immediately."
          schema={userCreateSchema}
          defaultValues={{ email: '', displayName: '', password: '', phoneNumber: '', role: '' }}
          onSubmit={onCreate}
          submitLabel="Create user"
          pendingLabel="Creating…"
        >
          {(form) => (
            <>
              <FormField id="user-display-name" label="Display name" required error={form.formState.errors.displayName?.message}>
                <Input id="user-display-name" maxLength={120} {...form.register('displayName')} />
              </FormField>
              <FormField id="user-email" label="Email" required error={form.formState.errors.email?.message}>
                <Input id="user-email" type="email" maxLength={256} {...form.register('email')} />
              </FormField>
              <FormField id="user-password" label="Temporary password" required hint="At least 8 characters" error={form.formState.errors.password?.message}>
                <Input id="user-password" type="password" maxLength={128} {...form.register('password')} />
              </FormField>
              <FormField id="user-phone" label="Phone (optional)" error={form.formState.errors.phoneNumber?.message}>
                <Input id="user-phone" maxLength={32} {...form.register('phoneNumber')} />
              </FormField>
              <FormField id="user-role" label="Additional role (optional)">
                <Select
                  value={form.watch('role') ?? ''}
                  onValueChange={(value) => form.setValue('role', value)}
                >
                  <SelectTrigger id="user-role" aria-label="Additional role">
                    <SelectValue placeholder="Member (default)" />
                  </SelectTrigger>
                  <SelectContent>
                    {roleOptions.map((r) => (
                      <SelectItem key={r} value={r}>{r}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FormField>
            </>
          )}
        </ResourceFormDialog>
      ) : null}

      <Dialog open={Boolean(suspendFor)} onOpenChange={(open) => !open && setSuspendFor(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle id="suspend-user-title">Suspend {suspendFor?.displayName ?? suspendFor?.email}?</DialogTitle>
          </DialogHeader>
          <FormField id="suspend-reason" label="Reason" hint="Shown in the audit log">
            <Input id="suspend-reason" value={suspendReason} onChange={(e) => setSuspendReason(e.target.value)} />
          </FormField>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => setSuspendFor(null)}>Cancel</Button>
            <Button
              type="button"
              onClick={() => {
                suspend.mutate({ id: suspendFor.id, payload: { reason: suspendReason || 'Administrator decision' } })
                setSuspendFor(null)
              }}
              disabled={suspend.isPending}
            >
              <Ban className="h-4 w-4" aria-hidden="true" />
              Confirm suspension
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <ConfirmDialog
        open={Boolean(deleteFor)}
        onOpenChange={(open) => !open && setDeleteFor(null)}
        title="Delete user?"
        description={
          deleteFor
            ? `Permanently delete ${deleteFor.displayName ?? deleteFor.email}. This is blocked if the account owns any records or is the last active administrator.`
            : ''
        }
        confirmLabel="Delete user"
        variant="destructive"
        onConfirm={() => deleteFor && remove.mutate(deleteFor.id)}
      />
    </div>
  )
}