import api from '@/services/apiClient'

export const entrepreneurApi = {
  createInquiry: (ideaId, payload) => api.post(`/ideas/${ideaId}/inquiries`, payload),
  inquiriesReceived: (params) => api.getPaged('/inquiries/received', { params }),
  inquiriesSent: (params) => api.getPaged('/inquiries/sent', { params }),
  respondInquiry: (id, payload) => api.post(`/inquiries/${id}/respond`, payload),

  watchlist: (params) => api.getPaged('/watchlist', { params }),
  addToWatchlist: (ideaId) => api.post(`/watchlist/${ideaId}`),
  removeFromWatchlist: (ideaId) => api.delete(`/watchlist/${ideaId}`),
  matches: (params) => api.get('/matches', { params }),
}

export default entrepreneurApi