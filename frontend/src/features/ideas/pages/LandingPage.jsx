import { Link } from 'react-router-dom'
import {
  ArrowRight,
  Compass,
  HeartHandshake,
  Lightbulb,
  Rocket,
  Sparkles,
  Users,
} from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { APP_KICKER, APP_TAGLINE } from '@/config/constants'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'
import { cn } from '@/lib/utils'

const STEPS = [
  {
    icon: Lightbulb,
    title: 'Share an idea',
    body: 'Describe a problem and your proposed solution. Quick ideas capture it in minutes.',
  },
  {
    icon: Compass,
    title: 'Get feedback',
    body: 'The community reviews, comments and votes. Screening keeps quality high.',
  },
  {
    icon: Users,
    title: 'Connect',
    body: 'Entrepreneurs, mentors and collaborators reach out through inquiries.',
  },
  {
    icon: Rocket,
    title: 'Grow',
    body: 'List resources, run projects and report outcomes — all within the community.',
  },
]

const SHOWCASES = [
  {
    icon: HeartHandshake,
    title: 'Social impact',
    body: 'Ideas that improve communities.',
    to: '/ideas',
    cta: 'Explore ideas',
    className: 'from-[#13a88a] to-[#2bd9a8]',
  },
  {
    icon: Sparkles,
    title: 'Entrepreneurship',
    body: 'Possibilities waiting to grow.',
    to: '/leaderboard',
    cta: 'See what is rising',
    className: 'from-[#5b4bdb] to-[#8e7cff]',
  },
]

/** Public landing page. */
export default function LandingPage() {
  useDocumentTitle(APP_TAGLINE, false)
  const { isAuthenticated } = useAuth()

  return (
    <div className="space-y-12">
      <section className="relative overflow-hidden rounded-3xl border bg-card">
        <div aria-hidden="true" className="pointer-events-none absolute inset-0">
          <div className="absolute -left-16 -top-24 h-64 w-64 rounded-full bg-[#5b4bdb]/15 blur-3xl" />
          <div className="absolute -right-10 top-8 h-72 w-72 rounded-full bg-[#13a88a]/15 blur-3xl" />
          <div className="absolute bottom-0 left-1/3 h-56 w-56 rounded-full bg-[#f3a847]/20 blur-3xl" />
        </div>

        <div className="relative grid gap-10 p-6 sm:p-10 lg:grid-cols-2 lg:items-center">
          <div className="space-y-5">
            <Badge variant="secondary" className="text-xs uppercase tracking-wider">
              {APP_KICKER}
            </Badge>
            <h1 className="text-3xl font-bold tracking-tight sm:text-4xl lg:text-5xl">
              Think boldly. Build together.
            </h1>
            <p className="text-lg font-semibold text-primary">Every great idea starts somewhere.</p>
            <p className="max-w-prose text-base text-muted-foreground">
              Share a problem worth solving. Discover bold ideas. Connect with people who can help
              bring them forward.
            </p>
            <div className="flex flex-wrap gap-3">
              <Button asChild size="lg">
                <Link to="/ideas">
                  Explore ideas
                  <ArrowRight className="ml-1.5 h-4 w-4" aria-hidden="true" />
                </Link>
              </Button>
              <Button asChild size="lg" variant="outline">
                <Link to={isAuthenticated ? '/app/submissions/new' : '/register'}>
                  Submit an idea
                </Link>
              </Button>
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            {STEPS.map((step) => (
              <Card key={step.title} className="bg-card/90 backdrop-blur">
                <CardContent className="space-y-2 p-5">
                  <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-lavender text-primary">
                    <step.icon className="h-5 w-5" aria-hidden="true" />
                  </span>
                  <h2 className="text-base font-semibold">{step.title}</h2>
                  <p className="text-sm text-muted-foreground">{step.body}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      <FacilitationNotice />

      <section aria-label="Explore by focus" className="grid gap-4 sm:grid-cols-2">
        {SHOWCASES.map((item) => (
          <Link
            key={item.title}
            to={item.to}
            className={cn(
              'group relative overflow-hidden rounded-3xl bg-gradient-to-br p-6 text-white shadow-sm transition-shadow hover:shadow-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
              item.className,
            )}
          >
            <div
              aria-hidden="true"
              className="pointer-events-none absolute -right-8 -top-10 h-36 w-36 rounded-full bg-white/15 blur-2xl"
            />
            <span className="flex h-11 w-11 items-center justify-center rounded-xl bg-white/20">
              <item.icon className="h-6 w-6" aria-hidden="true" />
            </span>
            <h2 className="mt-4 text-xl font-bold">{item.title}</h2>
            <p className="mt-1 text-white/85">{item.body}</p>
            <span className="mt-4 inline-flex items-center gap-1.5 text-sm font-semibold transition-all group-hover:gap-2.5">
              {item.cta}
              <ArrowRight className="h-4 w-4" aria-hidden="true" />
            </span>
          </Link>
        ))}
      </section>
    </div>
  )
}