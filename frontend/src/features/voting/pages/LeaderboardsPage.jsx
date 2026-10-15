import { Link } from 'react-router-dom'
import { Trophy, Medal, Lightbulb, Heart, MessageSquare } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useLeaderboardIdeas, useLeaderboardIdeaMakers } from '@/features/voting/hooks'
import { useVotingPeriods } from '@/features/voting/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'
import { Card, CardContent } from '@/components/ui/card'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { UserAvatar } from '@/components/shared/UserAvatar'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { formatCompact, formatDate } from '@/lib/format'

function rankIcon(rank) {
  if (rank === 1) return <Trophy className="h-4 w-4 text-warning" aria-hidden="true" />
  if (rank <= 3) return <Medal className="h-4 w-4 text-info" aria-hidden="true" />
  return <span className="text-xs font-semibold text-muted-foreground">{rank}</span>
}

export default function LeaderboardsPage() {
  useDocumentTitle('Leaderboards')
  const ideas = useLeaderboardIdeas({ pageSize: 50 })
  const makers = useLeaderboardIdeaMakers({ pageSize: 50 })
  const periods = useVotingPeriods()

  const ideaItems = ideas.data?.items ?? ideas.data ?? []
  const makerItems = makers.data?.items ?? makers.data ?? []
  const activePeriod = (periods.data?.items ?? periods.data ?? []).find((p) => p.isActive)

  return (
    <div className="space-y-6">
      <PageHeader
        title="Leaderboards"
        description="Celebrating the most supported ideas and the people behind them. Popularity reflects community interest — it does not determine funding eligibility."
      />

      {activePeriod ? (
        <p className="rounded-lg border bg-muted/40 px-4 py-3 text-sm text-muted-foreground">
          Active voting period: <strong className="text-foreground">{activePeriod.name}</strong>
          {activePeriod.endAt ? ` · closes ${formatDate(activePeriod.endAt)}` : ''}
        </p>
      ) : null}

      <Tabs defaultValue="ideas">
        <TabsList>
          <TabsTrigger value="ideas">
            <Lightbulb className="mr-1.5 h-4 w-4" aria-hidden="true" />
            Top ideas
          </TabsTrigger>
          <TabsTrigger value="makers">
            <Trophy className="mr-1.5 h-4 w-4" aria-hidden="true" />
            Idea makers
          </TabsTrigger>
        </TabsList>

        <TabsContent value="ideas" className="pt-4">
          {ideas.isLoading ? <ListSkeleton count={6} /> : null}
          {ideas.isError ? <ErrorState onRetry={ideas.refetch} /> : null}
          {!ideas.isLoading && !ideas.isError && ideaItems.length === 0 ? (
            <EmptyState icon={Trophy} title="No ranked ideas yet" description="Voting results will appear here once a voting period closes." />
          ) : null}
          {ideaItems.length > 0 ? (
            <div className="space-y-6">
              <div className="grid gap-4 sm:grid-cols-3">
                {ideaItems.slice(0, 3).map((idea, index) => (
                  <Card key={idea.id ?? idea.slug} className="border-primary/40 bg-accent/40">
                    <CardContent className="space-y-2 p-4 text-center">
                      <span className="mx-auto flex h-10 w-10 items-center justify-center rounded-full bg-primary text-primary-foreground">
                        {index === 0 ? (
                          <Trophy className="h-5 w-5" aria-hidden="true" />
                        ) : (
                          <Medal className="h-5 w-5" aria-hidden="true" />
                        )}
                      </span>
                      <p className="text-xs font-semibold uppercase tracking-wider text-primary">
                        Rank #{idea.rank ?? index + 1}
                      </p>
                      <Link
                        to={`/ideas/${idea.slug ?? idea.id}`}
                        className="block font-semibold hover:text-primary"
                      >
                        {idea.title}
                      </Link>
                      <p className="text-xs text-muted-foreground">
                        {formatCompact(idea.voteCount ?? 0)} votes
                      </p>
                    </CardContent>
                  </Card>
                ))}
              </div>
              {ideaItems.length > 3 ? (
                <ol className="space-y-3">
                  {ideaItems.slice(3).map((idea, index) => (
                    <li key={idea.id ?? idea.slug}>
                      <Card>
                        <CardContent className="flex items-center gap-4 p-4">
                          <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted">
                            {rankIcon(idea.rank ?? index + 4)}
                          </span>
                          <div className="min-w-0 flex-1">
                            <Link to={`/ideas/${idea.slug ?? idea.id}`} className="font-medium hover:text-primary">
                              {idea.title}
                            </Link>
                            <div className="mt-1 flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
                              <span className="inline-flex items-center gap-1">
                                <Heart className="h-3.5 w-3.5" aria-hidden="true" />
                                {formatCompact(idea.voteCount ?? 0)} votes
                              </span>
                              <span className="inline-flex items-center gap-1">
                                <MessageSquare className="h-3.5 w-3.5" aria-hidden="true" />
                                {formatCompact(idea.reviewCount ?? 0)} reviews
                              </span>
                            </div>
                          </div>
                          <StatusBadge status={idea.status} />
                        </CardContent>
                      </Card>
                    </li>
                  ))}
                </ol>
              ) : null}
            </div>
          ) : null}
        </TabsContent>

        <TabsContent value="makers" className="pt-4">
          {makers.isLoading ? <ListSkeleton count={6} /> : null}
          {makers.isError ? <ErrorState onRetry={makers.refetch} /> : null}
          {!makers.isLoading && !makers.isError && makerItems.length === 0 ? (
            <EmptyState icon={Trophy} title="No idea makers yet" description="Top contributors will be recognised here." />
          ) : null}
          {makerItems.length > 0 ? (
            <ol className="space-y-3">
              {makerItems.map((maker, index) => (
                <li key={maker.userId ?? maker.id ?? index}>
                  <Card>
                    <CardContent className="flex items-center gap-4 p-4">
                      <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted">
                        {rankIcon(maker.rank ?? index + 1)}
                      </span>
                      <UserAvatar name={maker.displayName} src={maker.imageUrl} className="h-9 w-9" />
                      <div className="min-w-0 flex-1">
                        <p className="truncate font-medium">{maker.displayName ?? 'Community member'}</p>
                        <p className="text-xs text-muted-foreground">
                          {formatCompact(maker.ideaCount ?? 0)} ideas · {formatCompact(maker.totalVotes ?? 0)} votes
                        </p>
                      </div>
                      {maker.userId ? (
                        <Link to={`/creator/${maker.userId}`} className="text-sm text-primary hover:underline">
                          View profile
                        </Link>
                      ) : null}
                    </CardContent>
                  </Card>
                </li>
              ))}
            </ol>
          ) : null}
        </TabsContent>
      </Tabs>
    </div>
  )
}
