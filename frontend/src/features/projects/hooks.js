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

export function useIdeaOutcomes(ideaId, enabled = true) {
  return useQuery({
    queryKey: ['outcomes', 'idea', ideaId],
    queryFn: () => outcomesApi.ideaOutcomes(ideaId),
    enabled: enabled && Boolean(ideaId),
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