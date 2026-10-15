import api from '@/services/apiClient'

/** User management endpoints (mirrors UsersController). */
export const usersApi = {
  list: (params) => api.getPaged('/users', { params }),
  get: (id) => api.get(`/users/${id}`),
  create: (payload) => api.post('/users', payload),
  update: (id, payload) => api.put(`/users/${id}`, payload),
  remove: (id) => api.delete(`/users/${id}`),
  suspend: (id, payload) => api.post(`/users/${id}/suspend`, payload),
  reactivate: (id, payload) => api.post(`/users/${id}/reactivate`, payload ?? {}),
  assignRole: (id, payload) => api.post(`/users/${id}/roles`, payload),
  removeRole: (id, role) => api.delete(`/users/${id}/roles/${role}`),
}

export default usersApi
