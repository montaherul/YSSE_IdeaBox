import api from '@/services/apiClient'

export const messagingApi = {
  conversations: (params) => api.getPaged('/conversations', { params }),
  conversation: (id) => api.get(`/conversations/${id}`),
  startConversation: (payload) => api.post('/conversations', payload),
  sendMessage: (conversationId, payload) =>
    api.post(`/conversations/${conversationId}/messages`, payload),
  markRead: (conversationId, payload) => api.post(`/conversations/${conversationId}/read`, payload ?? {}),
  conversationStatus: (conversationId, payload) => api.post(`/conversations/${conversationId}/status`, payload),
  blockUser: (userId, payload) => api.post(`/users/${userId}/block`, payload ?? {}),
  createReport: (payload) => api.post('/reports', payload),
}

export default messagingApi