import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { adminApi } from './api'
import { toErrorMessage } from '@/lib/validation'

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

export function useUpdateReport() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => adminApi.updateReport(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'reports'] })
      toast.success('Report updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteReport() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => adminApi.deleteReport(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'reports'] })
      toast.success('Report deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useAdminTags(params) {
  return useQuery({
    queryKey: ['admin', 'tags', params],
    queryFn: () => adminApi.tags(params),
    placeholderData: (prev) => prev,
  })
}

export function useCreateTag() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => adminApi.createTag(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'tags'] })
      toast.success('Tag created.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateTag() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => adminApi.updateTag(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'tags'] })
      toast.success('Tag updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteTag() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => adminApi.deleteTag(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'tags'] })
      toast.success('Tag deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useAdminRoles(params) {
  return useQuery({
    queryKey: ['admin', 'roles', params],
    queryFn: () => adminApi.roles(params),
    placeholderData: (prev) => prev,
  })
}

export function useAdminRoleUsers(roleId, params) {
  return useQuery({
    queryKey: ['admin', 'roles', roleId, 'users', params],
    queryFn: () => adminApi.roleUsers(roleId, params),
    enabled: Boolean(roleId),
    placeholderData: (prev) => prev,
  })
}

export function useCreateRole() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => adminApi.createRole(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'roles'] })
      toast.success('Role created.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateRole() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => adminApi.updateRole(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'roles'] })
      toast.success('Role updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteRole() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => adminApi.deleteRole(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'roles'] })
      toast.success('Role deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useAdminPermissions(params) {
  return useQuery({
    queryKey: ['admin', 'permissions', params],
    queryFn: () => adminApi.permissions(params),
    placeholderData: (prev) => prev,
  })
}

export function useCreatePermission() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => adminApi.createPermission(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'permissions'] })
      toast.success('Permission created.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdatePermission() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => adminApi.updatePermission(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'permissions'] })
      toast.success('Permission updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeletePermission() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => adminApi.deletePermission(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'permissions'] })
      toast.success('Permission deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useAssignPermissionToRole() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ roleId, permissionId }) => adminApi.assignPermissionToRole(roleId, permissionId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'roles'] })
      qc.invalidateQueries({ queryKey: ['admin', 'permissions'] })
      toast.success('Permission assigned to role.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useRemovePermissionFromRole() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ roleId, permissionId }) => adminApi.removePermissionFromRole(roleId, permissionId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin', 'roles'] })
      qc.invalidateQueries({ queryKey: ['admin', 'permissions'] })
      toast.success('Permission removed from role.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}