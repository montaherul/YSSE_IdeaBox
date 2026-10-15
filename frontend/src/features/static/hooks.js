import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { staticApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export function useStaticContent(key) {
  return useQuery({
    queryKey: ['static', key],
    queryFn: () => staticApi.getByKey(key),
    enabled: !!key,
    staleTime: 5 * 60_000,
  })
}

function invalidateStaticContent(qc) {
  qc.invalidateQueries({ queryKey: ['admin', 'static-content'] })
  qc.invalidateQueries({ queryKey: ['static'] })
}

export function useCreateStaticContent() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => staticApi.create(payload),
    onSuccess: () => {
      invalidateStaticContent(qc)
      toast.success('Content created.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateStaticContent() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => staticApi.update(id, payload),
    onSuccess: () => {
      invalidateStaticContent(qc)
      toast.success('Content updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteStaticContent() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => staticApi.delete(id),
    onSuccess: () => {
      invalidateStaticContent(qc)
      toast.success('Content deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}