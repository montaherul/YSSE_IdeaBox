import api from '@/services/apiClient'

export const reviewsApi = {
  list: (ideaId) => api.get(`/ideas/${ideaId}/reviews`),
  mine: (params) => api.getPaged('/reviews/mine', { params }),
  counts: (ideaId) => api.get(`/ideas/${ideaId}/reviews/counts`),
  create: (ideaId, payload) => api.post(`/ideas/${ideaId}/reviews`, payload),
  createGuest: (ideaId, payload) => api.post(`/ideas/${ideaId}/reviews/guest`, payload),
  update: (id, payload) => api.patch(`/reviews/${id}`, payload),
  remove: (id) => api.delete(`/reviews/${id}`),
  moderate: (id, payload) => api.post(`/reviews/${id}/moderate`, payload),

  comments: (ideaId, params) => api.getPaged(`/ideas/${ideaId}/comments`, { params }),
  createComment: (ideaId, payload) => api.post(`/ideas/${ideaId}/comments`, payload),
  updateComment: (id, payload) => api.put(`/comments/${id}`, payload),
  deleteComment: (id) => api.delete(`/comments/${id}`),
  moderateComment: (id, payload) => api.post(`/comments/${id}/moderate`, payload),
}

export const followsApi = {
  follow: (ideaId) => api.post(`/ideas/${ideaId}/follow`),
  followed: (params) => api.getPaged('/ideas/followed', { params }),
}

export default reviewsApi