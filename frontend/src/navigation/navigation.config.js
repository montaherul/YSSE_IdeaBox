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
 * Visibility is resolved in two steps: the group audience gates the section first, then
 * any item-level role/permission requirement filters within it:
 *   - `group.audience` sets the base role audience for the group:
 *       workspace  -> members and moderators (never a full administrator)
 *       member     -> members only (Discover / Connect)
 *       moderator  -> moderators, screening reviewers and administrators
 *       admin      -> administrators only
 *   - `item.roles`       (optional) role keys resolved by `usePermission().can()`
 *   - `item.permissions` (optional) permission codes resolved by `usePermission().canPermission()`
 *
 * An item is shown only when its group audience matches AND (when declared) the user
 * holds one of its roles/permissions. Requiring the audience as well prevents generic
 * member read permissions (e.g. `categories.read`) from revealing admin-only sections.
 * Because an administrator holds every permission code and matches the staff audiences,
 * an admin sees every staff module without enumerating roles.
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
      { to: '/app/ideas', label: 'My ideas', icon: Lightbulb, permissions: ['ideas.read'] },
      { to: '/me/reviews', label: 'My reviews', icon: MessageSquare, permissions: ['reviews.read'] },
      { to: '/me/following', label: 'Following', icon: Bookmark, permissions: ['profiles.read'] },
      { to: '/app/notifications', label: 'Notifications', icon: Bell, permissions: ['notifications.read'] },
    ],
  },
  {
    group: 'Discover',
    audience: 'member',
    items: [
      { to: '/ideas', label: 'Discover ideas', icon: Search, permissions: ['ideas.read'] },
      { to: '/problems', label: 'Problems', icon: Users, permissions: ['problems.read'] },
      { to: '/leaderboard', label: 'Leaderboard', icon: Trophy, permissions: ['voting.read'] },
      { to: '/marketplace', label: 'Marketplace', icon: Store, permissions: ['marketplace.read'] },
    ],
  },
  {
    group: 'Connect',
    audience: 'member',
    items: [
      { to: '/inquiries', label: 'Inquiries', icon: Inbox, permissions: ['inquiries.read'] },
      { to: '/inbox', label: 'Messages', icon: MessageSquare, permissions: ['messages.read'] },
      { to: '/opportunities', label: 'Entrepreneur portal', icon: Compass, roles: ['entrepreneur'] },
    ],
  },
  {
    group: 'Moderation',
    audience: 'moderator',
    items: [
      { to: '/review/queue', label: 'Review queue', icon: MessageSquare, permissions: ['screening.read'] },
      { to: '/admin/moderation', label: 'Moderation', icon: Flag, permissions: ['content.reports.read'] },
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
      { to: '/admin/entrepreneurs', label: 'Entrepreneurs', icon: Users, roles: ['admin'] },
      { to: '/admin/categories', label: 'Categories', icon: FolderTree, roles: ['admin'] },
      { to: '/admin/tags', label: 'Tags', icon: Tag, roles: ['admin'] },
      { to: '/admin/content', label: 'Site content', icon: FileText, permissions: ['staticcontent.read'] },
      { to: '/admin/voting', label: 'Voting & sessions', icon: CalendarDays, permissions: ['votingperiods.read'] },
      { to: '/admin/featured', label: 'Featured ideas', icon: Star, permissions: ['ideas.feature'] },
      { to: '/admin/audit', label: 'Audit & reports', icon: BookOpen, permissions: ['audit.read'] },
      { to: '/admin/settings', label: 'Settings', icon: SlidersHorizontal, permissions: ['settings.read'] },
    ],
  },
]

export default navigationConfig
