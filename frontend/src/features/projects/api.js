import api from '@/services/apiClient'

export const projectsApi = {
  byId: (id) => api.get(`/projects/${id}`),
  ideaProjects: (ideaId) => api.get(`/ideas/${ideaId}/projects`),
  createProject: (payload) => api.post('/projects', payload),
  projectStatus: (id, payload) => api.post(`/projects/${id}/status`, payload),
  createMilestone: (projectId, payload) => api.post(`/projects/${projectId}/milestones`, payload),
  createTask: (projectId, payload) => api.post(`/projects/${projectId}/tasks`, payload),
  createUpdate: (projectId, payload) => api.post(`/projects/${projectId}/updates`, payload),
}

export const outcomesApi = {
  ideaOutcomes: (ideaId) => api.get(`/ideas/${ideaId}/outcomes`),
  create: (payload) => api.post('/outcomes', payload),
  verify: (id, payload) => api.post(`/outcomes/${id}/verify`, payload ?? {}),
}

export default projectsApi