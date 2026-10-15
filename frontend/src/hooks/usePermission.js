import { useAuth } from './useAuth'

const ROLE_ALIASES = {
  admin: ['Administrator', 'PlatformAdministrator', 'Admin'],
  moderator: ['Moderator'],
  reviewer: ['ScreeningReviewer', 'Screening Reviewer'],
  entrepreneur: ['Entrepreneur', 'Investor', 'Mentor'],
}

function normalizeRoles(roles = []) {
  return roles.map((r) => String(r))
}

function normalizePermissions(permissions = []) {
  return permissions.map((p) => String(p).toLowerCase())
}

/**
 * Access checks for guarded routes, navigation and conditional UI.
 *
 * Two mechanisms are supported and combined as a union (role access first, then
 * permission access): a user is granted when they hold one of the required roles
 * OR one of the required permission codes. An administrator holds every permission
 * code, so an admin sees every module without enumerating roles.
 *
 * This only drives UI visibility. The .NET Web API remains the authoritative
 * authorization boundary for every protected operation.
 */
export function usePermission() {
  const { user, isAuthenticated } = useAuth()
  const roles = normalizeRoles(user?.roles)
  const permissions = normalizePermissions(user?.permissions)

  const can = (roleKey) => {
    if (!isAuthenticated) return false
    const candidates = ROLE_ALIASES[roleKey] || [roleKey]
    return roles.some((r) =>
      candidates.some((c) => r.toLowerCase() === c.toLowerCase() || r.includes(c) || c.includes(r)),
    )
  }

  const canPermission = (code) => {
    if (!isAuthenticated || !code) return false
    return permissions.includes(String(code).toLowerCase())
  }

  const canAnyPermission = (codes = []) => {
    if (!Array.isArray(codes) || codes.length === 0) return false
    return codes.some((code) => canPermission(code))
  }

  /**
   * Combines role-based and permission-based requirements.
   * Role access is evaluated first; if it fails, permission access is evaluated.
   * When neither requirement is supplied the user only needs to be authenticated.
   */
  const canAccess = ({ roles: requiredRoles, permissions: requiredPermissions } = {}) => {
    if (!isAuthenticated) return false
    const hasRoleRequirement = Array.isArray(requiredRoles) && requiredRoles.length > 0
    const hasPermissionRequirement = Array.isArray(requiredPermissions) && requiredPermissions.length > 0

    if (!hasRoleRequirement && !hasPermissionRequirement) return true
    if (hasRoleRequirement && requiredRoles.some((role) => can(role))) return true
    if (hasPermissionRequirement && requiredPermissions.some((code) => canPermission(code))) return true
    return false
  }

  const canAccessAdmin = () => can('admin') || can('moderator') || can('reviewer')
  const canSendInquiries = () => can('entrepreneur')

  return {
    isAuthenticated,
    roles,
    permissions,
    can,
    canPermission,
    canAnyPermission,
    canAccess,
    canAccessAdmin,
    canSendInquiries,
  }
}

export default usePermission
