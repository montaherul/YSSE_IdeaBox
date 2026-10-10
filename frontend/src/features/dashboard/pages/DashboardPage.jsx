import { Link } from 'react-router-dom'
import {
  Bell,
  Bookmark,
  Compass,
  Inbox,
  Lightbulb,
  MessageSquare,
  Plus,
  Sparkles,
} from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { usePermission } from '@/hooks/usePermission'
import { useMyIdeas } from '@/features/ideas/hooks'
import { useNotifications } from '@/features/notifications/hooks'
import { useInquiriesReceived } from '@/features/entrepreneur/hooks'
import { useMemberProfile } from '@/features/profile/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { BoundaryNotice } from '@/components/shared/BoundaryNotice'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { formatCompact } from '@/lib/format'

const QUICK_LINKS = [
  { to: '/ideas', label: 'Discover ideas', icon: Sparkles },
  { to: '/leaderboard', label: 'Leaderboard', icon: Compass },
  { to: '/marketplace', label: 'Marketplace', icon: Lightbulb },
]

export default function DashboardPage() {
  useDocumentTitle('Dashboard')
  const { user } = useAuth()
  const { can } = usePermission()
  const myIdeas = useMyIdeas({ page: 1, pageSize: 1 })
  const notifications = useNotifications({ page: 1, pageSize: 5 })
  const inquiries = useInquiriesReceived({ page: 1, pageSize: 5 })
  const profile = useMemberProfile()

  const ideaTotal = myIdeas.data?.total ?? 0
  const unreadCount = (notifications.data?.items ?? []).filter((n) => !n.isRead).length
  const pendingInquiries = (inquiries.data?.items ?? []).filter((i) => i.status === 'Pending' || i.status === 'Requested').length

  const stats = [
    { label: 'My ideas', value: formatCompact(ideaTotal), to: '/app/ideas' },
    { label: 'Unread notifications', value: String(unreadCount), to: '/app/notifications' },
    { label: 'Open inquiries', value: String(pendingInquiries), to: '/inquiries' },
  ]

  return (
    <div className="space-y-6">
      <PageHeader
        title={`Hello, ${user?.displayName?.split(' ')[0] ?? 'member'}`}
        description={profile.data?.biography ? 'Welcome back to your Idea Box workspace.' : 'Welcome to YSSE Idea Box — share ideas, get feedback, and find support.'}
        actions={
          <Button asChild>
            <Link to="/app/submissions/new">
              <Plus className="h-4 w-4" aria-hidden="true" />
              New idea
            </Link>
          </Button>
        }
      />

      <div className="grid gap-4 sm:grid-cols-3">
        {stats.map((stat) => (
          <Link key={stat.to} to={stat.to} className="rounded-xl border bg-card p-5 shadow-sm transition-shadow hover:shadow-md">
            <p className="text-2xl font-bold">{stat.value}</p>
            <p className="mt-1 text-sm text-muted-foreground">{stat.label}</p>
          </Link>
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Bell className="h-4 w-4" aria-hidden="true" />
              Recent notifications
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {(notifications.data?.items ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">No notifications yet.</p>
            ) : (
              (notifications.data?.items ?? []).map((n) => (
                <div key={n.id} className="flex items-start gap-2 text-sm">
                  <span className={`mt-1 h-2 w-2 shrink-0 rounded-full ${n.isRead ? 'bg-muted-foreground/40' : 'bg-primary'}`} aria-hidden="true" />
                  <p className="text-muted-foreground">{n.title ?? n.body}</p>
                </div>
              ))
            )}
            <Link to="/app/notifications" className="inline-block text-sm text-primary hover:underline">
              View all notifications
            </Link>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <MessageSquare className="h-4 w-4" aria-hidden="true" />
              Quick actions
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {QUICK_LINKS.map((link) => (
              <Link key={link.to} to={link.to} className="flex items-center gap-3 rounded-lg border p-3 text-sm font-medium hover:bg-accent">
                <link.icon className="h-4 w-4 text-primary" aria-hidden="true" />
                {link.label}
              </Link>
            ))}
            {can('entrepreneur') ? (
              <Link to="/opportunities" className="flex items-center gap-3 rounded-lg border p-3 text-sm font-medium hover:bg-accent">
                <Inbox className="h-4 w-4 text-primary" aria-hidden="true" />
                Entrepreneur portal
              </Link>
            ) : null}
            <Link to="/me/following" className="flex items-center gap-3 rounded-lg border p-3 text-sm font-medium hover:bg-accent">
              <Bookmark className="h-4 w-4 text-primary" aria-hidden="true" />
              Following list
            </Link>
          </CardContent>
        </Card>
      </div>

      <BoundaryNotice />
    </div>
  )
}