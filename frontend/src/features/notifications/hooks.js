import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { notificationsApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export function useNotifications(params) {
  return useQuery({
    queryKey: ['notifications', 'list', params],
    queryFn: () => notificationsApi.list(params),
    placeholderData: (prev) => prev,
    refetchInterval: 60_000,
  })
}

export function useMarkNotificationRead() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => notificationsApi.markRead(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['notifications'] }),
    retry: false,
  })
}

export function useMarkAllNotificationsRead() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => notificationsApi.markAllRead(),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['notifications'] })
      toast.success('All notifications marked as read.')
    },
    retry: false,
  })
}

export function useNotificationPreferences() {
  return useQuery({
    queryKey: ['notifications', 'preferences'],
    queryFn: () => notificationsApi.preferences(),
    staleTime: 60_000,
  })
}

export function useUpdateNotificationPreference() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => notificationsApi.updatePreference(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['notifications', 'preferences'] })
      toast.success('Preferences saved.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}