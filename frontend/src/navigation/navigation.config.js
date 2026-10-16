import {
  BarChart3,
  Bell,
  Bookmark,
  BookOpen,
  Briefcase,
  CalendarDays,
  Compass,
  FileText,
  Flag,
  FolderKanban,
  FolderTree,
  Inbox,
  LayoutDashboard,
  Lightbulb,
  ListChecks,
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
 * Role-scoped navigation definition.
 *
 * The sidebar is built from this configuration rather than scattering role checks
 * through components (SRS dashboard rule: configurable navigation).
 *
 * Each group declares an `audience`. The Sidebar resolves the current user's roles
 * into an audience set and renders only the matching groups:
 *   - member    -> registered member / idea creator (not staff, not a seeker)
 *   - seeker    -> Entrepreneur / Investor / Mentor (opportunity seekers)
 *   - browse    -> every authenticated non-administrator (shared discovery links)
 *   - moderator -> Moderator, Screening Reviewer and Administrator (staff)
 *   - admin     -> Administrator only
 *
 * Within a group, an item may additionally declare `roles` (resolved by
 * `usePermission().can()`) and/or `permissions` (resolved by `canPermission()`), and is
 * only shown when the user holds one of them. Administrators hold every permission code,
 * so they see every staff module without enumerating roles.
 *
 * This only controls UI visibility. The .NET Web API remains the authoritative
 * authorization boundary for every protected operation.
 *
 * Note: a few SRS sidebar entries (Recommended for Me, Opportunity Pipeline, Verification
 * Status, Outcome Reports, My Votes, appeal/history screens, etc.) have no page yet and
 * are intentionally omitted rather than linked to a non-existent route.
 */
export const navigationConfig = [
  {
    group: 'Workspace',
    audience: 'member',
    items: [
      { to: '/app', label: 'Dashboard', icon: LayoutDashboard },
      { to: '/app/ideas', label: 'My ideas', icon: Lightbulb, permissions: ['ideas.read'] },
      { to: '/me/reviews', label: 'My reviews', icon: MessageSquare, permissions: ['reviews.read'] },
      { to: '/me/following', label: 'Following', icon: Bookmark, permissions: ['profiles.read'] },
      { to: '/marketplace/mine', label: 'My listings', icon: Store, permissions: ['marketplace.read'] },
      { to: '/app/notifications', label: 'Notifications', icon: Bell, permissions: ['notifications.read'] },
    ],
  },
  {
    group: 'My Opportunities',
    audience: 'seeker',
    items: [
      { to: '/app', label: 'Dashboard', icon: LayoutDashboard },
      { to: '/opportunities', label: 'Discover opportunities', icon: Compass },
      { to: '/inquiries', label: 'My inquiries', icon: Inbox, permissions: ['inquiries.read'] },
      { to: '/inbox', label: 'Messages', icon: MessageSquare, permissions: ['messages.read'] },
      { to: '/me/following', label: 'Saved ideas', icon: Bookmark, permissions: ['profiles.read'] },
      { to: '/entrepreneur/profile', label: 'Business profile', icon: Briefcase, permissions: ['profiles.read'] },
      { to: '/app/notifications', label: 'Notifications', icon: Bell, permissions: ['notifications.read'] },
    ],
  },
  {
    group: 'Discover',
    audience: 'browse',
    items: [
      { to: '/ideas', label: 'Discover ideas', icon: Search, permissions: ['ideas.read'] },
      { to: '/problems', label: 'Problems', icon: Users, permissions: ['problems.read'] },
      { to: '/leaderboard', label: 'Leaderboard', icon: Trophy, permissions: ['voting.read'] },
      { to: '/marketplace', label: 'Marketplace', icon: Store, permissions: ['marketplace.read'] },
    ],
  },
  {
    group: 'Moderation',
    audience: 'moderator',
    items: [
      { to: '/admin/moderation', label: 'Moderation queue', icon: Flag, permissions: ['content.reports.read'] },
      { to: '/review/queue', label: 'Review queue', icon: ListChecks, permissions: ['screening.read'] },
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
      { to: '/admin/projects', label: 'Project archive', icon: FolderKanban, permissions: ['projects.manage'] },
      { to: '/admin/audit', label: 'Audit & reports', icon: BookOpen, permissions: ['audit.read'] },
      { to: '/admin/settings', label: 'Settings', icon: SlidersHorizontal, permissions: ['settings.read'] },
    ],
  },
]

export default navigationConfig
