import api from '@/services/apiClient'

export const commentsApi = {
  list: (ideaId, params) => api.getPaged(`/ideas/${ideaId}/comments`, { params }),
  create: (ideaId, payload) => api.post(`/ideas/${ideaId}/comments`, payload),
  update: (id, payload) => api.put(`/comments/${id}`, payload),
  remove: (id) => api.delete(`/comments/${id}`),
  moderate: (id, payload) => api.post(`/comments/${id}/moderate`, payload),
}

export default commentsApi