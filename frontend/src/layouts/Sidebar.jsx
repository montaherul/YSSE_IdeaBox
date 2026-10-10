import { NavLink, Link } from 'react-router-dom'
import {
  Bell,
  Inbox,
  LayoutDashboard,
  Lightbulb,
  Plus,
  Search,
  ShieldCheck,
  Store,
  Trophy,
  Settings,
  MessageSquare,
  Bookmark,
  Compass,
  Users,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { usePermission } from '@/hooks/usePermission'
import { APP_NAME } from '@/config/constants'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'

const NAV_ITEMS = [
  {
    group: 'Workspace',
    items: [
      { to: '/app', label: 'Dashboard', icon: LayoutDashboard },
      { to: '/app/ideas', label: 'My ideas', icon: Lightbulb },
      { to: '/me/reviews', label: 'My reviews', icon: MessageSquare },
      { to: '/me/following', label: 'Following', icon: Bookmark },
      { to: '/app/notifications', label: 'Notifications', icon: Bell },
    ],
  },
  {
    group: 'Discover',
    items: [
      { to: '/ideas', label: 'Discover ideas', icon: Search },
      { to: '/problems', label: 'Problems', icon: Users },
      { to: '/leaderboard', label: 'Leaderboard', icon: Trophy },
      { to: '/marketplace', label: 'Marketplace', icon: Store },
    ],
  },
  {
    group: 'Connect',
    items: [
      { to: '/inquiries', label: 'Inquiries', icon: Inbox },
      { to: '/inbox', label: 'Messages', icon: MessageSquare },
      { to: '/opportunities', label: 'Entrepreneur portal', icon: Compass },
    ],
  },
]

export function Sidebar({ onNavigate }) {
  const { can } = usePermission()
  const { canAccessAdmin } = usePermission()
  const showAdmin = canAccessAdmin()
  const showEntrepreneur = can('entrepreneur')

  return (
    <div className="flex h-full flex-col gap-4 overflow-y-auto p-3">
      <Link to="/" className="flex items-center gap-2 px-2 py-2" onClick={onNavigate}>
        <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
          <Lightbulb className="h-4 w-4" aria-hidden="true" />
        </span>
        <span className="text-base font-bold tracking-tight">{APP_NAME}</span>
      </Link>

      <Button asChild className="w-full justify-start" size="sm">
        <Link to="/app/submissions/new" onClick={onNavigate}>
          <Plus className="h-4 w-4" aria-hidden="true" />
          New idea
        </Link>
      </Button>

      {NAV_ITEMS.map((group) => {
        const items = group.items.filter(
          (item) => item.to !== '/opportunities' || showEntrepreneur,
        )
        if (items.length === 0) return null
        return (
          <nav key={group.group} aria-label={group.group} className="space-y-0.5">
            <p className="px-3 pb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              {group.group}
            </p>
            {items.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
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
        )
      })}

      <nav aria-label="Account" className="space-y-0.5">
        <p className="px-3 pb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
          Account
        </p>
        {showAdmin ? (
          <NavLink
            to="/admin"
            onClick={onNavigate}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                isActive && 'bg-accent text-accent-foreground',
              )
            }
          >
            <ShieldCheck className="h-4 w-4 shrink-0" aria-hidden="true" />
            Administration
          </NavLink>
        ) : null}
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