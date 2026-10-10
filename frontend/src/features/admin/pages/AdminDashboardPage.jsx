import { Link } from 'react-router-dom'
import { Users, Lightbulb, Flag, BarChart3 } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAdminDashboard } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { formatCompact } from '@/lib/format'

export default function AdminDashboardPage() {
  useDocumentTitle('Admin dashboard')
  const { data, isLoading, isError, refetch } = useAdminDashboard()

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState onRetry={refetch} />

  const stats = [
    { label: 'Total users', value: data?.totalUsers ?? data?.userCount, icon: Users, to: '/admin/users' },
    { label: 'Total ideas', value: data?.totalIdeas ?? data?.ideaCount, icon: Lightbulb, to: '/admin/moderation' },
    { label: 'Published ideas', value: data?.publishedIdeas, icon: Lightbulb, to: '/admin/moderation' },
    { label: 'Pending reports', value: data?.pendingReports ?? data?.reportCount, icon: Flag, to: '/admin/moderation' },
    { label: 'Total votes', value: data?.totalVotes ?? data?.voteCount, icon: BarChart3, to: '/admin/voting' },
  ].filter((s) => s.value !== undefined)

  return (
    <div className="space-y-6">
      <PageHeader title="Overview" description="A snapshot of activity across the platform." />

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {stats.map((stat) => (
          <Link key={stat.label} to={stat.to} className="rounded-xl border bg-card p-5 shadow-sm transition-shadow hover:shadow-md">
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">{stat.label}</p>
              <stat.icon className="h-5 w-5 text-primary" aria-hidden="true" />
            </div>
            <p className="mt-2 text-2xl font-bold">{formatCompact(stat.value)}</p>
          </Link>
        ))}
      </div>

      {data?.recentIdeas?.length ? (
        <Card>
          <CardHeader>
            <CardTitle>Recent ideas</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {data.recentIdeas.slice(0, 5).map((idea) => (
              <div key={idea.id ?? idea.slug} className="flex items-center justify-between gap-3 text-sm">
                <Link to={idea.slug ? `/ideas/${idea.slug}` : `/ideas/id/${idea.id}`} className="truncate text-primary hover:underline">
                  {idea.title}
                </Link>
                <span className="shrink-0 text-muted-foreground">{idea.author?.displayName ?? idea.authorName ?? ''}</span>
              </div>
            ))}
          </CardContent>
        </Card>
      ) : null}
    </div>
  )
}