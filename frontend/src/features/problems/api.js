import api from '@/services/apiClient'

export const problemsApi = {
  list: (params) => api.getPaged('/problems', { params }),
  byId: (id) => api.get(`/problems/${id}`),
  create: (payload) => api.post('/problems', payload),
  update: (id, payload) => api.put(`/problems/${id}`, payload),
  remove: (id) => api.delete(`/problems/${id}`),
  endorse: (id) => api.post(`/problems/${id}/endorse`),
  status: (id, payload) => api.post(`/problems/${id}/status`, payload),
  link: (id, payload) => api.post(`/problems/${id}/link`, payload),
}

export default problemsApi