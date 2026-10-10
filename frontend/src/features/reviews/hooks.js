import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { reviewsApi, followsApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export function useIdeaReviews(ideaId, enabled = true) {
  return useQuery({
    queryKey: ['reviews', ideaId],
    queryFn: () => reviewsApi.list(ideaId),
    enabled: enabled && Boolean(ideaId),
  })
}

export function useMyReviews(params) {
  return useQuery({
    queryKey: ['reviews', 'mine', params],
    queryFn: () => reviewsApi.mine(params),
    placeholderData: (prev) => prev,
  })
}

export function useReviewCounts(ideaId) {
  return useQuery({
    queryKey: ['reviews', ideaId, 'counts'],
    queryFn: () => reviewsApi.counts(ideaId),
    enabled: Boolean(ideaId),
    staleTime: 30_000,
  })
}

function invalidateReviews(qc, ideaId) {
  qc.invalidateQueries({ queryKey: ['reviews', ideaId] })
  qc.invalidateQueries({ queryKey: ['reviews', ideaId, 'counts'] })
  qc.invalidateQueries({ queryKey: ['ideas', 'list'] })
}

export function useCreateReview(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => reviewsApi.create(ideaId, payload),
    onSuccess: () => {
      invalidateReviews(qc, ideaId)
      toast.success('Thanks for your review!')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useCreateGuestReview(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => reviewsApi.createGuest(ideaId, payload),
    onSuccess: () => {
      invalidateReviews(qc, ideaId)
      toast.success('Thanks for your anonymous review!')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateReview(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => reviewsApi.update(id, payload),
    onSuccess: () => {
      invalidateReviews(qc, ideaId)
      toast.success('Review updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteReview(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => reviewsApi.remove(id),
    onSuccess: () => {
      invalidateReviews(qc, ideaId)
      toast.success('Review removed.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useModerateReview(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => reviewsApi.moderate(id, payload),
    onSuccess: () => {
      invalidateReviews(qc, ideaId)
      toast.success('Review moderated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

/* ---------------- Comments ---------------- */

export function useIdeaComments(ideaId) {
  return useQuery({
    queryKey: ['comments', ideaId],
    queryFn: () => reviewsApi.comments(ideaId),
    enabled: Boolean(ideaId),
  })
}

export function useCreateComment(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => reviewsApi.createComment(ideaId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['comments', ideaId] })
      toast.success('Comment posted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteComment(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => reviewsApi.deleteComment(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['comments', ideaId] })
      toast.success('Comment deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useModerateComment(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => reviewsApi.moderateComment(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['comments', ideaId] })
      toast.success('Comment moderated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

/* ---------------- Follows ---------------- */

export function useFollowedIdeas(params) {
  return useQuery({
    queryKey: ['ideas', 'followed', params],
    queryFn: () => followsApi.followed(params),
    placeholderData: (prev) => prev,
  })
}

export function useFollowIdea() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (ideaId) => followsApi.follow(ideaId),
    onSuccess: (_, ideaId) => {
      qc.invalidateQueries({ queryKey: ['ideas', 'followed'] })
      qc.invalidateQueries({ queryKey: ['ideas', 'id', ideaId] })
      qc.invalidateQueries({ queryKey: ['ideas', 'slug', ideaId] })
      toast.success('Saved to your following list.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}