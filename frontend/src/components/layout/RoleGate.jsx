import { usePermission } from '@/hooks/usePermission'

/**
 * Conditionally renders children only when the current user holds one of
 * the required roles. UI hiding is a UX convenience — the server still
 * enforces authorization.
 */
export function RoleGate({ roles, fallback = null, children }) {
  const { can } = usePermission()
  const allowed = !roles || roles.length === 0 || roles.some((role) => can(role))
  return allowed ? children : fallback
}

export default RoleGate
