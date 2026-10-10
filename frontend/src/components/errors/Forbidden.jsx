import { Link } from 'react-router-dom'
import { ShieldAlert } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/shared/EmptyState'

/** 403 view: authorization is enforced on the server; this is the UX backstop. */
export function Forbidden({
  title = 'Access denied',
  description = 'You do not have permission to view this page.',
} = {}) {
  return (
    <EmptyState
      icon={ShieldAlert}
      title={title}
      description={description}
      action={
        <div className="flex flex-wrap items-center justify-center gap-2">
          <Button asChild>
            <Link to="/app">Go to dashboard</Link>
          </Button>
          <Button asChild variant="outline">
            <Link to="/">Back to home</Link>
          </Button>
        </div>
      }
    />
  )
}

export default Forbidden
