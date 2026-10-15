import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ThumbsUp, MapPin, Calendar, Link2, Pencil, Trash2 } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { usePermission } from '@/hooks/usePermission'
import { useProblem, useEndorseProblem, useChangeProblemStatus, useDeleteProblem } from '@/features/problems/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { formatDate, formatCompact } from '@/lib/format'

const STATUSES = ['Open', 'SolutionsProposed', 'Solved', 'Closed', 'Paused']

export default function ProblemDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { isAuthenticated, user } = useAuth()
  const { can } = usePermission()
  const canModerate = can('admin') || can('moderator') || can('reviewer')
  const { data: problem, isLoading, isError, error } = useProblem(id)
  const endorse = useEndorseProblem()
  const changeStatus = useChangeProblemStatus()
  const remove = useDeleteProblem()
  const [confirmingDelete, setConfirmingDelete] = useState(false)
  const title = problem?.title ? `${problem.title} — Problem` : 'Problem'
  useDocumentTitle(title)

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState message={error?.message} />
  if (!problem) return <ErrorState title="Problem not found" message="This problem may have been closed or removed." />

  const isAuthor = isAuthenticated && user?.id === problem.authorUserId

  return (
    <div className="space-y-6">
      <PageHeader
        title={problem.title}
        description={problem.coarseLocation ? `Location: ${problem.coarseLocation}` : undefined}
        actions={
          <div className="flex flex-wrap items-center gap-2">
            {isAuthor || canModerate ? (
              <>
                <Button asChild variant="outline">
                  <Link to={`/problems/${problem.id}/edit`}>
                    <Pencil className="h-4 w-4" aria-hidden="true" />
                    Edit
                  </Link>
                </Button>
                <Button variant="outline" onClick={() => setConfirmingDelete(true)}>
                  <Trash2 className="h-4 w-4" aria-hidden="true" />
                  Delete
                </Button>
              </>
            ) : null}
            {isAuthenticated ? (
              <Button onClick={() => endorse.mutate(problem.id)} disabled={endorse.isPending}>
                <ThumbsUp className="h-4 w-4" aria-hidden="true" />
                Endorse ({formatCompact(problem.endorsementCount ?? 0)})
              </Button>
            ) : (
              <Button asChild>
                <Link to="/login">Sign in to endorse</Link>
              </Button>
            )}
          </div>
        }
      />

      <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
        <Card>
          <CardHeader className="flex items-center justify-between">
            <StatusBadge status={problem.status} />
            <div className="flex items-center gap-3 text-sm text-muted-foreground">
              <span className="inline-flex items-center gap-1">
                <Calendar className="h-4 w-4" aria-hidden="true" />
                {formatDate(problem.createdAt)}
              </span>
              {problem.coarseLocation ? (
                <span className="inline-flex items-center gap-1">
                  <MapPin className="h-4 w-4" aria-hidden="true" />
                  {problem.coarseLocation}
                </span>
              ) : null}
            </div>
          </CardHeader>
          <CardContent className="space-y-5">
            <section>
              <h2 className="text-base font-semibold">Who is affected</h2>
              <p className="mt-2 text-sm text-muted-foreground">{problem.whoAffected}</p>
            </section>
            <section>
              <h2 className="text-base font-semibold">Why it matters</h2>
              <p className="mt-2 whitespace-pre-line text-sm text-muted-foreground">{problem.whyItMatters}</p>
            </section>
            {problem.urgency ? (
              <section>
                <h2 className="text-base font-semibold">Urgency</h2>
                <p className="mt-2 text-sm text-muted-foreground">{problem.urgency}</p>
              </section>
            ) : null}
          </CardContent>
        </Card>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Link2 className="h-4 w-4" aria-hidden="true" />
                Linked ideas
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {(problem.linkedIdeas ?? []).length === 0 ? (
                <p className="text-sm text-muted-foreground">No ideas linked to this problem yet.</p>
              ) : (
                <ul className="space-y-2 text-sm">
                  {problem.linkedIdeas.map((idea) => (
                    <li key={idea.id}>
                      <Link to={`/ideas/${idea.slug ?? idea.id}`} className="text-primary hover:underline">
                        {idea.title}
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
            </CardContent>
          </Card>

          {canModerate ? (
            <Card>
              <CardHeader>
                <CardTitle>Moderation</CardTitle>
              </CardHeader>
              <CardContent className="flex flex-wrap gap-2">
                {STATUSES.map((status) => (
                  <Button
                    key={status}
                    size="sm"
                    variant={problem.status === status ? 'default' : 'outline'}
                    onClick={() => changeStatus.mutate({ id: problem.id, payload: { status } })}
                    disabled={changeStatus.isPending || problem.status === status}
                  >
                    {status}
                  </Button>
                ))}
              </CardContent>
            </Card>
          ) : null}
        </div>
      </div>

      <ConfirmDialog
        open={confirmingDelete}
        onOpenChange={setConfirmingDelete}
        title="Delete this problem?"
        description="This problem will be removed for everyone. This cannot be undone."
        confirmLabel="Delete problem"
        onConfirm={() => {
          remove.mutate(problem.id, {
            onSuccess: () => navigate('/problems'),
          })
        }}
      />
    </div>
  )
}
