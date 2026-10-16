import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import {
  ArrowRight,
  Compass,
  Heart,
  Lightbulb,
  Medal,
  MessageSquare,
  Plus,
  Search,
  Send,
  Sparkles,
  Star,
  Trophy,
  UserRound,
} from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { APP_TAGLINE, FALLBACK_CATEGORY_NAMES } from '@/config/constants'
import { useIdeaList } from '@/features/ideas/hooks'
import { useMonthlyTopIdeas } from '@/features/voting/hooks'
import { useCategories } from '@/features/categories/hooks'
import { IdeaCard } from '@/features/ideas/components/IdeaCard'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'
import { cn, slugify } from '@/lib/utils'
import { formatCompact } from '@/lib/format'

const DISCOVER_SIZE = 6
const FEATURED_SIZE = 3
const MONTHLY_TOP_SIZE = 3

const STEPS = [
  {
    icon: Lightbulb,
    title: 'Share Your Idea',
    body: 'Submit a promising idea or solution.',
  },
  {
    icon: MessageSquare,
    title: 'Get Community Feedback',
    body: 'Let others review and discuss your idea.',
  },
  {
    icon: Sparkles,
    title: 'Create Real-World Impact',
    body: 'Develop promising ideas into meaningful action.',
  },
]

/** Public landing page — brand hero, idea discovery and community call to action. */
export default function LandingPage() {
  useDocumentTitle(APP_TAGLINE, false)
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const [query, setQuery] = useState('')

  const submitHref = isAuthenticated ? '/app/submissions/new' : '/register'

  const featured = useIdeaList({ status: 'Published', sort: 'newest', page: 1, pageSize: DISCOVER_SIZE })
  const featuredIdeas = useIdeaList({
    status: 'Published',
    featured: true,
    sort: 'newest',
    page: 1,
    pageSize: FEATURED_SIZE,
  })
  const monthlyTop = useMonthlyTopIdeas(MONTHLY_TOP_SIZE)
  const { data: categoriesData, isLoading: categoriesLoading } = useCategories()

  const featuredItems = featured.data?.items ?? featured.data?.data ?? []
  const highlightedItems = featuredIdeas.data?.items ?? featuredIdeas.data?.data ?? []
  const monthlyTopIdeas = monthlyTop.data?.ideas ?? []
  const monthlyTopLabel = monthlyTop.data?.periodLabel
  const backendCategories = categoriesData?.items ?? categoriesData ?? []
  const categories =
    !categoriesLoading && backendCategories.length === 0
      ? FALLBACK_CATEGORY_NAMES.map((name) => ({ name }))
      : backendCategories

  const onSearch = (event) => {
    event.preventDefault()
    const trimmed = query.trim()
    navigate(trimmed ? `/ideas?q=${encodeURIComponent(trimmed)}` : '/ideas')
  }

  return (
    <div className="space-y-16">
      {/* Hero */}
      <section className="relative overflow-hidden rounded-3xl border bg-gradient-to-b from-background via-background to-accent/60">
        {/* Decorative mint curves (behind content) */}
        <div aria-hidden="true" className="pointer-events-none absolute inset-0 overflow-hidden">
          <div className="absolute -left-24 -top-24 h-72 w-72 rounded-full bg-primary/10 blur-3xl" />
          <div className="absolute -right-16 top-8 h-64 w-64 rounded-full bg-primary/10 blur-3xl" />
          <svg
            className="absolute inset-x-0 bottom-0 h-24 w-full text-primary/10"
            viewBox="0 0 1440 320"
            preserveAspectRatio="none"
          >
            <path
              fill="currentColor"
              d="M0,224L80,213.3C160,203,320,181,480,192C640,203,800,245,960,250.7C1120,256,1280,224,1360,208L1440,192L1440,320L0,320Z"
            />
          </svg>
        </div>

        <div className="relative grid items-center gap-8 px-6 py-12 sm:px-10 sm:py-16 lg:grid-cols-[1fr_1.1fr] lg:gap-10 lg:py-20">
          {/* Left: content */}
          <div className="max-w-xl">
            <span className="inline-flex items-center gap-2 rounded-full bg-secondary px-3 py-1 text-xs font-semibold uppercase tracking-wider text-primary">
              <Lightbulb className="h-3.5 w-3.5" aria-hidden="true" />
              Idea Box
            </span>

            <h1 className="mt-5 text-4xl font-bold leading-[1.05] tracking-tight sm:text-5xl lg:text-6xl">
              <span className="block">Every Great Change</span>
              <span className="block">
                Starts With an{' '}
                <span className="relative inline-block text-primary">
                  Idea.
                  <span
                    aria-hidden="true"
                    className="absolute -bottom-0.5 left-0 h-1.5 w-full rounded-full bg-primary/70"
                  />
                </span>
              </span>
            </h1>

            <p className="mt-5 max-w-lg text-lg text-muted-foreground">
              Discover bold ideas, share your perspective, and help turn promising solutions into
              real-world impact.
            </p>

            <div className="mt-8 flex flex-col gap-3 sm:flex-row sm:flex-wrap">
              <Button asChild size="lg" className="rounded-full">
                <Link to="/ideas">
                  <Compass aria-hidden="true" />
                  Explore Ideas
                  <ArrowRight aria-hidden="true" />
                </Link>
              </Button>
              <Button
                asChild
                size="lg"
                variant="outline"
                className="rounded-full border-primary text-primary hover:bg-accent hover:text-primary"
              >
                <Link to={submitHref}>
                  <Send aria-hidden="true" />
                  Share Your Idea
                </Link>
              </Button>
            </div>

            <div className="mt-7 flex items-center gap-3">
              <div className="flex -space-x-2" aria-hidden="true">
                {[0, 1, 2].map((i) => (
                  <span
                    key={i}
                    className="inline-flex h-8 w-8 items-center justify-center rounded-full border-2 border-background bg-accent text-primary"
                  >
                    <UserRound className="h-4 w-4" />
                  </span>
                ))}
                <span className="inline-flex h-8 w-8 items-center justify-center rounded-full border-2 border-background bg-primary text-primary-foreground">
                  <Plus className="h-4 w-4" />
                </span>
              </div>
              <p className="text-sm text-muted-foreground">
                A community for changemakers and innovators
              </p>
            </div>
          </div>

          {/* Right: illustration */}
          <div className="relative flex items-end justify-center self-stretch lg:justify-end">
            <img
              src="/idea-box-hero-illustration.png"
              alt="Three young people collaborating around an idea box, with a glowing lightbulb, floating idea cards, a magnifying glass, paper airplanes and green plants"
              width={805}
              height={489}
              loading="eager"
              decoding="async"
              className="h-auto w-full max-w-md object-contain object-bottom sm:max-w-lg lg:max-w-2xl lg:translate-y-4"
            />
          </div>
        </div>
      </section>

      {/* Featured ideas */}
      {featuredIdeas.isLoading || highlightedItems.length > 0 ? (
        <section id="featured" aria-labelledby="featured-heading" className="space-y-6">
          <div className="mx-auto max-w-2xl text-center">
            <span className="inline-flex items-center gap-2 rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-primary">
              <Star className="h-3.5 w-3.5" aria-hidden="true" />
              Editor&apos;s Picks
            </span>
            <h2 id="featured-heading" className="mt-4 text-2xl font-bold tracking-tight sm:text-3xl">
              Featured Ideas
            </h2>
            <p className="mt-2 text-muted-foreground">
              Hand-picked ideas the community should not miss.
            </p>
          </div>

          {featuredIdeas.isLoading ? <ListSkeleton count={FEATURED_SIZE} /> : null}

          {!featuredIdeas.isLoading ? (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {highlightedItems.map((idea) => (
                <IdeaCard key={idea.id ?? idea.slug} idea={idea} />
              ))}
            </div>
          ) : null}
        </section>
      ) : null}

      {/* Monthly top three */}
      {monthlyTop.isLoading || monthlyTopIdeas.length > 0 ? (
        <section
          id="monthly-top"
          aria-labelledby="monthly-top-heading"
          className="scroll-mt-20 rounded-3xl border bg-gradient-to-b from-accent/50 to-background p-6 sm:p-10"
        >
          <div className="mx-auto max-w-2xl text-center">
            <span className="inline-flex items-center gap-2 rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-primary">
              <Trophy className="h-3.5 w-3.5" aria-hidden="true" />
              {monthlyTopLabel ? `Top 3 · ${monthlyTopLabel}` : 'Monthly Top Three'}
            </span>
            <h2 id="monthly-top-heading" className="mt-4 text-2xl font-bold tracking-tight sm:text-3xl">
              This Month&apos;s Top Three Ideas
            </h2>
            <p className="mt-2 text-muted-foreground">
              The most supported ideas this voting period. Popularity reflects community interest and
              does not determine funding eligibility.
            </p>
          </div>

          {monthlyTop.isLoading ? <ListSkeleton count={MONTHLY_TOP_SIZE} /> : null}

          {!monthlyTop.isLoading && monthlyTopIdeas.length > 0 ? (
            <ol className="mt-10 grid items-end gap-5 sm:grid-cols-3">
              {monthlyTopIdeas.map((idea, index) => {
                const rank = idea.rank ?? index + 1
                const isChampion = rank === 1
                return (
                  <li key={idea.ideaId ?? idea.slug ?? index} className="flex">
                    <Link
                      to={`/ideas/${idea.slug ?? idea.ideaId}`}
                      className={cn(
                        'group relative flex w-full flex-col items-center gap-4 overflow-hidden rounded-2xl border bg-card p-6 pt-8 text-center shadow-sm',
                        'transition-all duration-200 hover:-translate-y-1 hover:shadow-lg',
                        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                        isChampion
                          ? 'border-warning/50 ring-2 ring-warning/25 hover:border-warning sm:pb-12'
                          : 'border-border hover:border-primary/50',
                      )}
                    >
                      <span
                        aria-hidden="true"
                        className={cn(
                          'absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-transparent to-transparent',
                          rank === 1 && 'via-warning/70',
                          rank === 2 && 'via-info/60',
                          rank === 3 && 'via-muted-foreground/40',
                        )}
                      />

                      <span
                        className={cn(
                          'absolute right-3 top-3 inline-flex h-7 min-w-7 items-center justify-center rounded-full px-2 text-xs font-bold',
                          rank === 1 && 'bg-warning/15 text-warning',
                          rank === 2 && 'bg-info/15 text-info',
                          rank === 3 && 'bg-muted text-muted-foreground',
                        )}
                      >
                        #{rank}
                      </span>

                      <span
                        className={cn(
                          'flex items-center justify-center rounded-full text-primary-foreground shadow-md',
                          isChampion ? 'h-16 w-16' : 'h-13 w-13',
                          rank === 1 && 'bg-gradient-to-br from-warning to-warning/60',
                          rank === 2 && 'bg-gradient-to-br from-info to-info/60',
                          rank === 3 && 'bg-gradient-to-br from-muted-foreground/70 to-muted-foreground/40',
                        )}
                      >
                        {isChampion ? (
                          <Trophy className="h-8 w-8" aria-hidden="true" />
                        ) : (
                          <Medal className="h-7 w-7" aria-hidden="true" />
                        )}
                      </span>

                      <div className="min-w-0 space-y-1">
                        <h3
                          className={cn(
                            'line-clamp-2 font-semibold leading-snug transition-colors group-hover:text-primary',
                            isChampion ? 'text-lg' : 'text-base',
                          )}
                        >
                          {idea.title}
                        </h3>
                        {idea.categoryName ? (
                          <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                            {idea.categoryName}
                          </p>
                        ) : null}
                      </div>

                      <span className="mt-auto inline-flex items-center gap-1.5 rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
                        <Heart className="h-3.5 w-3.5" aria-hidden="true" />
                        {formatCompact(idea.voteCount ?? 0)} votes
                      </span>
                    </Link>
                  </li>
                )
              })}
            </ol>
          ) : null}

          <div className="mt-8 flex justify-center">
            <Button asChild variant="outline" size="lg">
              <Link to="/leaderboards">
                View Full Leaderboard
                <ArrowRight aria-hidden="true" />
              </Link>
            </Button>
          </div>
        </section>
      ) : null}

      {/* Discover ideas */}
      <section id="discover" aria-labelledby="discover-heading" className="space-y-6">
        <div className="mx-auto max-w-2xl text-center">
          <h2 id="discover-heading" className="text-2xl font-bold tracking-tight sm:text-3xl">
            Discover Ideas That Can Make a Difference
          </h2>
          <p className="mt-2 text-muted-foreground">
            Explore innovative thinking, share your perspective, and help promising ideas grow.
          </p>
        </div>

        <form onSubmit={onSearch} className="mx-auto flex max-w-xl flex-col gap-3 sm:flex-row">
          <div className="relative flex-1">
            <Search
              className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
              aria-hidden="true"
            />
            <Input
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Search ideas, keywords, tags…"
              aria-label="Search ideas"
              className="h-10 pl-9"
            />
          </div>
          <Button type="submit" size="lg" className="h-10">
            Search ideas
          </Button>
        </form>

        {categories.length > 0 ? (
          <div className="flex flex-wrap justify-center gap-2">
            {categories.slice(0, 8).map((category) => {
              const name = category.name ?? category.title ?? 'Category'
              const href = `/categories/${category.slug ?? slugify(name)}`
              return (
                <Link
                  key={category.id ?? category.categoryId ?? name}
                  to={href}
                  className={cn(
                    'rounded-full border bg-card px-3 py-1.5 text-sm font-medium text-muted-foreground',
                    'transition-colors hover:border-primary/40 hover:text-primary',
                    'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                  )}
                >
                  {name}
                </Link>
              )
            })}
          </div>
        ) : null}

        {featured.isLoading ? <ListSkeleton count={DISCOVER_SIZE} /> : null}

        {featured.isError ? (
          <ErrorState onRetry={featured.refetch} message={featured.error?.message} />
        ) : null}

        {!featured.isLoading && !featured.isError && featuredItems.length === 0 ? (
          <EmptyState
            icon={Lightbulb}
            title="No published ideas yet"
            description="Be the first to share a problem worth solving with the community."
            action={
              <Button asChild>
                <Link to={submitHref}>Submit an Idea</Link>
              </Button>
            }
          />
        ) : null}

        {!featured.isLoading && !featured.isError && featuredItems.length > 0 ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {featuredItems.map((idea) => (
              <IdeaCard key={idea.id ?? idea.slug} idea={idea} />
            ))}
          </div>
        ) : null}

        <div className="flex justify-center">
          <Button asChild variant="outline" size="lg">
            <Link to="/ideas">
              View All Ideas
              <ArrowRight aria-hidden="true" />
            </Link>
          </Button>
        </div>
      </section>

      {/* How it works */}
      <section
        id="how-it-works"
        aria-labelledby="how-heading"
        className="scroll-mt-20 rounded-3xl border bg-accent/40 p-6 sm:p-10"
      >
        <div className="mx-auto max-w-2xl text-center">
          <h2 id="how-heading" className="text-2xl font-bold tracking-tight sm:text-3xl">
            How It Works
          </h2>
          <p className="mt-2 text-muted-foreground">
            Three simple steps from a rough thought to real-world impact.
          </p>
        </div>
        <div className="mt-8 grid gap-4 sm:grid-cols-3">
          {STEPS.map((step, index) => (
            <div key={step.title} className="rounded-2xl border bg-card p-6 text-center shadow-sm">
              <span className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-primary text-primary-foreground">
                <step.icon className="h-6 w-6" aria-hidden="true" />
              </span>
              <span className="mt-4 block text-xs font-semibold uppercase tracking-wider text-primary">
                Step {index + 1}
              </span>
              <h3 className="mt-1 text-lg font-semibold">{step.title}</h3>
              <p className="mt-2 text-sm text-muted-foreground">{step.body}</p>
            </div>
          ))}
        </div>
        <div className="mt-8 flex justify-center">
          <Button asChild variant="outline">
            <Link to="/how-it-works">
              See the Full Journey
              <ArrowRight aria-hidden="true" />
            </Link>
          </Button>
        </div>
      </section>

      {/* Community call to action */}
      <section
        aria-labelledby="cta-heading"
        className="rounded-3xl border bg-accent/60 px-6 py-12 text-center sm:px-10"
      >
        <div className="mx-auto max-w-2xl space-y-4">
          <h2 id="cta-heading" className="text-2xl font-bold tracking-tight sm:text-3xl">
            Have an Idea Worth Sharing?
          </h2>
          <p className="text-muted-foreground">
            Your perspective could inspire the next great solution.
          </p>
          <div className="pt-2">
            <Button asChild size="lg" className="rounded-full">
              <Link to={submitHref}>
                <Send aria-hidden="true" />
                Submit an Idea
              </Link>
            </Button>
          </div>
        </div>
      </section>

      <FacilitationNotice />
    </div>
  )
}
