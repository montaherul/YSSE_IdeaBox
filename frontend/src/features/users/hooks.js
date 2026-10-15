import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { usersApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export const USERS_QUERY_KEY = ['users']

function invalidateUsers(qc) {
  qc.invalidateQueries({ queryKey: USERS_QUERY_KEY })
  qc.invalidateQueries({ queryKey: ['admin', 'audit-logs'] })
}

export function useUserDetails(id) {
  return useQuery({
    queryKey: [...USERS_QUERY_KEY, 'detail', id],
    queryFn: () => usersApi.get(id),
    enabled: Boolean(id),
  })
}

export function useCreateUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => usersApi.create(payload),
    onSuccess: () => {
      invalidateUsers(qc)
      toast.success('User created.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => usersApi.update(id, payload),
    onSuccess: () => {
      invalidateUsers(qc)
      toast.success('User updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => usersApi.remove(id),
    onSuccess: () => {
      invalidateUsers(qc)
      toast.success('User deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useSuspendUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => usersApi.suspend(id, payload),
    onSuccess: () => {
      invalidateUsers(qc)
      toast.success('User suspended.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useReactivateUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => usersApi.reactivate(id),
    onSuccess: () => {
      invalidateUsers(qc)
      toast.success('User reactivated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useAssignRole() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => usersApi.assignRole(id, payload),
    onSuccess: () => {
      invalidateUsers(qc)
      toast.success('Role assigned.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useRemoveRole() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, role }) => usersApi.removeRole(id, role),
    onSuccess: () => {
      invalidateUsers(qc)
      toast.success('Role removed.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}
