import { useEffect } from 'react'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { toast } from 'sonner'
import { queryClient } from '@/app/query-client'
import { useAuthStore } from '@/store/auth-store'
import { API_BASE_URL } from '@/config/constants'

function resolveHubUrl() {
  return API_BASE_URL.replace(/\/api\/v1\/?$/, '') + '/hubs/notifications'
}

/**
 * Live notification channel. Connects to the SignalR hub while a session exists,
 * invalidates TanStack Query notification data and shows a toast when the server
 * pushes a "notification" event. Reconnects automatically via SignalR.
 */
export function useNotificationHub() {
  useEffect(() => {
    const { accessToken, isAuthenticated } = useAuthStore.getState()
    if (!accessToken || !isAuthenticated) return undefined

    const connection = new HubConnectionBuilder()
      .withUrl(resolveHubUrl(), {
        withCredentials: true,
        accessTokenFactory: () => {
          const state = useAuthStore.getState()
          return Promise.resolve(state.accessToken ?? '')
        },
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    const onNotification = (payload) => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      const message = payload?.body || payload?.title
      if (message) {
        toast(payload?.title ?? 'New notification', { description: message })
      }
    }

    connection.on('notification', onNotification)

    connection
      .start()
      .catch(() => {
        // Backend may be offline or unreachable during dev/test; ignore and retry.
      })

    return () => {
      connection.off('notification', onNotification)
      connection.stop().catch(() => {})
    }
  }, [])
}

/** Mount this once inside the app providers to enable live notifications. */
export function NotificationHub() {
  useNotificationHub()
  return null
}