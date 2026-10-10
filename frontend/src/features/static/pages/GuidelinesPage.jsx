import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { PageHeader } from '@/components/shared/PageHeader'
import { Card, CardContent } from '@/components/ui/card'

const RULES = [
  { title: 'Be respectful', body: 'Critique ideas, never people. Harassment, hate speech and personal attacks are removed.' },
  { title: 'Be constructive', body: 'Reviews must be at least 10 characters and should help the creator improve the idea.' },
  { title: 'No spam or self-promotion', body: 'Do not post unrelated links, advertisements or duplicate content.' },
  { title: 'Respect privacy', body: 'Do not share others’ personal information without consent.' },
  { title: 'One guest review', body: 'Guests may submit a single anonymous review. Create an account to review more.' },
  { title: 'Follow the law', body: 'Do not post illegal content or content that infringes intellectual property.' },
]

export default function GuidelinesPage() {
  useDocumentTitle('Community guidelines')
  return (
    <div className="space-y-6">
      <PageHeader
        title="Community guidelines"
        description="A few simple rules keep the community useful and welcoming for everyone."
      />
      <div className="grid gap-4 sm:grid-cols-2">
        {RULES.map((rule) => (
          <Card key={rule.title}>
            <CardContent className="space-y-1.5 p-5">
              <h2 className="font-semibold">{rule.title}</h2>
              <p className="text-sm text-muted-foreground">{rule.body}</p>
            </CardContent>
          </Card>
        ))}
      </div>
      <p className="text-sm text-muted-foreground">
        Content that breaks these guidelines may be hidden, removed or moderated, and accounts may
        be suspended.
      </p>
    </div>
  )
}
