import api from '@/services/apiClient'

export const adminApi = {
  auditLogs: (params) => api.getPaged('/audit-logs', { params }),
  settings: () => api.get('/settings'),
  updateSettings: (payload) => api.put('/settings', payload),
  dashboard: () => api.get('/dashboard'),
  reports: (params) => api.getPaged('/reports', { params }),
  resolveReport: (id, payload) => api.post(`/reports/${id}/resolve`, payload),
  updateReport: (id, payload) => api.put(`/reports/${id}`, payload),
  deleteReport: (id) => api.delete(`/reports/${id}`),
  tags: (params) => api.getPaged('/categories/tags', { params }),
  createTag: (payload) => api.post('/categories/tags', payload),
  updateTag: (id, payload) => api.put(`/categories/tags/${id}`, payload),
  deleteTag: (id) => api.delete(`/categories/tags/${id}`),
  roles: (params) => api.get('/roles', { params }),
  role: (id) => api.get(`/roles/${id}`),
  roleUsers: (id, params) => api.getPaged(`/roles/${id}/users`, { params }),
  createRole: (payload) => api.post('/roles', payload),
  updateRole: (id, payload) => api.put(`/roles/${id}`, payload),
  deleteRole: (id) => api.delete(`/roles/${id}`),
  permissions: (params) => api.get('/permissions', { params }),
  permissionsPaged: (params) => api.getPaged('/permissions/paged', { params }),
  createPermission: (payload) => api.post('/permissions', payload),
  updatePermission: (id, payload) => api.put(`/permissions/${id}`, payload),
  deletePermission: (id) => api.delete(`/permissions/${id}`),
  assignPermissionToRole: (roleId, permissionId) => api.post(`/roles/${roleId}/permissions/${permissionId}`),
  removePermissionFromRole: (roleId, permissionId) => api.delete(`/roles/${roleId}/permissions/${permissionId}`),
}

export default adminApi