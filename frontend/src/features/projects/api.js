import api from '@/services/apiClient'

export const projectsApi = {
  byId: (id) => api.get(`/projects/${id}`),
  ideaProjects: (ideaId) => api.get(`/ideas/${ideaId}/projects`),
  createProject: (payload) => api.post('/projects', payload),
  updateProject: (id, payload) => api.put(`/projects/${id}`, payload),
  deleteProject: (id) => api.delete(`/projects/${id}`),
  projectStatus: (id, payload) => api.post(`/projects/${id}/status`, payload),
  createMilestone: (projectId, payload) => api.post(`/projects/${projectId}/milestones`, payload),
  createTask: (projectId, payload) => api.post(`/projects/${projectId}/tasks`, payload),
  createUpdate: (projectId, payload) => api.post(`/projects/${projectId}/updates`, payload),
}

export const outcomesApi = {
  ideaOutcomes: (ideaId, params) => api.getPaged(`/ideas/${ideaId}/outcomes`, { params }),
  create: (payload) => api.post('/outcomes', payload),
  update: (id, payload) => api.put(`/outcomes/${id}`, payload),
  remove: (id) => api.delete(`/outcomes/${id}`),
  verify: (id, payload) => api.post(`/outcomes/${id}/verify`, payload ?? {}),
}

export default projectsApi