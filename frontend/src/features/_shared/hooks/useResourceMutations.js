import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { toErrorMessage } from '@/lib/validation'

/**
 * Generic CRUD mutation hook factory.
 *
 * Every feature currently repeats the same `useCreateX` / `useUpdateX` /
 * `useDeleteX` blocks (invalidate + success toast + error toast). This factory
 * captures that pattern once.
 *
 * Usage:
 *   const useTagMutations = createResourceMutations({
 *     createFn: (payload) => adminApi.createTag(payload),
 *     updateFn: ({ id, payload }) => adminApi.updateTag(id, payload),
 *     deleteFn: (id) => adminApi.deleteTag(id),
 *     invalidateKeys: [['admin', 'tags']],
 *     messages: { create: 'Tag created.', update: 'Tag updated.', delete: 'Tag deleted.' },
 *   })
 *   const { create, update, remove } = useTagMutations()
 */
export function createResourceMutations({
  createFn,
  updateFn,
  deleteFn,
  invalidateKeys = [],
  messages = {},
}) {
  return function useResourceMutations() {
    const qc = useQueryClient()

    const invalidateAll = () => {
      invalidateKeys.forEach((key) => qc.invalidateQueries({ queryKey: key }))
    }
    const onError = (error) => {
      if (!error?.status) toast.error(toErrorMessage(error))
    }

    const create = useMutation({
      mutationFn: createFn,
      onSuccess: () => {
        invalidateAll()
        if (messages.create) toast.success(messages.create)
      },
      onError,
    })

    const update = useMutation({
      mutationFn: updateFn,
      onSuccess: () => {
        invalidateAll()
        if (messages.update) toast.success(messages.update)
      },
      onError,
    })

    const remove = useMutation({
      mutationFn: deleteFn,
      onSuccess: () => {
        invalidateAll()
        if (messages.delete) toast.success(messages.delete)
      },
      onError,
    })

    return { create, update, remove }
  }
}

export default createResourceMutations
