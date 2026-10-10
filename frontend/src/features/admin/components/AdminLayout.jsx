import { NavLink, Outlet } from 'react-router-dom'
import {
  BarChart3,
  BookOpen,
  CalendarDays,
  Flag,
  FolderTree,
  ShieldCheck,
  SlidersHorizontal,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { PageHeader } from '@/components/shared/PageHeader'
import { Separator } from '@/components/ui/separator'

const ADMIN_NAV = [
  { to: '/admin', label: 'Dashboard', icon: BarChart3, end: true },
  { to: '/admin/users', label: 'Users & roles', icon: ShieldCheck },
  { to: '/admin/ideas', label: 'Moderation', icon: Flag },
  { to: '/admin/categories', label: 'Categories', icon: FolderTree },
  { to: '/admin/sessions', label: 'Sessions', icon: CalendarDays },
  { to: '/admin/reports', label: 'Reports', icon: BookOpen },
  { to: '/admin/settings', label: 'Settings', icon: SlidersHorizontal },
]

export default function AdminLayout() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Administration"
        description="Platform moderation and configuration. Server-side permissions still apply to every action."
      />

      <nav aria-label="Administration" className="flex flex-wrap gap-1.5">
        {ADMIN_NAV.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            className={({ isActive }) =>
              cn(
                'inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                isActive && 'bg-accent text-accent-foreground',
              )
            }
          >
            <item.icon className="h-4 w-4" aria-hidden="true" />
            {item.label}
          </NavLink>
        ))}
      </nav>

      <Separator />

      <Outlet />
    </div>
  )
}