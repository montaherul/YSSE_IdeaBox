import { NavLink, Link } from 'react-router-dom'
import { Plus, Settings } from 'lucide-react'
import { cn } from '@/lib/utils'
import { usePermission } from '@/hooks/usePermission'
import { APP_NAME } from '@/config/constants'
import { navigationConfig } from '@/navigation/navigation.config'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { Logo } from '@/components/shared/Logo'

export function Sidebar({ onNavigate }) {
  const { can, canAccess } = usePermission()

  const isAdmin = can('admin')
  const isModerator = can('moderator') || can('reviewer')

  // Administrators match the staff audiences, so they see every staff module without
  // enumerating roles. Member/workspace sections remain hidden from administrators.
  const audienceMatches = (audience) => {
    if (audience === 'admin') return isAdmin
    if (audience === 'moderator') return isModerator || isAdmin
    if (audience === 'workspace') return !isAdmin
    if (audience === 'member') return !isAdmin
    return false
  }

  // The group audience gates the whole section first; an item that also declares
  // roles/permissions is then shown only when the user holds one of them. This keeps
  // member-only read permissions (e.g. categories.read) from surfacing admin sections.
  const isItemVisible = (group, item) => {
    if (!audienceMatches(group.audience)) return false
    const hasRequirements = (item.roles?.length ?? 0) > 0 || (item.permissions?.length ?? 0) > 0
    if (!hasRequirements) return true
    return canAccess({ roles: item.roles, permissions: item.permissions })
  }

  const visibleGroups = navigationConfig
    .map((group) => ({ group, items: group.items.filter((item) => isItemVisible(group, item)) }))
    .filter(({ items }) => items.length > 0)

  return (
    <div className="flex h-full flex-col gap-4 overflow-y-auto p-3">
      <Link to="/" className="flex items-center gap-2 px-2 py-2" onClick={onNavigate}>
        <Logo size="md" />
        <span className="text-base font-bold tracking-tight">{APP_NAME}</span>
      </Link>

      {!isAdmin ? (
        <Button asChild className="w-full justify-start" size="sm">
          <Link to="/app/submissions/new" onClick={onNavigate}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            New idea
          </Link>
        </Button>
      ) : null}

      {visibleGroups.map(({ group, items }) => (
        <nav key={group.group} aria-label={group.group} className="space-y-0.5">
          <p className="px-3 pb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
            {group.group}
          </p>
          {items.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              onClick={onNavigate}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                  isActive && 'bg-accent text-accent-foreground',
                )
              }
            >
              <item.icon className="h-4 w-4 shrink-0" aria-hidden="true" />
              {item.label}
            </NavLink>
          ))}
        </nav>
      ))}

      <nav aria-label="Account" className="space-y-0.5">
        <p className="px-3 pb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
          Account
        </p>
        <NavLink
          to="/app/profile"
          onClick={onNavigate}
          className={({ isActive }) =>
            cn(
              'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
              isActive && 'bg-accent text-accent-foreground',
            )
          }
        >
          <Settings className="h-4 w-4 shrink-0" aria-hidden="true" />
          Settings
        </NavLink>
      </nav>

      <Separator className="my-1" />
      <div className="space-y-0.5 px-1">
        <Link to="/about" onClick={onNavigate} className="block rounded px-2 py-1.5 text-sm text-muted-foreground hover:text-foreground">
          About
        </Link>
        <Link to="/guidelines" onClick={onNavigate} className="block rounded px-2 py-1.5 text-sm text-muted-foreground hover:text-foreground">
          Guidelines
        </Link>
        <Link to="/contact" onClick={onNavigate} className="block rounded px-2 py-1.5 text-sm text-muted-foreground hover:text-foreground">
          Contact
        </Link>
      </div>
    </div>
  )
}
