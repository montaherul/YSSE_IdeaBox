import { useState } from 'react'
import { Ban, CircleCheck, ShieldCheck, Users } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAdminUsers, useSuspendUser, useReactivateUser, useAssignRole, useRemoveRole } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { FormField } from '@/components/ui/form-field'
import { Card, CardContent } from '@/components/ui/card'
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

const ROLES = ['Admin', 'Moderator', 'ScreeningReviewer', 'Entrepreneur', 'Investor', 'Mentor', 'IdeaHelper', 'LanguageEditor']

function RoleDialog({ user, onClose }) {
  const assign = useAssignRole()
  const [role, setRole] = useState('')
  const [error, setError] = useState('')

  const submit = async () => {
    if (!role) {
      setError('Choose a role to assign.')
      return
    }
    try {
      await assign.mutateAsync({ id: user.id, payload: { role, justification: `Admin assignment by ${user.displayName ?? 'admin'}` } })
      onClose()
    } catch (err) {
      setError(err?.message ?? 'Could not assign role.')
    }
  }

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle id="assign-role-title">Assign role to {user.displayName ?? user.email}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          {error ? <p role="alert" className="text-sm text-destructive">{error}</p> : null}
          <FormField id="assign-role" label="Role">
            <Select value={role} onValueChange={setRole}>
              <SelectTrigger id="assign-role" aria-label="Role">
                <SelectValue placeholder="Choose a role" />
              </SelectTrigger>
              <SelectContent>
                {ROLES.map((r) => (
                  <SelectItem key={r} value={r}>{r}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormField>
        </div>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="button" onClick={submit} disabled={assign.isPending}>Assign</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

export default function AdminUsersPage() {
  useDocumentTitle('Users & roles')
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [roleFor, setRoleFor] = useState(null)
  const [suspendFor, setSuspendFor] = useState(null)
  const [suspendReason, setSuspendReason] = useState('')
  const [removeRoleFor, setRemoveRoleFor] = useState(null)

  const { data, isLoading, isError, refetch } = useAdminUsers({ page, pageSize: DEFAULT_PAGE_SIZE, search: search || undefined })
  const suspend = useSuspendUser()
  const reactivate = useReactivateUser()
  const removeRole = useRemoveRole()

  const items = data?.items ?? []

  return (
    <div className="space-y-6">
      <PageHeader title="Users & roles" description="Manage accounts, suspensions and role assignments." />

      <FormField id="admin-user-search" label="Search users">
        <Input id="admin-user-search" placeholder="Name or email…" value={search} onChange={(e) => { setSearch(e.target.value); setPage(1) }} />
      </FormField>

      {isLoading ? <PageSkeleton lines={6} /> : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState icon={Users} title="No users found" description="Try a different search." />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="space-y-3">
            {items.map((user) => (
              <Card key={user.id}>
                <CardContent className="flex flex-wrap items-center gap-3 p-4">
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium">{user.displayName ?? user.email}</p>
                    <p className="truncate text-sm text-muted-foreground">{user.email}</p>
                    {user.roles?.length ? (
                      <p className="mt-1 flex flex-wrap gap-1">
                        {user.roles.map((role) => (
                          <span key={role} className="inline-flex items-center gap-1 rounded-full bg-secondary px-2 py-0.5 text-xs">
                            {role}
                            <button type="button" aria-label={`Remove role ${role}`} className="rounded-full p-0.5 hover:bg-muted" onClick={() => setRemoveRoleFor({ id: user.id, role })}>
                              &times;
                            </button>
                          </span>
                        ))}
                      </p>
                    ) : null}
                  </div>
                  <div className="flex flex-wrap items-center gap-2">
                    <Button size="sm" variant="outline" onClick={() => setRoleFor(user)}>
                      <ShieldCheck className="h-4 w-4" aria-hidden="true" />
                      Assign role
                    </Button>
                    {String(user.status).toLowerCase() === 'suspended' ? (
                      <Button size="sm" onClick={() => reactivate.mutate(user.id)}>
                        <CircleCheck className="h-4 w-4" aria-hidden="true" />
                        Reactivate
                      </Button>
                    ) : (
                      <Button size="sm" variant="outline" onClick={() => { setSuspendFor(user); setSuspendReason('') }}>
                        <Ban className="h-4 w-4" aria-hidden="true" />
                        Suspend
                      </Button>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={data?.total ?? 0} onChange={setPage} />
        </>
      ) : null}

      {roleFor ? <RoleDialog user={roleFor} onClose={() => setRoleFor(null)} /> : null}

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
                suspend.mutate({ id: suspendFor.id, payload: { reason: suspendReason || 'Admin decision' } })
                setSuspendFor(null)
              }}
              disabled={suspend.isPending}
            >
              Confirm suspension
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <ConfirmDialog
        open={Boolean(removeRoleFor)}
        onOpenChange={(open) => !open && setRemoveRoleFor(null)}
        title="Remove role?"
        description={removeRoleFor ? `This removes the ${removeRoleFor.role} role from the user.` : ''}
        confirmLabel="Remove role"
        variant="destructive"
        onConfirm={() => removeRoleFor && removeRole.mutate({ id: removeRoleFor.id, role: removeRoleFor.role })}
      />
    </div>
  )
}