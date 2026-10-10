import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { usePermission } from '@/hooks/usePermission'

/**
 * Route guard: requires authentication (and optionally specific roles).
 * Server-side authorization remains the real security boundary.
 */
export function ProtectedRoute({ roles, children }) {
  const { isAuthenticated } = useAuth()
  const { can } = usePermission()
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location.pathname + (location.search || '') }} replace />
  }

  if (roles && roles.length > 0) {
    const allowed = roles.some((role) => can(role))
    if (!allowed) {
      return <Navigate to="/" replace />
    }
  }

  return children ?? <Outlet />
}

/** Guard for admin/moderator/reviewer (lazy admin area). */
export function AdminRoute({ children }) {
  const { canAccessAdmin } = usePermission()
  if (!canAccessAdmin()) return <Navigate to="/" replace />
  return children ?? <Outlet />
}