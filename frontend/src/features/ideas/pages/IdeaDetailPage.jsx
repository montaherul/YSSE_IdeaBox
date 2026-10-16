import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Bookmark,
  Calendar,
  Download,
  Eye,
  FileText,
  Heart,
  Link2,
  MessageSquare,
  Pencil,
  Share2,
  ShieldCheck,
  Star,
  Trash2,
  UserRound,
  Lock,
  FolderKanban,
  Plus,
} from 'lucide-react'
import { toast } from 'sonner'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { usePermission } from '@/hooks/usePermission'
import { reviewSchema, toErrorMessage } from '@/lib/validation'
import { downloadBlob } from '@/lib/media'
import { useIdeaBySlug, useRegisterIdeaView, useSimilarIdeas } from '@/features/ideas/hooks'
import { ideasApi } from '@/features/ideas/api'
import { useIdeaVotes, useCastVote, useRemoveVote } from '@/features/voting/hooks'
import {
  useIdeaReviews,
  useCreateReview,
  useCreateGuestReview,
  useUpdateReview,
  useDeleteReview,
  useModerateReview,
  useFollowIdea,
} from '@/features/reviews/hooks'
import { useIdeaProjects } from '@/features/projects/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { TagList } from '@/components/shared/TagList'
import { UserAvatar } from '@/components/shared/UserAvatar'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { useFormat } from '@/hooks/useFormat'
import { GUEST_REVIEW_NOTICE } from '@/config/constants'
import { IdeaCard } from '@/features/ideas/components/IdeaCard'

function RatingInput({ value, onChange, disabled }) {
  return (
    <div className="flex items-center gap-1" role="group" aria-label="Rating (1 to 5 stars)">
      {[1, 2, 3, 4, 5].map((n) => (
        <button
          key={n}
          type="button"
          aria-label={`${n} star${n > 1 ? 's' : ''}`}
          aria-pressed={value === n}
          disabled={disabled}
          onClick={() => onChange(value === n ? null : n)}
          className={`rounded p-0.5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring ${
            n <= (value ?? 0) ? 'text-warning' : 'text-muted-foreground'
          }`}
        >
          <Star className="h-5 w-5" fill={n <= (value ?? 0) ? 'currentColor' : 'none'} aria-hidden="true" />
        </button>
      ))}
    </div>
  )
}

