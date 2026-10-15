import { useQuery } from '@tanstack/react-query'
import api from '@/services/apiClient'

export function usePlatformSettings() {
  return useQuery({
    queryKey: ['platform', 'settings'],
    queryFn: () => api.get('/settings'),
    staleTime: 5 * 60_000,
  })
}

export function usePrimaryColor() {
  const { data, isLoading } = usePlatformSettings()
  const primaryColor = data?.['Theme.PrimaryColor'] || '#2db97c'
  return { primaryColor, isLoading }
}