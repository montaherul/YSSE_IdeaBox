import { Link } from 'react-router-dom'
import {
  ArrowRight,
  ChevronDown,
  ClipboardCheck,
  Compass,
  Handshake,
  Lightbulb,
  MessageSquare,
  Rocket,
  Send,
  ShieldCheck,
  Sparkles,
  UserRound,
  Users,
  Vote,
} from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { Button } from '@/components/ui/button'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'

const JOURNEY = [
  {
    icon: Lightbulb,
    title: 'Share your idea',
    body: 'Create a full submission or start with a quick idea using just three fields. Save it as a draft and refine it anytime.',
  },
  {
    icon: ClipboardCheck,
    title: 'Screening & review',
    body: 'Submissions go through structured screening. Reviewers may ask for clarification or revisions before an idea is published.',
  },
  {
    icon: MessageSquare,
    title: 'Community feedback',
    body: 'Published ideas receive constructive reviews and discussion so creators can strengthen their thinking.',
  },
  {
    icon: Vote,
    title: 'Voting & spotlight',
    body: 'During voting periods the community supports ideas it believes in. Top ideas are highlighted on monthly leaderboards.',
  },
  {
    icon: Handshake,
    title: 'Connect & collaborate',
    body: 'Entrepreneurs, mentors and collaborators discover promising ideas and request introductions with creators.',
  },
  {
    icon: Rocket,
    title: 'Grow into impact',
    body: 'Shortlisted ideas can move toward development, with progress tracked from concept through to completion.',
  },
]

const LIFECYCLE = [
  { label: 'Draft', body: 'Private working copy, visible only to you.' },
  { label: 'Submitted', body: 'Sent for screening against community guidelines.' },
  { label: 'Under review', body: 'Moderators and reviewers assess the idea.' },
  { label: 'Published', body: 'Visible to everyone on Idea Box.' },
  { label: 'Shortlisted', body: 'Recognised as promising by the community.' },
  { label: 'In development', body: 'Moving toward real-world implementation.' },
  { label: 'Completed', body: 'Outcomes shared back with the community.' },
]

const AUDIENCES = [
  {
    icon: UserRound,
    title: 'Idea creators',
    body: 'Anyone with a problem worth solving or a solution worth sharing can post an idea and track it from draft to impact.',
  },
  {
    icon: Users,
    title: 'Community reviewers',
    body: 'Share your perspective: review ideas constructively, join the discussion and vote for the ones you believe in.',
  },
  {
    icon: Handshake,
    title: 'Entrepreneurs & collaborators',
    body: 'Discover structured, reviewed ideas and request introductions to the creators behind them.',
  },
  {
    icon: ShieldCheck,
    title: 'Reviewers & moderators',
    body: 'Keep quality high: screen submissions fairly, enforce guidelines and help creators improve their ideas.',
  },
]

const FAQS = [
  {
    question: 'Does YSSE Idea Box fund ideas?',
    answer:
      'No. YSSE facilitates introductions between creators and entrepreneurs. It does not manage funding, hold money, or guarantee investment. Negotiations and agreements happen directly between the parties involved.',
  },
  {
    question: 'Who can submit an idea?',
    answer:
      'Any registered member can submit an idea. Guests can browse published ideas and leave a single anonymous review; creating an account unlocks full participation.',
  },
  {
    question: 'Can I submit anonymously?',
    answer:
      'Yes. Each idea can be published with public attribution or anonymously. Your identity is always recorded internally for accountability, but it is never shown publicly on anonymous ideas.',
  },
  {
    question: 'Can I edit my idea after publishing?',
    answer:
      'Yes. You can edit while an idea is in draft, when revisions are requested, or after publishing. Edits to published ideas are tracked as revisions and the idea re-enters review.',
  },
  {
    question: 'How does voting work?',
    answer:
      'The community votes for ideas during voting periods, and the most supported ideas are highlighted monthly. Popularity reflects community interest and does not determine funding eligibility.',
  },
  {
    question: 'How do entrepreneurs reach me?',
    answer:
      'Entrepreneurs can request an introduction from your published idea. Once you accept, you can continue the conversation through the platform messaging system.',
  },
]

