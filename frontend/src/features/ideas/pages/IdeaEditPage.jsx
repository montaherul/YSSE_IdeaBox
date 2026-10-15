import { useParams } from 'react-router-dom'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useIdeaById, useUpdateIdea } from '@/features/ideas/hooks'
import { IdeaForm } from '@/features/ideas/components/IdeaForm'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { BoundaryNotice } from '@/components/shared/BoundaryNotice'
import { toErrorMessage } from '@/lib/validation'

export default function IdeaEditPage() {
  const { id } = useParams()
  const { data: idea, isLoading, isError, error } = useIdeaById(id)
  const update = useUpdateIdea()
  useDocumentTitle(idea?.title ? `Edit ${idea.title}` : 'Edit idea')

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState message={error?.message} />
  if (!idea) return <ErrorState title="Idea not found" message="This idea may have been removed." />

  const defaultValues = {
    title: idea.title ?? '',
    shortSummary: idea.shortSummary ?? '',
    categoryId: idea.categoryId ?? '',
    problemStatement: idea.problemStatement ?? '',
    proposedSolution: idea.proposedSolution ?? '',
    targetAudience: idea.targetAudience ?? '',
    expectedImpact: idea.expectedImpact ?? '',
    stage: idea.stage ?? '',
    attribution: idea.attribution ?? 'Public',
    supportSought: idea.supportSought ?? '',
    skillsNeeded: idea.skillsNeeded ?? '',
    estimatedBudget: idea.estimatedBudget ?? '',
    expectedTimeline: idea.expectedTimeline ?? '',
    successMetrics: idea.successMetrics ?? '',
    collaborationPreferences: idea.collaborationPreferences ?? '',
    languageTag: idea.languageTag ?? 'en',
    tags: idea.tags ?? [],
  }

  const onSubmit = async ({ attachmentFile, removeAttachment, ...values }) => {
    try {
      await update.mutateAsync({
        id: idea.id ?? id,
        payload: values,
        attachmentFile,
        removeAttachment,
      })
    } catch (err) {
      throw new Error(toErrorMessage(err), { cause: err })
    }
  }

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <PageHeader title="Edit idea" description="Update your idea. Changes may be re-screened." />
      <IdeaForm
        mode="edit"
        defaultValues={defaultValues}
        attachment={idea.attachments?.[0]}
        onSubmit={onSubmit}
        submitting={update.isPending}
      />
      <BoundaryNotice />
    </div>
  )
}
