import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Bookmark,
  Calendar,
  Eye,
  Heart,
  Link2,
  MessageSquare,
  Share2,
  Star,
  UserRound,
  Lock,
} from 'lucide-react'
import { toast } from 'sonner'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { usePermission } from '@/hooks/usePermission'
import { reviewSchema, commentSchema, toErrorMessage } from '@/lib/validation'
import { useIdeaBySlug, useRegisterIdeaView, useSimilarIdeas } from '../hooks'
import { useIdeaVotes, useCastVote, useRemoveVote } from '@/features/voting/hooks'
import {
  useIdeaReviews,
  useCreateReview,
  useCreateGuestReview,
  useIdeaComments,
  useCreateComment,
  useFollowIdea,
} from '@/features/reviews/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { TagList } from '@/components/shared/TagList'
import { UserAvatar } from '@/components/shared/UserAvatar'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { formatCompact, formatDate, sanitizeText, timeAgo } from '@/lib/format'
import { GUEST_REVIEW_NOTICE } from '@/config/constants'
import { IdeaCard } from '../components/IdeaCard'

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
  const { data: reviews } = useIdeaReviews(ideaId)
  const create = useCreateReview(ideaId)
  const createGuest = useCreateGuestReview(ideaId)
  const items = (reviews?.items ?? reviews ?? []).filter((r) => r.status !== 'Hidden' && r.status !== 'RemovedReview')

  const form = useForm({
    resolver: zodResolver(reviewSchema),
    defaultValues: { body: '', rating: null },
  })
  const [rating, setRating] = useState(null)

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

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <MessageSquare className="h-4 w-4" aria-hidden="true" />
          Reviews ({formatCompact(items.length)})
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
                    <UserAvatar name={review.author?.displayName ?? review.authorName} className="h-7 w-7" />
                    <span className="text-sm font-medium">{review.author?.displayName ?? review.authorName ?? 'Anonymous'}</span>
                  </div>
                  <span className="text-xs text-muted-foreground">{timeAgo(review.createdAt)}</span>
                </div>
                {review.rating ? (
                  <p aria-label={`Rated ${review.rating} out of 5`} className="mt-2 text-warning">
                    {'★'.repeat(review.rating)}
                    <span className="text-muted-foreground" aria-hidden="true">{'★'.repeat(5 - review.rating)}</span>
                  </p>
                ) : null}
                <p className="mt-1 whitespace-pre-line text-sm text-muted-foreground">{review.body}</p>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}

function CommentsSection({ ideaId }) {
  const { isAuthenticated } = useAuth()
  const { data: comments } = useIdeaComments(ideaId)
  const create = useCreateComment(ideaId)
  const items = comments?.items ?? comments ?? []

  const form = useForm({
    resolver: zodResolver(commentSchema),
    defaultValues: { body: '' },
  })

  const onSubmit = async (values) => {
    try {
      await create.mutateAsync(values)
      form.reset()
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <MessageSquare className="h-4 w-4" aria-hidden="true" />
          Discussion ({formatCompact(items.length)})
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-5">
        {isAuthenticated ? (
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-3">
            {form.formState.errors.root ? (
              <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{form.formState.errors.root.message}</p>
            ) : null}
            <FormField id="comment-body" label="Add a comment" required error={form.formState.errors.body?.message}>
              <Textarea id="comment-body" rows={2} {...form.register('body')} />
            </FormField>
            <div className="flex justify-end">
              <Button type="submit" size="sm" disabled={create.isPending}>{create.isPending ? 'Posting…' : 'Comment'}</Button>
            </div>
          </form>
        ) : (
          <p className="text-sm text-muted-foreground">
            <Link to="/login" className="text-primary hover:underline">Sign in</Link> to join the discussion.
          </p>
        )}

        {items.length === 0 ? (
          <p className="text-sm text-muted-foreground">No comments yet. Start the conversation.</p>
        ) : (
          <ul aria-label="Comments" className="space-y-3">
            {items.map((comment) => (
              <li key={comment.id} className="flex items-start gap-3 rounded-lg border p-3">
                <UserAvatar name={comment.author?.displayName ?? comment.authorName} className="h-8 w-8" />
                <div className="min-w-0 flex-1">
                  <div className="flex items-center justify-between gap-2">
                    <span className="text-sm font-medium">{comment.author?.displayName ?? comment.authorName ?? 'Guest'}</span>
                    <span className="text-xs text-muted-foreground">{timeAgo(comment.createdAt)}</span>
                  </div>
                  <p className="mt-1 whitespace-pre-line text-sm text-muted-foreground">{comment.body}</p>
                </div>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}

function VotePanel({ idea }) {
  const { isAuthenticated } = useAuth()
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

export default function IdeaDetailPage() {
  const { slug } = useParams()
  const { user, isAuthenticated } = useAuth()
  const { canSendInquiries, can } = usePermission()
  const registerView = useRegisterIdeaView()
  const follow = useFollowIdea()
  const { data: idea, isLoading, isError, error } = useIdeaBySlug(slug)
  const { data: similar } = useSimilarIdeas(idea?.id, Boolean(idea?.id))
  const title = idea?.title ? `${idea.title} — Idea` : 'Idea'

  useDocumentTitle(title)

  useEffect(() => {
    if (idea?.id) registerView.mutate(idea.id)
  }, [idea?.id])

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
            </CardContent>
          </Card>

          <ReviewSection ideaId={idea.id} />
          <CommentsSection ideaId={idea.id} />
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