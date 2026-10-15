import api from '@/services/apiClient'

export const marketplaceApi = {
  list: (params) => api.getPaged('/marketplace', { params }),
  byId: (id) => api.get(`/marketplace/${id}`),
  create: (payload) => api.post('/marketplace', payload),
  update: (id, payload) => api.put(`/marketplace/${id}`, payload),
  remove: (id) => api.delete(`/marketplace/${id}`),
  status: (id, payload) => api.post(`/marketplace/${id}/status`, payload),
  mine: (params) => api.getPaged('/marketplace/mine', { params }),
  createReservation: (listingId, payload) => api.post(`/marketplace/${listingId}/reservations`, payload),
  listingReservations: (listingId) => api.get(`/marketplace/${listingId}/reservations`),
  respondReservation: (reservationId, payload) => api.post(`/reservations/${reservationId}/respond`, payload),
}

export default marketplaceApi