import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { entrepreneurApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export function useCreateInquiry(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => entrepreneurApi.createInquiry(ideaId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inquiries'] })
      qc.invalidateQueries({ queryKey: ['matching'] })
      toast.success('Inquiry sent. The creator will be notified.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useInquiriesReceived(params) {
  return useQuery({
    queryKey: ['inquiries', 'received', params],
    queryFn: () => entrepreneurApi.inquiriesReceived(params),
    placeholderData: (prev) => prev,
  })
}

export function useInquiriesSent(params) {
  return useQuery({
    queryKey: ['inquiries', 'sent', params],
    queryFn: () => entrepreneurApi.inquiriesSent(params),
    placeholderData: (prev) => prev,
  })
}

export function useRespondInquiry() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => entrepreneurApi.respondInquiry(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inquiries'] })
      toast.success('Response sent.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateInquiry() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => entrepreneurApi.updateInquiry(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inquiries'] })
      toast.success('Inquiry updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteInquiry() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => entrepreneurApi.deleteInquiry(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inquiries'] })
      toast.success('Inquiry withdrawn.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useWatchlist(params) {
  return useQuery({
    queryKey: ['watchlist', params],
    queryFn: () => entrepreneurApi.watchlist(params),
    placeholderData: (prev) => prev,
  })
}

export function useAddToWatchlist() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (ideaId) => entrepreneurApi.addToWatchlist(ideaId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['watchlist'] })
      toast.success('Added to your watchlist.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useRemoveFromWatchlist() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (ideaId) => entrepreneurApi.removeFromWatchlist(ideaId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['watchlist'] })
      toast.success('Removed from your watchlist.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useMatches(params) {
  return useQuery({
    queryKey: ['matching', params],
    queryFn: () => entrepreneurApi.matches(params),
    staleTime: 5 * 60_000,
    placeholderData: (prev) => prev,
  })
}

export function useRunMatching() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (params) => entrepreneurApi.matches(params),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['matching'] })
      toast.success('Recommendations refreshed.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}