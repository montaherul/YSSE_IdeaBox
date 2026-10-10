import api from '@/services/apiClient'

export const commentsApi = {
  list: (ideaId) => api.get(`/ideas/${ideaId}/comments`),
  create: (ideaId, payload) => api.post(`/ideas/${ideaId}/comments`, payload),
  remove: (id) => api.delete(`/comments/${id}`),
  moderate: (id, payload) => api.post(`/comments/${id}/moderate`, payload),
}

export default commentsApi