export default function HowItWorksPage() {
  useDocumentTitle('How it works')
  const { isAuthenticated } = useAuth()
  const submitHref = isAuthenticated ? '/app/submissions/new' : '/register'

  return (
    <div className="space-y-16">
      <section
        aria-labelledby="how-hero-heading"
        className="relative overflow-hidden rounded-3xl border bg-gradient-to-b from-background via-background to-accent/60 px-6 py-12 text-center sm:px-10 sm:py-16"
      >
        <div aria-hidden="true" className="pointer-events-none absolute inset-0 overflow-hidden">
          <div className="absolute -left-20 -top-20 h-64 w-64 rounded-full bg-primary/10 blur-3xl" />
          <div className="absolute -right-16 top-4 h-56 w-56 rounded-full bg-primary/10 blur-3xl" />
        </div>

        <div className="relative mx-auto max-w-2xl">
          <span className="inline-flex items-center gap-2 rounded-full bg-secondary px-3 py-1 text-xs font-semibold uppercase tracking-wider text-primary">
            <Sparkles className="h-3.5 w-3.5" aria-hidden="true" />
            How It Works
          </span>
          <h1
            id="how-hero-heading"
            className="mt-5 text-3xl font-bold tracking-tight sm:text-4xl lg:text-5xl"
          >
            From a rough thought to real-world impact
          </h1>
          <p className="mt-4 text-lg text-muted-foreground">
            Every idea on Idea Box follows a clear journey: share it, let the community strengthen
            it, and connect with the people who can help it grow.
          </p>
          <div className="mt-8 flex flex-col justify-center gap-3 sm:flex-row">
            <Button asChild size="lg" className="rounded-full">
              <Link to="/ideas">
                <Compass aria-hidden="true" />
                Explore Ideas
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
        </div>
      </section>

      <section aria-labelledby="journey-heading" className="space-y-8">
        <div className="mx-auto max-w-2xl text-center">
          <h2 id="journey-heading" className="text-2xl font-bold tracking-tight sm:text-3xl">
            Your idea&apos;s journey
          </h2>
          <p className="mt-2 text-muted-foreground">
            Six steps take an idea from a private draft to something the community can rally around.
          </p>
        </div>

        <ol className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {JOURNEY.map((step, index) => (
            <li key={step.title} className="rounded-2xl border bg-card p-6 shadow-sm">
              <div className="flex items-center gap-3">
                <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-primary text-primary-foreground">
                  <step.icon className="h-5 w-5" aria-hidden="true" />
                </span>
                <span className="text-xs font-semibold uppercase tracking-wider text-primary">
                  Step {index + 1}
                </span>
              </div>
              <h3 className="mt-4 text-lg font-semibold">{step.title}</h3>
              <p className="mt-2 text-sm text-muted-foreground">{step.body}</p>
            </li>
          ))}
        </ol>
      </section>

      <section
        aria-labelledby="lifecycle-heading"
        className="rounded-3xl border bg-accent/40 p-6 sm:p-10"
      >
        <div className="mx-auto max-w-2xl text-center">
          <h2 id="lifecycle-heading" className="text-2xl font-bold tracking-tight sm:text-3xl">
            Idea status lifecycle
          </h2>
          <p className="mt-2 text-muted-foreground">
            You can always see exactly where your idea stands. Status changes are recorded with the
            actor, reason and timestamp.
          </p>
        </div>

        <ol className="mt-8 flex snap-x gap-3 overflow-x-auto pb-2">
          {LIFECYCLE.map((stage, index) => (
            <li
              key={stage.label}
              className="w-48 shrink-0 snap-start rounded-2xl border bg-card p-4 shadow-sm"
            >
              <span className="flex h-7 w-7 items-center justify-center rounded-full bg-secondary text-xs font-bold text-primary">
                {index + 1}
              </span>
              <h3 className="mt-3 text-sm font-semibold">{stage.label}</h3>
              <p className="mt-1 text-xs text-muted-foreground">{stage.body}</p>
            </li>
          ))}
        </ol>
      </section>

      <section aria-labelledby="audiences-heading" className="space-y-8">
        <div className="mx-auto max-w-2xl text-center">
          <h2 id="audiences-heading" className="text-2xl font-bold tracking-tight sm:text-3xl">
            Built for everyone in the journey
          </h2>
          <p className="mt-2 text-muted-foreground">
            Idea Box works best when creators, the community and collaborators all take part.
          </p>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          {AUDIENCES.map((audience) => (
            <div key={audience.title} className="rounded-2xl border bg-card p-6 shadow-sm">
              <span className="flex h-11 w-11 items-center justify-center rounded-full bg-secondary text-primary">
                <audience.icon className="h-5 w-5" aria-hidden="true" />
              </span>
              <h3 className="mt-4 text-lg font-semibold">{audience.title}</h3>
              <p className="mt-2 text-sm text-muted-foreground">{audience.body}</p>
            </div>
          ))}
        </div>
      </section>

      <section aria-labelledby="faq-heading" className="space-y-8">
        <div className="mx-auto max-w-2xl text-center">
          <h2 id="faq-heading" className="text-2xl font-bold tracking-tight sm:text-3xl">
            Frequently asked questions
          </h2>
          <p className="mt-2 text-muted-foreground">
            Quick answers before you share your first idea.
          </p>
        </div>

        <div className="mx-auto max-w-3xl divide-y rounded-2xl border bg-card">
          {FAQS.map((faq) => (
            <details key={faq.question} className="group px-5 py-4">
              <summary className="flex cursor-pointer items-center justify-between gap-4 text-sm font-medium focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring [&::-webkit-details-marker]:hidden">
                {faq.question}
                <ChevronDown
                  className="h-4 w-4 shrink-0 text-muted-foreground transition-transform group-open:rotate-180"
                  aria-hidden="true"
                />
              </summary>
              <p className="mt-3 text-sm text-muted-foreground">{faq.answer}</p>
            </details>
          ))}
        </div>
      </section>

      <section
        aria-labelledby="how-cta-heading"
        className="rounded-3xl border bg-accent/60 px-6 py-12 text-center sm:px-10"
      >
        <div className="mx-auto max-w-2xl space-y-4">
          <h2 id="how-cta-heading" className="text-2xl font-bold tracking-tight sm:text-3xl">
            Ready to share your idea?
          </h2>
          <p className="text-muted-foreground">
            It takes a few minutes to get started, and the community takes it from there.
          </p>
          <div className="flex flex-col justify-center gap-3 pt-2 sm:flex-row">
            <Button asChild size="lg" className="rounded-full">
              <Link to={submitHref}>
                <Send aria-hidden="true" />
                Submit an Idea
              </Link>
            </Button>
            <Button asChild size="lg" variant="outline" className="rounded-full">
              <Link to="/guidelines">
                Read Community Guidelines
                <ArrowRight aria-hidden="true" />
              </Link>
            </Button>
          </div>
        </div>
      </section>

      <FacilitationNotice />
    </div>
  )
}
