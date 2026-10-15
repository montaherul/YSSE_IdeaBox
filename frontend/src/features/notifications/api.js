import api from '@/services/apiClient'

export const notificationsApi = {
  list: (params) => api.getPaged('/notifications', { params }),
  markRead: (id) => api.post(`/notifications/${id}/read`),
  markAllRead: () => api.post('/notifications/read-all'),
  remove: (id) => api.delete(`/notifications/${id}`),
  preferences: () => api.get('/notifications/preferences'),
  updatePreference: (payload) => api.put('/notifications/preferences', payload),
}

export default notificationsApi