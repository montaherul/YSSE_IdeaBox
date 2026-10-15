import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { StaticPage } from '@/features/static/components/StaticBlock'

const FALLBACK = [
  { title: 'Information we collect', body: 'Account details (display name, email, optional phone), profile information you choose to provide, and content you post such as ideas, reviews, comments and listings.' },
  { title: 'How we use it', body: 'To operate the platform, show your contributions, enable connections between members, and keep the community safe through moderation.' },
  { title: 'Sharing', body: 'Public contributions are visible to other members and visitors. We do not sell your personal data.' },
  { title: 'Your choices', body: 'You can edit or delete your profile and creator profile, control per-field visibility, and request account support through the contact page.' },
  { title: 'Security', body: 'Passwords are stored using strong hashing. Access is protected by authentication and role-based authorization enforced on the server.' },
]

export default function PrivacyPage() {
  useDocumentTitle('Privacy policy')
  return (
    <StaticPage
      key="privacy"
      title="Privacy policy"
      description="How YSSE Idea Box handles your information."
      fallbackSections={
        <>
          <div className="prose-sm max-w-3xl space-y-4">
            {FALLBACK.map((s) => (
              <section key={s.title} className="space-y-1">
                <h2 className="text-base font-semibold">{s.title}</h2>
                <p className="text-sm text-muted-foreground">{s.body}</p>
              </section>
            ))}
            <p className="text-xs text-muted-foreground">
              This is a summary for the community. For formal requests, contact us.
            </p>
          </div>
        </>
      }
    />
  )
}
