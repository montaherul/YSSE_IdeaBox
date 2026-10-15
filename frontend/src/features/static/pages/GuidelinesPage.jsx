import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { StaticPage } from '@/features/static/components/StaticBlock'

const FALLBACK_RULES = [
  { title: 'Be respectful', body: 'Critique ideas, never people. Harassment, hate speech and personal attacks are removed.' },
  { title: 'Be constructive', body: 'Reviews must be at least 10 characters and should help the creator improve the idea.' },
  { title: 'No spam or self-promotion', body: 'Do not post unrelated links, advertisements or duplicate content.' },
  { title: 'Respect privacy', body: 'Do not share others\' personal information without consent.' },
  { title: 'One guest review', body: 'Guests may submit a single anonymous review. Create an account to review more.' },
  { title: 'Follow the law', body: 'Do not post illegal content or content that infringes intellectual property.' },
]

export default function GuidelinesPage() {
  useDocumentTitle('Community guidelines')
  return (
    <StaticPage
      key="guidelines"
      title="Community guidelines"
      description="A few simple rules keep the community useful and welcoming for everyone."
      fallbackSections={
        <>
          <div className="grid gap-4 sm:grid-cols-2">
            {FALLBACK_RULES.map((rule) => (
              <div key={rule.title} className="rounded-xl border bg-card p-5 space-y-1.5">
                <h2 className="font-semibold">{rule.title}</h2>
                <p className="text-sm text-muted-foreground">{rule.body}</p>
              </div>
            ))}
          </div>
          <p className="text-sm text-muted-foreground">
            Content that breaks these guidelines may be hidden, removed or moderated, and accounts may
            be suspended.
          </p>
        </>
      }
    />
  )
}
