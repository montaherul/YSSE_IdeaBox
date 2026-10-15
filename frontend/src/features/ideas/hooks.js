import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { ideasApi } from './api'
import { screeningApi } from './screening-api'
import { toErrorMessage } from '@/lib/validation'

/**
 * Applies the optional private-PDF changes after the idea itself has been saved.
 * A failed upload must not fail the whole submission (the idea is already persisted),
 * so the error is surfaced as a toast without throwing.
 */
async function applyIdeaAttachment({ ideaId, attachmentFile, removeAttachment }) {
  if (!ideaId) return
  if (attachmentFile) {
    try {
      await ideasApi.uploadAttachment(ideaId, attachmentFile)
    } catch (e) {
      toast.error(`Idea saved, but the document upload failed: ${toErrorMessage(e)}`)
    }
    return
  }
  if (removeAttachment) {
    try {
      await ideasApi.removeAttachment(ideaId)
    } catch (e) {
      toast.error(`Idea saved, but the document could not be removed: ${toErrorMessage(e)}`)
    }
  }
}

export function useIdeaList(params, enabled = true) {
  return useQuery({
    queryKey: ['ideas', 'list', params],
    queryFn: () => ideasApi.list(params),
    enabled,
    placeholderData: (prev) => prev,
  })
}

export function useIdeaBySlug(slug) {
  return useQuery({
    queryKey: ['ideas', 'slug', slug],
    queryFn: () => ideasApi.bySlug(slug),
    enabled: Boolean(slug),
    retry: false,
  })
}

export function useIdeaById(id) {
  return useQuery({
    queryKey: ['ideas', 'id', id],
    queryFn: () => ideasApi.byId(id),
    enabled: Boolean(id),
    retry: false,
  })
}

export function useMyIdeas(params) {
  return useQuery({
    queryKey: ['ideas', 'mine', params],
    queryFn: () => ideasApi.mine(params),
    placeholderData: (prev) => prev,
  })
}

export function useSimilarIdeas(id, enabled = true) {
  return useQuery({
    queryKey: ['ideas', id, 'similar'],
    queryFn: () => ideasApi.similar(id),
    enabled: enabled && Boolean(id),
    staleTime: 5 * 60_000,
  })
}

export function useRegisterIdeaView() {
  return useMutation({
    mutationFn: (id) => ideasApi.registerView(id),
    retry: false,
  })
}

export function useCreateIdea(quick = false) {
  const qc = useQueryClient()
  const navigate = useNavigate()
  return useMutation({
    mutationFn: async ({ attachmentFile, removeAttachment, ...payload }) => {
      const id = await (quick ? ideasApi.createQuick(payload) : ideasApi.create(payload))
      await applyIdeaAttachment({ ideaId: id, attachmentFile, removeAttachment })
      return id
    },
    onSuccess: (data) => {
      qc.invalidateQueries({ queryKey: ['ideas', 'mine'] })
      qc.invalidateQueries({ queryKey: ['ideas', 'list'] })
      toast.success(quick ? 'Quick idea saved.' : 'Idea submitted for screening.')
      navigate('/me/ideas')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateIdea() {
  const qc = useQueryClient()
  const navigate = useNavigate()
  return useMutation({
    mutationFn: async ({ id, payload, attachmentFile, removeAttachment }) => {
      await ideasApi.update(id, payload)
      await applyIdeaAttachment({ ideaId: id, attachmentFile, removeAttachment })
      return id
    },
    onSuccess: (data) => {
      qc.invalidateQueries({ queryKey: ['ideas'] })
      toast.success('Idea updated.')
      navigate('/me/ideas')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useSubmitIdea() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => ideasApi.submit(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['ideas', 'mine'] })
      toast.success('Idea submitted for screening.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useWithdrawIdea() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => ideasApi.withdraw(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['ideas', 'mine'] })
      qc.invalidateQueries({ queryKey: ['ideas', 'list'] })
      toast.success('Idea withdrawn.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useSetIdeaFeatured() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, featured }) => (featured ? ideasApi.feature(id) : ideasApi.unfeature(id)),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['ideas'] })
      toast.success(variables.featured ? 'Idea featured.' : 'Idea removed from featured.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteIdea() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => ideasApi.remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['ideas'] })
      toast.success('Idea deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

/* ---------------- Screening ---------------- */

export function useScreeningQueue(params) {
  return useQuery({
    queryKey: ['screening', 'queue', params],
    queryFn: () => screeningApi.queue(params),
  })
}

export function useScreeningDecision(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => screeningApi.decision(ideaId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['screening'] })
      qc.invalidateQueries({ queryKey: ['ideas', ideaId, 'decisions'] })
      qc.invalidateQueries({ queryKey: ['ideas', 'id', ideaId] })
      toast.success('Screening decision recorded.')
    },
    onError: (e) => {
      toast.error(toErrorMessage(e))
    },
  })
}

export function useScreeningStatus(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => screeningApi.status(ideaId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['screening'] })
      qc.invalidateQueries({ queryKey: ['ideas'] })
      toast.success('Idea status updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useScreeningDecisions(ideaId) {
  return useQuery({
    queryKey: ['ideas', ideaId, 'decisions'],
    queryFn: () => screeningApi.decisions(ideaId),
    enabled: Boolean(ideaId),
  })
}