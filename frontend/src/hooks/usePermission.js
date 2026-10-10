import { useAuth } from './useAuth'

const ROLE_ALIASES = {
  admin: ['Administrator', 'PlatformAdministrator', 'Admin'],
  moderator: ['Moderator'],
  reviewer: ['ScreeningReviewer', 'Screening Reviewer'],
  entrepreneur: ['Entrepreneur', 'Investor', 'Mentor'],
  helper: ['IdeaHelper'],
  languageEditor: ['LanguageEditor'],
}

function normalizeRoles(roles = []) {
  return roles.map((r) => String(r))
}

/** Role checks for guarded routes and conditional UI (server still enforces). */
export function usePermission() {
  const { user, isAuthenticated } = useAuth()
  const roles = normalizeRoles(user?.roles)

  const can = (roleKey) => {
    if (!isAuthenticated) return false
    const candidates = ROLE_ALIASES[roleKey] || [roleKey]
    return roles.some((r) =>
      candidates.some((c) => r.toLowerCase() === c.toLowerCase() || r.includes(c) || c.includes(r)),
    )
  }

  const canAccessAdmin = () => can('admin') || can('moderator') || can('reviewer')
  const canSendInquiries = () => can('entrepreneur')

  return { isAuthenticated, roles, can, canAccessAdmin, canSendInquiries }
}