import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { adminApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export function useAdminUsers(params) {
  return useQuery({
    queryKey: ['admin', 'users', params],
    queryFn: () => adminApi.users(params),
    placeholderData: (prev) => prev,
  })
}

export function useSuspendUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => adminApi.suspendUser(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'users'] })
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
    mutationFn: (id) => adminApi.reactivateUser(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'users'] })
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
    mutationFn: ({ id, payload }) => adminApi.assignRole(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'users'] })
      qc.invalidateQueries({ queryKey: ['admin', 'audit-logs'] })
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
    mutationFn: ({ id, role }) => adminApi.removeRole(id, role),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'users'] })
      toast.success('Role removed.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useAuditLogs(params) {
  return useQuery({
    queryKey: ['admin', 'audit-logs', params],
    queryFn: () => adminApi.auditLogs(params),
    placeholderData: (prev) => prev,
  })
}

export function useAdminSettings() {
  return useQuery({
    queryKey: ['admin', 'settings'],
    queryFn: () => adminApi.settings(),
    staleTime: 5 * 60_000,
  })
}

export function useUpdateAdminSettings() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => adminApi.updateSettings(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'settings'] })
      toast.success('Settings saved.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useAdminDashboard() {
  return useQuery({
    queryKey: ['admin', 'dashboard'],
    queryFn: () => adminApi.dashboard(),
    staleTime: 60_000,
  })
}

export function useAdminReports(params) {
  return useQuery({
    queryKey: ['admin', 'reports', params],
    queryFn: () => adminApi.reports(params),
    placeholderData: (prev) => prev,
  })
}

export function useResolveReport() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => adminApi.resolveReport(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'reports'] })
      toast.success('Report resolved.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}