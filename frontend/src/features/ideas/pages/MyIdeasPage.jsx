import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus, Lightbulb } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useMyIdeas, useSubmitIdea, useWithdrawIdea, useDeleteIdea } from '@/features/ideas/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { formatDate, formatCompact } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

export default function MyIdeasPage() {
  useDocumentTitle('My ideas')
  const [page, setPage] = useState(1)
  const [confirm, setConfirm] = useState(null)
  const { data, isLoading, isError, refetch } = useMyIdeas({ page, pageSize: DEFAULT_PAGE_SIZE })
  const submit = useSubmitIdea()
  const withdraw = useWithdrawIdea()
  const remove = useDeleteIdea()

  const items = data?.items ?? []
  const total = data?.total ?? 0

  const runConfirm = () => {
    if (!confirm) return
    if (confirm.action === 'withdraw') withdraw.mutate(confirm.id)
    if (confirm.action === 'delete') remove.mutate(confirm.id)
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="My ideas"
        description="Track the status of the ideas you have submitted."
        actions={
          <Button asChild>
            <Link to="/app/submissions/new">
              <Plus className="h-4 w-4" aria-hidden="true" />
              New idea
            </Link>
          </Button>
        }
      />

      {isLoading ? <ListSkeleton count={6} /> : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState
          icon={Lightbulb}
          title="You have no ideas yet"
          description="Share your first idea and start getting feedback from the community."
          action={
            <Button asChild>
              <Link to="/app/submissions/new">Submit an idea</Link>
            </Button>
          }
        />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2">
            {items.map((idea) => (
              <Card key={idea.id}>
                <CardContent className="flex h-full flex-col gap-3 p-5">
                  <div className="flex items-center justify-between gap-2">
                    <StatusBadge status={idea.status} />
                    <span className="text-xs text-muted-foreground">{formatDate(idea.createdAt, { short: true })}</span>
                  </div>
                  <h2 className="text-base font-semibold leading-snug">
                    <Link to={`/ideas/${idea.slug ?? idea.id}`} className="hover:text-primary">
                      {idea.title}
                    </Link>
                  </h2>
                  <p className="line-clamp-2 text-sm text-muted-foreground">{idea.shortSummary}</p>
                  <p className="text-xs text-muted-foreground">
                    {formatCompact(idea.voteCount ?? 0)} votes · {formatCompact(idea.reviewCount ?? 0)} reviews
                  </p>
                  <div className="mt-auto flex flex-wrap gap-2 pt-2">
                    {idea.status === 'Draft' || idea.status === 'RevisionRequired' ? (
                      <Button size="sm" onClick={() => submit.mutate(idea.id)} disabled={submit.isPending}>
                        Submit
                      </Button>
                    ) : null}
                    {idea.status === 'Draft' || idea.status === 'RevisionRequired' ? (
                      <Button size="sm" variant="outline" asChild>
                        <Link to={`/ideas/${idea.id}/edit`}>Edit</Link>
                      </Button>
                    ) : null}
                    {idea.status === 'Published' || idea.status === 'Submitted' ? (
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => setConfirm({ id: idea.id, action: 'withdraw', title: 'Withdraw idea?' })}
                      >
                        Withdraw
                      </Button>
                    ) : null}
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => setConfirm({ id: idea.id, action: 'delete', title: 'Delete idea?' })}
                    >
                      Delete
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={total} onChange={setPage} />
        </>
      ) : null}

      <ConfirmDialog
        open={Boolean(confirm)}
        onOpenChange={(open) => !open && setConfirm(null)}
        title={confirm?.title ?? 'Are you sure?'}
        description="This action may not be reversible."
        confirmLabel={confirm?.action === 'delete' ? 'Delete' : 'Withdraw'}
        onConfirm={runConfirm}
      />
    </div>
  )
}
