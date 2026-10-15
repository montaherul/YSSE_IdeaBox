import { useNavigate } from 'react-router-dom'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useCreateIdea } from '@/features/ideas/hooks'
import { IdeaForm } from '@/features/ideas/components/IdeaForm'
import { PageHeader } from '@/components/shared/PageHeader'
import { BoundaryNotice } from '@/components/shared/BoundaryNotice'
import { toErrorMessage } from '@/lib/validation'

export default function IdeaFormPage() {
  useDocumentTitle('Submit an idea')
  const navigate = useNavigate()
  const create = useCreateIdea(false)

  const onSubmit = async (values) => {
    try {
      await create.mutateAsync(values)
    } catch (err) {
      throw new Error(toErrorMessage(err), { cause: err })
    }
  }

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <PageHeader
        title="Submit an idea"
        description="Share your problem and solution. Submissions go through screening before publishing."
        actions={
          <button type="button" className="text-sm text-muted-foreground hover:text-foreground" onClick={() => navigate('/me/ideas')}>
            My ideas
          </button>
        }
      />
      <IdeaForm mode="create" wizard onSubmit={onSubmit} submitting={create.isPending} />
      <BoundaryNotice />
    </div>
  )
}
