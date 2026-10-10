import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { PageHeader } from '@/components/shared/PageHeader'
import { Card, CardContent } from '@/components/ui/card'

const SECTIONS = [
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
    <div className="space-y-6">
      <PageHeader title="About YSSE Idea Box" description="From ideas to entrepreneurship." />
      <div className="grid gap-4">
        {SECTIONS.map((s) => (
          <Card key={s.title}>
            <CardContent className="space-y-2 p-6">
              <h2 className="text-lg font-semibold">{s.title}</h2>
              <p className="text-sm text-muted-foreground">{s.body}</p>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
