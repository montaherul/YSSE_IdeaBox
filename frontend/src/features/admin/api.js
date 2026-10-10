import api from '@/services/apiClient'

export const adminApi = {
  users: (params) => api.getPaged('/admin/users', { params }),
  suspendUser: (id, payload) => api.post(`/admin/users/${id}/suspend`, payload),
  reactivateUser: (id, payload) => api.post(`/admin/users/${id}/reactivate`, payload ?? {}),
  assignRole: (id, payload) => api.post(`/admin/users/${id}/roles`, payload),
  removeRole: (id, role) => api.delete(`/admin/users/${id}/roles/${role}`),
  auditLogs: (params) => api.getPaged('/admin/audit-logs', { params }),
  settings: () => api.get('/admin/settings'),
  updateSettings: (payload) => api.put('/admin/settings', payload),
  dashboard: () => api.get('/admin/dashboard'),
  reports: (params) => api.getPaged('/admin/reports', { params }),
  resolveReport: (id, payload) => api.post(`/admin/reports/${id}/resolve`, payload),
}

export default adminApi