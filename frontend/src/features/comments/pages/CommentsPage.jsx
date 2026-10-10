import { useSearchParams, Link } from 'react-router-dom'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { CommentsList } from '../components/CommentsList'
import { PageHeader } from '@/components/shared/PageHeader'

export default function CommentsPage() {
  const [searchParams] = useSearchParams()
  const ideaId = Number(searchParams.get('ideaId')) || undefined
  useDocumentTitle('Comments')

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <PageHeader
        title="Comments"
        description={ideaId ? 'All public comments for this idea.' : 'Pass an idea in the URL to view its comment thread.'}
      />
      {ideaId ? (
        <CommentsList ideaId={ideaId} />
      ) : (
        <p className="text-sm text-muted-foreground">
          Comment threads are embedded on each idea page.{' '}
          <Link to="/ideas" className="text-primary hover:underline">
            Browse ideas
          </Link>{' '}
          to join the conversation.
        </p>
      )}
    </div>
  )
}