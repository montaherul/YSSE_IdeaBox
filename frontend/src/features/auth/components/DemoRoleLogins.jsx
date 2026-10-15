import { useState } from 'react'
import { useLogin } from '@/features/auth/hooks'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

// Development-only convenience: one-click sign-in for each seeded demo role.
// This module is dynamically imported only when import.meta.env.DEV is true, so it
// (and these credentials) are never part of a production build.
const DEMO_ACCOUNTS = [
  { role: 'Administrator', email: 'admin@ysse.local', password: 'Admin@123' },
  { role: 'Moderator', email: 'moderator@ysse.local', password: 'Moderator@123' },
  { role: 'Creator', email: 'creator@ysse.local', password: 'Creator@123' },
  { role: 'Entrepreneur', email: 'entrepreneur@ysse.local', password: 'Entrepreneur@123' },
  { role: 'Investor', email: 'investor@ysse.local', password: 'Investor@123' },
  { role: 'Member', email: 'member@ysse.local', password: 'Member@123' },
]

export default function DemoRoleLogins({ onSignIn }) {
  const login = useLogin()
  const [pendingEmail, setPendingEmail] = useState(null)

  const signInAs = async (account) => {
    setPendingEmail(account.email)
    try {
      await login.mutateAsync({ emailOrPhone: account.email, password: account.password })
      onSignIn?.()
    } catch {
      // useLogin surfaces non-validation failures via a toast; nothing else to do here.
    } finally {
      setPendingEmail(null)
    }
  }

  return (
    <Card className="border-dashed">
      <CardHeader>
        <CardTitle className="text-base">Demo accounts</CardTitle>
        <CardDescription>
          Development only — sign in instantly as any seeded role. Not included in production builds.
        </CardDescription>
      </CardHeader>
      <CardContent className="grid grid-cols-2 gap-2">
        {DEMO_ACCOUNTS.map((account) => (
          <Button
            key={account.email}
            type="button"
            variant="outline"
            size="sm"
            disabled={login.isPending}
            onClick={() => signInAs(account)}
            aria-label={`Sign in as ${account.role}`}
          >
            {pendingEmail === account.email ? 'Signing in…' : account.role}
          </Button>
        ))}
      </CardContent>
    </Card>
  )
}
