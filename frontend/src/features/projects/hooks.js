import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { projectsApi, outcomesApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export function useProject(id, enabled = true) {
  return useQuery({
    queryKey: ['projects', 'id', id],
    queryFn: () => projectsApi.byId(id),
    enabled: enabled && Boolean(id),
    retry: false,
  })
}

export function useIdeaProjects(ideaId, enabled = true) {
  return useQuery({
    queryKey: ['projects', 'idea', ideaId],
    queryFn: () => projectsApi.ideaProjects(ideaId),
    enabled: enabled && Boolean(ideaId),
    retry: false,
  })
}

export function useMyProjects() {
  return useQuery({
    queryKey: ['projects', 'mine'],
    queryFn: () => projectsApi.mine(),
    retry: false,
  })
}

export function useArchivedProjects(params) {
  return useQuery({
    queryKey: ['projects', 'archive', params],
    queryFn: () => projectsApi.archivedProjects(params),
    placeholderData: (prev) => prev,
  })
}

export function useProjectLeadCandidates(search) {
  return useQuery({
    queryKey: ['projects', 'lead-candidates', search],
    queryFn: () => projectsApi.leadCandidates(search ? { search } : undefined),
    placeholderData: (prev) => prev,
  })
}

export function useCreateProject() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => projectsApi.createProject(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['projects'] })
      toast.success('Project created.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateProject() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => projectsApi.updateProject(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['projects'] })
      toast.success('Project updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteProject() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => projectsApi.deleteProject(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['projects'] })
      toast.success('Project deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useProjectStatus() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => projectsApi.projectStatus(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['projects'] })
      toast.success('Project status updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useArchiveProject() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => projectsApi.archiveProject(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['projects'] })
      toast.success('Project archived as a success.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useCreateMilestone() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ projectId, payload }) => projectsApi.createMilestone(projectId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['projects'] })
      toast.success('Milestone added.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useCreateTask() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ projectId, payload }) => projectsApi.createTask(projectId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['projects'] })
      toast.success('Task added.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useCreateProjectUpdate() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ projectId, payload }) => projectsApi.createUpdate(projectId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['projects'] })
      toast.success('Progress update published.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

/* ---------------- Outcomes ---------------- */

export function useIdeaOutcomes(ideaId, params, enabled = true) {
  return useQuery({
    queryKey: ['outcomes', 'idea', ideaId, params],
    queryFn: () => outcomesApi.ideaOutcomes(ideaId, params),
    enabled: enabled && Boolean(ideaId),
    placeholderData: (prev) => prev,
  })
}

export function useUpdateOutcome() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => outcomesApi.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['outcomes'] })
      toast.success('Outcome updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteOutcome() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => outcomesApi.remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['outcomes'] })
      qc.invalidateQueries({ queryKey: ['leaderboards'] })
      toast.success('Outcome deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useCreateOutcome() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => outcomesApi.create(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['outcomes'] })
      qc.invalidateQueries({ queryKey: ['leaderboards'] })
      toast.success('Outcome reported.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useVerifyOutcome() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => outcomesApi.verify(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['outcomes'] })
      toast.success('Outcome verification updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}