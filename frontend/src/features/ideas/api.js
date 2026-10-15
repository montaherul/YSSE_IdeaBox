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
  feature: (id) => api.post(`/ideas/${id}/feature`),
  unfeature: (id) => api.post(`/ideas/${id}/unfeature`),

  // Private idea document (single PDF per idea).
  uploadAttachment: (id, file) => {
    const form = new FormData()
    form.append('file', file)
    return api.upload(`/ideas/${id}/attachment`, form)
  },
  removeAttachment: (id) => api.delete(`/ideas/${id}/attachment`),
  // Token-protected, so it is fetched as a blob instead of a plain link.
  downloadAttachment: (id) => api.get(`/ideas/${id}/attachment`, { responseType: 'blob' }),
}

export function cleanParams(params) {
  return Object.fromEntries(
    Object.entries(params ?? {}).filter(([, value]) =>
      value !== undefined && value !== null && value !== '',
    ),
  )
}

export default ideasApi