import api from '@/services/apiClient'
import { ideasApi } from './api'

export const screeningApi = {
  queue: (params) => api.getPaged('/screening/queue', { params }),
  decision: (ideaId, payload) => api.post(`/screening/${ideaId}/decision`, payload),
  status: (ideaId, payload) => api.post(`/screening/${ideaId}/status`, payload),
  decisions: (ideaId) => api.get(`/screening/${ideaId}/decisions`),
}

export { ideasApi }
export default api