import api from '@/services/apiClient'

export const staticApi = {
  getByKey: (key) => api.get(`/static-content/${key}`),
  getAll: (params) => api.getPaged('/static-content', { params }),
  create: (payload) => api.post('/static-content', payload),
  update: (id, payload) => api.put(`/static-content/${id}`, payload),
  delete: (id) => api.delete(`/static-content/${id}`),
}

export default staticApi