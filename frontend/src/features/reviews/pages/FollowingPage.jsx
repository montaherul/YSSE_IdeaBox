import { Bookmark } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { followsApi } from '@/features/reviews/api'
import { ResourceCardGrid } from '@/features/_shared'
import { IdeaCard } from '@/features/ideas/components/IdeaCard'
import { PageHeader } from '@/components/shared/PageHeader'

export default function FollowingPage() {
  useDocumentTitle('Following')

  return (
    <div className="space-y-6">
      <PageHeader title="Following" description="Ideas you have saved to follow for updates." />

      <ResourceCardGrid
        queryKey={['ideas', 'followed']}
        queryFn={(params) => followsApi.followed(params)}
        getKey={(item) => item?.idea?.id ?? item?.id ?? item?.ideaId ?? item?.slug}
        searchable={false}
        emptyIcon={Bookmark}
        emptyTitle="Nothing followed yet"
        emptyDescription="Follow ideas you care about to see updates as they evolve."
        renderCard={(item) => <IdeaCard idea={item?.idea ?? item} />}
      />
    </div>
  )
}
