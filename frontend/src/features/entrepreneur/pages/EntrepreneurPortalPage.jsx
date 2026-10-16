import { Link } from 'react-router-dom'
import { Compass, Eye, EyeOff, FolderKanban, Inbox, RefreshCw } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import {
  useMatches,
  useWatchlist,
  useAddToWatchlist,
  useRemoveFromWatchlist,
  useRunMatching,
} from '@/features/entrepreneur/hooks'
import { IdeaCard } from '@/features/ideas/components/IdeaCard'
import { MyProjectsPanel } from '@/features/projects/components/MyProjectsPanel'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'
import { Button } from '@/components/ui/button'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'

export default function EntrepreneurPortalPage() {
  useDocumentTitle('Entrepreneur portal')
  const matches = useMatches({ pageSize: 50 })
  const watchlist = useWatchlist({ pageSize: 50 })
  const addToWatchlist = useAddToWatchlist()
  const removeFromWatchlist = useRemoveFromWatchlist()
  const refresh = useRunMatching()

  const matchItems = matches.data?.items ?? matches.data ?? []
  const watchItems = watchlist.data?.items ?? watchlist.data ?? []

  return (
    <div className="space-y-6">
      <PageHeader
        title="Entrepreneur portal"
        description="Discover ideas worth supporting, keep a watchlist, and connect with creators."
        actions={
          <>
            <Button asChild variant="outline">
              <Link to="/entrepreneur/profile">Business profile</Link>
            </Button>
            <Button asChild>
              <Link to="/inquiries">
                <Inbox className="h-4 w-4" aria-hidden="true" />
                Inquiries
              </Link>
            </Button>
          </>
        }
      />

      <Tabs defaultValue="matches">
        <TabsList>
          <TabsTrigger value="matches">
            <Compass className="mr-1.5 h-4 w-4" aria-hidden="true" />
            Recommendations
          </TabsTrigger>
          <TabsTrigger value="watchlist">
            <Eye className="mr-1.5 h-4 w-4" aria-hidden="true" />
            Watchlist
          </TabsTrigger>
          <TabsTrigger value="projects">
            <FolderKanban className="mr-1.5 h-4 w-4" aria-hidden="true" />
            My projects
          </TabsTrigger>
        </TabsList>

        <TabsContent value="matches" className="pt-4">
          <div className="mb-4 flex items-center justify-between">
            <p className="text-sm text-muted-foreground">
              Ideas matched to your interests and expertise.
            </p>
            <Button size="sm" variant="outline" onClick={() => refresh.mutate({})} disabled={refresh.isPending}>
              <RefreshCw className="h-4 w-4" aria-hidden="true" />
              Refresh
            </Button>
          </div>
          {matches.isLoading ? <ListSkeleton count={6} /> : null}
          {matches.isError ? <ErrorState onRetry={matches.refetch} /> : null}
          {!matches.isLoading && !matches.isError && matchItems.length === 0 ? (
            <EmptyState icon={Compass} title="No recommendations yet" description="Complete your business profile to receive tailored recommendations." />
          ) : null}
          {matchItems.length > 0 ? (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {matchItems.map((item) => {
                const idea = item?.idea ?? item
                const watched = watchItems.some((w) => (w?.ideaId ?? w?.id) === idea.id)
                return (
                  <div key={idea.id ?? idea.slug}>
                    <IdeaCard idea={idea} />
                    <div className="mt-2">
                      <Button
                        size="sm"
                        variant="outline"
                        className="w-full"
                        onClick={() => (watched ? removeFromWatchlist.mutate(idea.id) : addToWatchlist.mutate(idea.id))}
                      >
                        {watched ? (
                          <>
                            <EyeOff className="h-4 w-4" aria-hidden="true" />
                            Remove from watchlist
                          </>
                        ) : (
                          <>
                            <Eye className="h-4 w-4" aria-hidden="true" />
                            Add to watchlist
                          </>
                        )}
                      </Button>
                    </div>
                  </div>
                )
              })}
            </div>
          ) : null}
        </TabsContent>

        <TabsContent value="watchlist" className="pt-4">
          {watchlist.isLoading ? <ListSkeleton count={6} /> : null}
          {watchlist.isError ? <ErrorState onRetry={watchlist.refetch} /> : null}
          {!watchlist.isLoading && !watchlist.isError && watchItems.length === 0 ? (
            <EmptyState icon={EyeOff} title="Your watchlist is empty" description="Save ideas you want to revisit later." />
          ) : null}
          {watchItems.length > 0 ? (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {watchItems.map((item) => {
                const idea = item?.idea ?? item
                return <IdeaCard key={idea.id ?? idea.slug} idea={idea} />
              })}
            </div>
          ) : null}
        </TabsContent>

        <TabsContent value="projects" className="pt-4">
          <p className="mb-4 text-sm text-muted-foreground">
            Projects the YSSE team has assigned to you as a lead or member.
          </p>
          <MyProjectsPanel />
        </TabsContent>
      </Tabs>

      <FacilitationNotice />
    </div>
  )
}