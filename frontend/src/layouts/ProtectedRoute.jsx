import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { usePermission } from '@/hooks/usePermission'

/**
 * Route guard: requires authentication and, optionally, a role or permission.
 * Role access is evaluated first, then permission access (union). Server-side
 * authorization remains the real security boundary.
 */
export function ProtectedRoute({ roles, permissions, children }) {
  const { isAuthenticated } = useAuth()
  const { canAccess } = usePermission()
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location.pathname + (location.search || '') }} replace />
  }

  if (!canAccess({ roles, permissions })) {
    return <Navigate to="/" replace />
  }

  return children ?? <Outlet />
}

/** Guard for the admin/moderator/reviewer admin area. */
export function AdminRoute({ children }) {
  const { canAccessAdmin } = usePermission()
  if (!canAccessAdmin()) return <Navigate to="/" replace />
  return children ?? <Outlet />
}
