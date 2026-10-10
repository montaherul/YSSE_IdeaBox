import api from '@/services/apiClient'

export const categoriesApi = {
  list: (config) => api.get('/categories', config),
  tags: (config) => api.get('/tags', config),
  create: (payload) => api.post('/categories', payload),
  update: (id, payload) => api.put(`/categories/${id}`, payload),
  remove: (id) => api.delete(`/categories/${id}`),
}

export default categoriesApi