function ReviewSection({ ideaId }) {
  const { isAuthenticated } = useAuth()
  const { can } = usePermission()
  const { formatCompact, timeAgo } = useFormat()
  const { data: reviews } = useIdeaReviews(ideaId)
  const create = useCreateReview(ideaId)
  const createGuest = useCreateGuestReview(ideaId)
  const update = useUpdateReview(ideaId)
  const remove = useDeleteReview(ideaId)
  const moderate = useModerateReview(ideaId)
  const [editing, setEditing] = useState(null)
  const [pendingDelete, setPendingDelete] = useState(null)
  const items = (reviews?.items ?? reviews ?? []).filter((r) => r.moderationStatus !== 'Hidden' && r.moderationStatus !== 'Removed' && r.moderationStatus !== 'Spam')
  const canModerate = can('admin') || can('moderator') || can('reviewer')

  const form = useForm({
    resolver: zodResolver(reviewSchema),
    defaultValues: { body: '', rating: null },
  })
  const [rating, setRating] = useState(null)

  const editForm = useForm({
    resolver: zodResolver(reviewSchema),
    defaultValues: { body: '', rating: null },
  })
  const [editRating, setEditRating] = useState(null)

  const onSubmit = async (values) => {
    const payload = { ...values, rating }
    try {
      if (isAuthenticated) await create.mutateAsync(payload)
      else await createGuest.mutateAsync(payload)
      form.reset()
      setRating(null)
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  const onEditSubmit = async (values) => {
    if (!editing) return
    try {
      await update.mutateAsync({ id: editing.id, payload: { ...values, rating: editRating } })
      setEditing(null)
      setEditRating(null)
    } catch (err) {
      editForm.setError('root', { message: toErrorMessage(err) })
    }
  }

  const startEdit = (review) => {
    editForm.reset({ body: review.body ?? '', rating: review.rating ?? null })
    setEditRating(review.rating ?? null)
    setEditing(review)
  }

  const moderateTo = (review, status) => {
    moderate.mutate({ id: review.id, payload: { status, reason: 'Moderated via idea page' } })
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <MessageSquare className="h-4 w-4" aria-hidden="true" />
          Reviews ({formatCompact(items.length + (editing ? 0 : 0))})
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-5">
        <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-3 rounded-lg border p-4">
          {!isAuthenticated ? (
            <p className="rounded-md bg-secondary/50 px-3 py-2 text-sm text-muted-foreground">{GUEST_REVIEW_NOTICE}</p>
          ) : null}
          {form.formState.errors.root ? (
            <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{form.formState.errors.root.message}</p>
          ) : null}
          <FormField id="review-body" label="Your review" required hint="At least 10 characters" error={form.formState.errors.body?.message}>
            <Textarea id="review-body" rows={3} {...form.register('body')} />
          </FormField>
          <div className="flex items-center justify-between gap-3">
            <RatingInput value={rating} onChange={setRating} disabled={create.isPending || createGuest.isPending} />
            <Button type="submit" size="sm" disabled={create.isPending || createGuest.isPending}>
              {create.isPending || createGuest.isPending ? 'Posting…' : 'Post review'}
            </Button>
          </div>
        </form>

        {items.length === 0 ? (
          <p className="text-sm text-muted-foreground">No reviews yet. Be the first to share feedback.</p>
        ) : (
          <ul aria-label="Reviews" className="space-y-3">
            {items.map((review) => (
              <li key={review.id} className="rounded-lg border p-4">
                <div className="flex items-center justify-between gap-2">
                  <div className="flex items-center gap-2">
                    <UserAvatar name={review.displayName} className="h-7 w-7" />
                    <span className="text-sm font-medium">{review.displayName ?? 'Anonymous'}</span>
                    {review.canEdit ? (
                      <span className="rounded-full bg-secondary px-2 py-0.5 text-xs text-muted-foreground">You</span>
                    ) : null}
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-muted-foreground">{timeAgo(review.createdAt)}</span>
                    {review.canEdit ? (
                      <>
                        <Button size="sm" variant="ghost" onClick={() => startEdit(review)} disabled={Boolean(editing)}>
                          <Pencil className="h-3.5 w-3.5" aria-hidden="true" />
                          Edit
                        </Button>
                        <Button size="sm" variant="ghost" onClick={() => setPendingDelete(review)}>
                          <Trash2 className="h-3.5 w-3.5" aria-hidden="true" />
                          Delete
                        </Button>
                      </>
                    ) : null}
                    {canModerate ? (
                      <div className="flex items-center gap-1">
                        {review.moderationStatus === 'Published' ? (
                          <Button size="sm" variant="ghost" onClick={() => moderateTo(review, 'Hidden')}>
                            <ShieldCheck className="h-3.5 w-3.5" aria-hidden="true" />
                            Hide
                          </Button>
                        ) : null}
                      </div>
                    ) : null}
                  </div>
                </div>
                {review.rating ? (
                  <p aria-label={`Rated ${review.rating} out of 5`} className="mt-2 text-warning">
                    {'★'.repeat(review.rating)}
                    <span className="text-muted-foreground" aria-hidden="true">{'★'.repeat(5 - review.rating)}</span>
                  </p>
                ) : null}
                {editing?.id === review.id ? (
                  <form onSubmit={editForm.handleSubmit(onEditSubmit)} noValidate className="mt-3 space-y-3 rounded-md border p-3">
                    {editForm.formState.errors.root ? (
                      <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{editForm.formState.errors.root.message}</p>
                    ) : null}
                    <FormField id={`edit-review-body-${review.id}`} label="Your review" required error={editForm.formState.errors.body?.message}>
                      <Textarea id={`edit-review-body-${review.id}`} rows={3} {...editForm.register('body')} />
                    </FormField>
                    <div className="flex items-center justify-between gap-3">
                      <RatingInput value={editRating} onChange={setEditRating} disabled={update.isPending} />
                      <div className="flex gap-2">
                        <Button type="submit" size="sm" disabled={update.isPending}>
                          {update.isPending ? 'Saving…' : 'Save'}
                        </Button>
                        <Button type="button" size="sm" variant="outline" onClick={() => setEditing(null)}>
                          Cancel
                        </Button>
                      </div>
                    </div>
                  </form>
                ) : (
                  <p className="mt-1 whitespace-pre-line text-sm text-muted-foreground">{review.body}</p>
                )}
              </li>
            ))}
          </ul>
        )}
      </CardContent>

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onOpenChange={(open) => !open && setPendingDelete(null)}
        title="Delete this review?"
        description="Your review will be removed from the idea page."
        confirmLabel="Delete"
        onConfirm={() => {
          if (pendingDelete) {
            remove.mutate(pendingDelete.id)
            setPendingDelete(null)
          }
        }}
      />
    </Card>
  )
}

function VotePanel({ idea }) {
  const { isAuthenticated } = useAuth()
  const { formatCompact } = useFormat()
  const { data: votes } = useIdeaVotes(idea?.id)
  const cast = useCastVote(idea?.id)
  const remove = useRemoveVote(idea?.id)

  const totalVotes = votes?.total ?? votes?.voteCount ?? idea?.voteCount ?? 0
  const userVoted = Boolean(votes?.userVoted ?? votes?.votedByMe ?? false)
  const eligibility = votes?.eligibility

  const onVote = () => {
    if (userVoted) remove.mutate()
    else cast.mutate({})
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Heart className="h-4 w-4" aria-hidden="true" />
          Voting
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        <p className="text-2xl font-bold">{formatCompact(totalVotes)}</p>
        <p className="text-sm text-muted-foreground">
          {userVoted ? 'You have voted for this idea.' : 'Community members support this idea with votes.'}
        </p>

        {isAuthenticated ? (
          <Button className="w-full" variant={userVoted ? 'outline' : 'default'} onClick={onVote} disabled={cast.isPending || remove.isPending}>
            <Heart className="h-4 w-4" fill={userVoted ? 'currentColor' : 'none'} aria-hidden="true" />
            {userVoted ? 'Remove vote' : 'Vote for this idea'}
          </Button>
        ) : (
          <p className="flex items-center gap-2 rounded-md bg-secondary/50 px-3 py-2 text-sm text-muted-foreground">
            <Lock className="h-4 w-4 shrink-0" aria-hidden="true" />
            <Link to="/login" className="text-primary hover:underline">Sign in</Link> to vote
          </p>
        )}

        {eligibility?.ok === false && eligibility?.reason ? (
          <p className="rounded-md bg-warning/10 px-3 py-2 text-sm text-warning" role="status">
            {eligibility.reason}
          </p>
        ) : null}
      </CardContent>
    </Card>
  )
}

function ProjectsSection({ ideaId, canStart }) {
  const { isAuthenticated } = useAuth()
  // Staff and participants only — the API hides this list from everyone else (403), so the section disappears.
  const { data: projects, isError } = useIdeaProjects(ideaId, isAuthenticated)
  const items = projects?.items ?? projects ?? []

  if (!isAuthenticated || isError) return null

  return (
    <Card>
      <CardHeader className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <CardTitle className="flex items-center gap-2">
          <FolderKanban className="h-4 w-4" aria-hidden="true" />
          Projects ({items.length})
        </CardTitle>
        {canStart ? (
          <Button asChild size="sm" variant="outline">
            <Link to={`/projects/new?ideaId=${ideaId}`}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              Start a project
            </Link>
          </Button>
        ) : null}
      </CardHeader>
      <CardContent>
        {items.length === 0 ? (
          <p className="text-sm text-muted-foreground">No projects yet.</p>
        ) : (
          <ul aria-label="Projects" className="space-y-2">
            {items.map((project) => (
              <li key={project.id}>
                <Link
                  to={`/projects/${project.id}`}
                  className="flex items-center justify-between gap-2 rounded-lg border p-3 text-sm hover:bg-accent"
                >
                  <span className="font-medium">{project.name}</span>
                  <StatusBadge status={project.status} />
                </Link>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}

export default function IdeaDetailPage() {
  const { formatCompact, formatDate, sanitizeText } = useFormat()
  const { slug } = useParams()
  const { user, isAuthenticated } = useAuth()
  const { canSendInquiries, can, canPermission } = usePermission()
  const registerView = useRegisterIdeaView()
  const follow = useFollowIdea()
  const { data: idea, isLoading, isError, error } = useIdeaBySlug(slug)
  const { data: similar } = useSimilarIdeas(idea?.id, Boolean(idea?.id))
  const [downloadingId, setDownloadingId] = useState(null)
  const title = idea?.title ? `${idea.title} — Idea` : 'Idea'

  useDocumentTitle(title)

  useEffect(() => {
    if (idea?.id) registerView.mutate(idea.id)
  }, [idea?.id])

  const downloadDocument = async (attachment) => {
    try {
      setDownloadingId(attachment.id)
      const blob = await ideasApi.downloadAttachment(idea.id)
      downloadBlob(blob, attachment.fileName || 'document.pdf')
    } catch (err) {
      toast.error(toErrorMessage(err))
    } finally {
      setDownloadingId(null)
    }
  }

  const share = async () => {
    const url = window.location.href
    try {
      if (navigator.share) await navigator.share({ title: idea?.title, url })
      else {
        await navigator.clipboard.writeText(url)
        toast.success('Link copied to clipboard.')
      }
    } catch {
      /* user cancelled */
    }
  }

  const isAuthor = Boolean(user?.id && idea?.author?.userId && idea.author.userId === user.id)
  const canEdit = isAuthenticated && (isAuthor || can('admin') || can('moderator'))

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState message={error?.message} />
  if (!idea) return <ErrorState title="Idea not found" message="This idea may have been removed or unpublished." />

  return (
    <div className="space-y-6">
      <PageHeader
        title={idea.title}
        description={idea.categoryName ? `Category: ${idea.categoryName}` : undefined}
        actions={
          <div className="flex flex-wrap items-center gap-2">
            {canSendInquiries() ? (
              <Button asChild>
                <Link to={`/inquiries?idea=${idea.id}`}>Contact about this idea</Link>
              </Button>
            ) : null}
            {canEdit ? (
              <Button asChild variant="outline">
                <Link to={`/ideas/${idea.id}/edit`}>Edit idea</Link>
              </Button>
            ) : null}
            {isAuthenticated ? (
              <Button variant="outline" onClick={() => follow.mutate(idea.id)} disabled={follow.isPending}>
                <Bookmark className="h-4 w-4" fill={idea.isFollowed ? 'currentColor' : 'none'} aria-hidden="true" />
                {idea.isFollowed ? 'Following' : 'Follow'}
              </Button>
            ) : null}
            <Button variant="outline" onClick={share}>
              <Share2 className="h-4 w-4" aria-hidden="true" />
              Share
            </Button>
          </div>
        }
      />

      <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
        <div className="space-y-6">
          <Card>
            <CardHeader className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
              <div className="flex items-center gap-3">
                <StatusBadge status={idea.status} />
                {idea.stage ? <span className="text-sm text-muted-foreground">{idea.stage}</span> : null}
              </div>
              <div className="flex items-center gap-4 text-sm text-muted-foreground" aria-label="Idea metrics">
                <span className="inline-flex items-center gap-1">
                  <Heart className="h-4 w-4" aria-hidden="true" />
                  {formatCompact(idea.voteCount ?? 0)}
                </span>
                <span className="inline-flex items-center gap-1">
                  <MessageSquare className="h-4 w-4" aria-hidden="true" />
                  {formatCompact(idea.reviewCount ?? 0)}
                </span>
                <span className="inline-flex items-center gap-1">
                  <Eye className="h-4 w-4" aria-hidden="true" />
                  {formatCompact(idea.viewCount ?? 0)}
                </span>
                <span className="inline-flex items-center gap-1">
                  <Calendar className="h-4 w-4" aria-hidden="true" />
                  {formatDate(idea.createdAt)}
                </span>
              </div>
            </CardHeader>
            <CardContent className="space-y-6">
              <section>
                <h2 className="text-base font-semibold">Short summary</h2>
                <p className="mt-2 whitespace-pre-line text-sm text-muted-foreground">
                  {sanitizeText(idea.shortSummary ?? idea.proposedSolution ?? '')}
                </p>
              </section>

              <section>
                <h2 className="text-base font-semibold">Problem statement</h2>
                <p className="mt-2 whitespace-pre-line text-sm text-muted-foreground">
                  {sanitizeText(idea.problemStatement ?? '')}
                </p>
              </section>

              <section>
                <h2 className="text-base font-semibold">Proposed solution</h2>
                <p className="mt-2 whitespace-pre-line text-sm text-muted-foreground">
                  {sanitizeText(idea.proposedSolution ?? '')}
                </p>
              </section>

              <section>
                <h2 className="text-base font-semibold">Expected impact</h2>
                <p className="mt-2 text-sm text-muted-foreground">{idea.expectedImpact}</p>
              </section>

              <section>
                <h2 className="text-base font-semibold">Target audience</h2>
                <p className="mt-2 text-sm text-muted-foreground">{idea.targetAudience}</p>
              </section>

              {idea.tags?.length ? (
                <section>
                  <h2 className="text-base font-semibold">Tags</h2>
                  <TagList className="mt-2" tags={idea.tags} />
                </section>
              ) : null}

              {idea.attachments?.length ? (
                <section>
                  <h2 className="text-base font-semibold">Full document</h2>
                  <p className="mt-1 text-xs text-muted-foreground">
                    Private document. Visible to you and authorised reviewers only.
                  </p>
                  <ul className="mt-2 space-y-2">
                    {idea.attachments.map((attachment) => (
                      <li key={attachment.id}>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => downloadDocument(attachment)}
                          disabled={downloadingId === attachment.id}
                        >
                          <FileText className="h-4 w-4" aria-hidden="true" />
                          {attachment.fileName}
                          <Download className="h-4 w-4" aria-hidden="true" />
                        </Button>
                      </li>
                    ))}
                  </ul>
                </section>
              ) : null}
            </CardContent>
          </Card>

          <ProjectsSection ideaId={idea.id} canStart={canPermission('projects.create')} />
          <ReviewSection ideaId={idea.id} />
        </div>

        <div className="space-y-6">
          <VotePanel idea={idea} />

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <UserRound className="h-4 w-4" aria-hidden="true" />
                Creator
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-3">
                <UserAvatar
                  name={idea.author?.displayName || idea.authorName}
                  src={idea.author?.imageUrl}
                  className="h-10 w-10"
                />
                <div>
                  <p className="font-medium">{idea.author?.displayName || idea.authorName || 'Anonymous creator'}</p>
                  {idea.author?.userId ? (
                    <Link to={`/creator/${idea.author.userId}`} className="text-sm text-primary hover:underline">
                      View creator profile
                    </Link>
                  ) : null}
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Link2 className="h-4 w-4" aria-hidden="true" />
                Quick info
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Attribution</span>
                <span>{idea.attribution ?? 'Public'}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Support sought</span>
                <span>{idea.supportSought || '—'}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Skills needed</span>
                <span>{idea.skillsNeeded || '—'}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Estimated budget</span>
                <span>{idea.estimatedBudget != null ? idea.estimatedBudget : '—'}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Attribution</span>
                <span>{idea.attribution ?? 'Public'}</span>
              </div>
            </CardContent>
          </Card>

          {similar?.items?.length ? (
            <Card>
              <CardHeader>
                <CardTitle>Similar ideas</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                {similar.items.slice(0, 3).map((s) => (
                  <IdeaCard key={s.id} idea={s} showAuthor={false} />
                ))}
              </CardContent>
            </Card>
          ) : null}
        </div>
      </div>

      <FacilitationNotice />
    </div>
  )
}