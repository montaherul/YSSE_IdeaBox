import { useNavigate } from 'react-router-dom'
import { Bookmark } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useFollowedIdeas } from '../hooks'
import { IdeaCard } from '@/features/ideas/components/IdeaCard'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { Button } from '@/components/ui/button'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

export default function FollowingPage() {
  useDocumentTitle('Following')
  const navigate = useNavigate()
  const page = Number(new URLSearchParams(window.location.search).get('page')) || 1
  const { data, isLoading, isError, refetch } = useFollowedIdeas({ page, pageSize: DEFAULT_PAGE_SIZE })

  const items = data?.items ?? []
  const total = data?.total ?? 0

  return (
    <div className="space-y-6">
      <PageHeader title="Following" description="Ideas you have saved to follow for updates." />

      {isLoading ? <ListSkeleton count={6} /> : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState
          icon={Bookmark}
          title="Nothing followed yet"
          description="Follow ideas you care about to see updates as they evolve."
          action={
            <Button onClick={() => navigate('/ideas')} variant="outline">
              Discover ideas
            </Button>
          }
        />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((item) => {
              const idea = item?.idea ?? item
              return <IdeaCard key={idea.id ?? idea.ideaId ?? idea.slug} idea={idea} />
            })}
          </div>
          <Pagination
            page={page}
            pageSize={DEFAULT_PAGE_SIZE}
            total={total}
            onChange={(next) => {
              const params = new URLSearchParams(window.location.search)
              if (next <= 1) params.delete('page')
              else params.set('page', String(next))
              navigate(`/me/following?${params.toString()}`)
            }}
          />
        </>
      ) : null}
    </div>
  )
}