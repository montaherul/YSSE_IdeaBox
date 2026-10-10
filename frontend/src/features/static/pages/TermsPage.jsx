import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { PageHeader } from '@/components/shared/PageHeader'

const SECTIONS = [
  { title: 'Using the platform', body: 'You must provide accurate registration information and keep your account secure. You are responsible for activity under your account.' },
  { title: 'Content ownership', body: 'You retain ownership of the content you post and grant YSSE Idea Box a licence to display it on the platform.' },
  { title: 'Acceptable use', body: 'You agree not to post unlawful content, harass others, or attempt to disrupt the service. Moderation decisions are final.' },
  { title: 'No funding relationship', body: 'YSSE Idea Box facilitates connections only. It is not a party to any agreement between members and does not manage funding.' },
  { title: 'Changes', body: 'We may update these terms to reflect new features or legal requirements. Continued use means you accept the updated terms.' },
]

export default function TermsPage() {
  useDocumentTitle('Terms of use')
  return (
    <div className="max-w-3xl space-y-4">
      <PageHeader title="Terms of use" description="The ground rules for using YSSE Idea Box." />
      {SECTIONS.map((s) => (
        <section key={s.title} className="space-y-1">
          <h2 className="text-base font-semibold">{s.title}</h2>
          <p className="text-sm text-muted-foreground">{s.body}</p>
        </section>
      ))}
    </div>
  )
}
