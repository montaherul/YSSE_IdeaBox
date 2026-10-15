import {
  BarChart3,
  Bell,
  Bookmark,
  BookOpen,
  CalendarDays,
  Compass,
  FileText,
  Flag,
  FolderTree,
  Inbox,
  LayoutDashboard,
  Lightbulb,
  MessageSquare,
  Search,
  ShieldCheck,
  SlidersHorizontal,
  Star,
  Store,
  Tag,
  Trophy,
  Users,
} from 'lucide-react'

/**
 * Navigation definition.
 *
 * Visibility is resolved as a union of role access (first) and permission access (second):
 *   - `group.audience` sets the base role audience for the group:
 *       workspace  -> members and moderators (never a full administrator)
 *       member     -> members only (Discover / Connect)
 *       moderator  -> moderators and screening reviewers
 *       admin      -> administrators only
 *   - `item.roles`       (optional) role keys resolved by `usePermission().can()`
 *   - `item.permissions` (optional) permission codes resolved by `usePermission().canPermission()`
 *
 * An item that declares `roles` or `permissions` is shown when the user holds one of
 * them; otherwise the group audience decides. Because an administrator holds every
 * permission code, an admin sees every module without enumerating roles.
 *
 * This only controls UI visibility. The .NET Web API remains the authoritative
 * authorization boundary for every protected operation.
 */
export const navigationConfig = [
  {
    group: 'Workspace',
    audience: 'workspace',
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
    audience: 'member',
    items: [
      { to: '/ideas', label: 'Discover ideas', icon: Search },
      { to: '/problems', label: 'Problems', icon: Users },
      { to: '/leaderboard', label: 'Leaderboard', icon: Trophy },
      { to: '/marketplace', label: 'Marketplace', icon: Store },
    ],
  },
  {
    group: 'Connect',
    audience: 'member',
    items: [
      { to: '/inquiries', label: 'Inquiries', icon: Inbox },
      { to: '/inbox', label: 'Messages', icon: MessageSquare },
      { to: '/opportunities', label: 'Entrepreneur portal', icon: Compass, roles: ['entrepreneur'] },
    ],
  },
  {
    group: 'Moderation',
    audience: 'moderator',
    items: [
      { to: '/review/queue', label: 'Review queue', icon: MessageSquare, permissions: ['screening.read'] },
      { to: '/admin/moderation', label: 'Moderation', icon: Flag, permissions: ['reports.read'] },
    ],
  },
  {
    group: 'Administration',
    audience: 'admin',
    items: [
      { to: '/admin', label: 'Admin dashboard', icon: BarChart3, end: true, permissions: ['dashboard.read'] },
      { to: '/users', label: 'Users', icon: ShieldCheck, permissions: ['users.read'] },
      { to: '/admin/roles', label: 'Roles', icon: Users, permissions: ['roles.read'] },
      { to: '/admin/permissions', label: 'Permissions', icon: ShieldCheck, permissions: ['permissions.read'] },
      { to: '/admin/entrepreneurs', label: 'Entrepreneurs', icon: Users, permissions: ['inquiries.read'] },
      { to: '/admin/categories', label: 'Categories', icon: FolderTree, permissions: ['categories.read'] },
      { to: '/admin/tags', label: 'Tags', icon: Tag, permissions: ['tags.read'] },
      { to: '/admin/content', label: 'Site content', icon: FileText, permissions: ['staticcontent.read'] },
      { to: '/admin/voting', label: 'Voting & sessions', icon: CalendarDays, permissions: ['votingperiods.read'] },
      { to: '/admin/featured', label: 'Featured ideas', icon: Star, permissions: ['ideas.feature'] },
      { to: '/admin/audit', label: 'Audit & reports', icon: BookOpen, permissions: ['audit.read'] },
      { to: '/admin/settings', label: 'Settings', icon: SlidersHorizontal, permissions: ['settings.read'] },
    ],
  },
]

export default navigationConfig
