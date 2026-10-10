import { useShallow } from 'zustand/react/shallow'
import { useAuthStore } from '@/store/auth-store'

/** Auth hook: exposes the current session and auth actions. */
export function useAuth() {
  const { user, accessToken, isAuthenticated } = useAuthStore(
    useShallow((s) => ({
      user: s.user,
      accessToken: s.accessToken,
      isAuthenticated: s.isAuthenticated,
    })),
  )

  const setAuth = useAuthStore((s) => s.setAuth)
  const setUser = useAuthStore((s) => s.setUser)
  const logout = useAuthStore((s) => s.logout)

  return { user, accessToken, isAuthenticated, setAuth, setUser, logout }
}

export default useAuth