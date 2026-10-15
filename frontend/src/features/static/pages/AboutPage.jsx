import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { StaticPage } from '@/features/static/components/StaticBlock'

const FALLBACK = [
  {
    title: 'Our mission',
    body: 'YSSE Idea Box exists to surface ideas worth pursuing and connect the people behind them with entrepreneurs, mentors and community support. We believe good ideas should be visible, discussable and discoverable.',
  },
  {
    title: 'What we do',
    body: 'We provide a public feed for ideas and problems, structured screening, community reviews and voting, a marketplace for resources, and facilitation of introductions between creators and entrepreneurs.',
  },
  {
    title: 'What we do not do',
    body: 'We do not manage funding, hold money, or guarantee investment. Negotiations, agreements and transfers happen directly between the parties involved.',
  },
]

export default function AboutPage() {
  useDocumentTitle('About')
  return (
    <StaticPage
      key="about"
      title="About YSSE Idea Box"
      description="From ideas to entrepreneurship."
      fallbackSections={
        <div className="grid gap-4">
          {FALLBACK.map((s) => (
            <div key={s.title} className="rounded-xl border bg-card p-6 space-y-2">
              <h2 className="text-lg font-semibold">{s.title}</h2>
              <p className="text-sm text-muted-foreground">{s.body}</p>
            </div>
          ))}
        </div>
      }
    />
  )
}
