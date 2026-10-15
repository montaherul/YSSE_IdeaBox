import api from '@/services/apiClient'

export const profileApi = {
  member: () => api.get('/profiles/me'),
  updateMember: (payload) => api.put('/profiles/me', payload),

  uploadAvatar: (file) => {
    const form = new FormData()
    form.append('file', file)
    return api.upload('/profiles/me/avatar', form)
  },
  removeAvatar: () => api.delete('/profiles/me/avatar'),

  creator: () => api.get('/profiles/creator/me'),
  updateCreator: (payload) => api.put('/profiles/creator/me', payload),
  deleteCreator: () => api.delete('/profiles/creator/me'),
  creatorByUser: (userId) => api.get(`/profiles/creator/${userId}`),

  entrepreneur: () => api.get('/profiles/entrepreneur/me'),
  updateEntrepreneur: (payload) => api.put('/profiles/entrepreneur/me', payload),
  entrepreneurPending: () => api.get('/profiles/entrepreneur/pending'),
  approveEntrepreneur: (userId) => api.post(`/profiles/entrepreneur/${userId}/approve`),
}

export default profileApi