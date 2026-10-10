import api from '@/services/apiClient'

export const ideasApi = {
  list: (params) =>
    api.getPaged('/ideas', { params: cleanParams(params) }),
  bySlug: (slug) => api.get(`/ideas/${slug}`),
  byId: (id) => api.get(`/ideas/id/${id}`),
  create: (payload) => api.post('/ideas', payload),
  createQuick: (payload) => api.post('/ideas/quick', payload),
  update: (id, payload) => api.put(`/ideas/${id}`, payload),
  submit: (id) => api.post(`/ideas/${id}/submit`),
  withdraw: (id) => api.post(`/ideas/${id}/withdraw`),
  remove: (id) => api.delete(`/ideas/${id}`),
  mine: (params) => api.getPaged('/ideas/mine', { params: cleanParams(params) }),
  similar: (id, config) => api.get(`/ideas/${id}/similar`, config),
  registerView: (id) => api.post(`/ideas/${id}/view`),
}

export function cleanParams(params) {
  return Object.fromEntries(
    Object.entries(params ?? {}).filter(([, value]) =>
      value !== undefined && value !== null && value !== '',
    ),
  )
}

export default ideasApi