import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { problemsApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export function useProblemList(params) {
  return useQuery({
    queryKey: ['problems', 'list', params],
    queryFn: () => problemsApi.list(params),
    placeholderData: (prev) => prev,
  })
}

export function useProblem(id) {
  return useQuery({
    queryKey: ['problems', id],
    queryFn: () => problemsApi.byId(id),
    enabled: Boolean(id),
    retry: false,
  })
}

export function useCreateProblem() {
  const qc = useQueryClient()
  const invalidate = () => qc.invalidateQueries({ queryKey: ['problems'] })
  return useMutation({
    mutationFn: (payload) => problemsApi.create(payload),
    onSuccess: invalidate,
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateProblem() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => problemsApi.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['problems'] })
      toast.success('Problem updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteProblem() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => problemsApi.remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['problems'] })
      toast.success('Problem deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useEndorseProblem() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => problemsApi.endorse(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['problems'] })
      toast.success('You endorsed this problem.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useChangeProblemStatus() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => problemsApi.status(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['problems'] })
      toast.success('Problem status updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useLinkIdeaToProblem() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => problemsApi.link(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['problems'] })
      toast.success('Idea linked to the problem.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}