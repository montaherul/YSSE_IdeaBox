import api from '@/services/apiClient'

export const votingApi = {
  votes: (ideaId) => api.get(`/ideas/${ideaId}/votes`),
  castVote: (ideaId, payload) => api.post(`/ideas/${ideaId}/votes`, payload ?? {}),
  removeVote: (ideaId) => api.delete(`/ideas/${ideaId}/votes`),
  periods: (config) => api.get('/voting-periods', config),
  createPeriod: (payload) => api.post('/voting-periods', payload),
  updatePeriod: (id, payload) => api.put(`/voting-periods/${id}`, payload),
  deletePeriod: (id) => api.delete(`/voting-periods/${id}`),
  activatePeriod: (id) => api.post(`/voting-periods/${id}/activate`),
  closePeriod: (id) => api.post(`/voting-periods/${id}/close`),
  leaderboardIdeas: (params) => api.get('/leaderboards/ideas', { params }),
  leaderboardIdeaMakers: (params) => api.get('/leaderboards/idea-makers', { params }),
  monthlyTopIdeas: (params) => api.get('/leaderboards/ideas/current-month', { params }),
  announceMonthlyTop: (payload) =>
    api.post('/leaderboards/ideas/current-month/announce', payload ?? {}),
}

export default votingApi