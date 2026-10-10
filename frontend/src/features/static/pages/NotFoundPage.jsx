import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { NotFound as NotFoundView } from '@/components/errors/NotFound'

export default function NotFoundPage() {
  useDocumentTitle('Page not found')
  return (
    <div className="py-10">
      <NotFoundView />
    </div>
  )
}
