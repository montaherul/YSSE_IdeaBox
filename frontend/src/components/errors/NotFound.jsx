import { Link } from 'react-router-dom'
import { Compass } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/shared/EmptyState'

/** 404 view: explains the situation and offers a way forward. */
export function NotFound({ title = 'Page not found' } = {}) {
  return (
    <EmptyState
      icon={Compass}
      title={title}
      description="The page you are looking for does not exist or may have been moved."
      action={
        <div className="flex flex-wrap items-center justify-center gap-2">
          <Button asChild>
            <Link to="/">Back to home</Link>
          </Button>
          <Button asChild variant="outline">
            <Link to="/ideas">Discover ideas</Link>
          </Button>
        </div>
      }
    />
  )
}

export default NotFound
