import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { categoriesApi } from './api'
import { toErrorMessage } from '@/lib/validation'
import { toast } from 'sonner'

export function useCategories(enabled = true) {
  return useQuery({
    queryKey: ['categories'],
    queryFn: () => categoriesApi.list(),
    enabled,
    staleTime: 5 * 60_000,
  })
}

export function useTags(enabled = true) {
  return useQuery({
    queryKey: ['tags'],
    queryFn: () => categoriesApi.tags(),
    enabled,
    staleTime: 5 * 60_000,
  })
}

export function useCreateCategory() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => categoriesApi.create(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['categories'] })
      toast.success('Category created.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateCategory() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => categoriesApi.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['categories'] })
      toast.success('Category updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteCategory() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => categoriesApi.remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['categories'] })
      toast.success('Category deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}