import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import { KeyRound } from 'lucide-react'
import { useForgotPassword } from '@/features/auth/hooks'
import { forgotPasswordSchema, toErrorMessage } from '@/lib/validation'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { FormField } from '@/components/ui/form-field'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { APP_NAME } from '@/config/constants'
import { Logo } from '@/components/shared/Logo'

export default function ForgotPasswordPage() {
  useDocumentTitle('Forgot password')
  const forgot = useForgotPassword()
  const [sentTo, setSentTo] = useState(null)
  const [devCode, setDevCode] = useState(null)

  const form = useForm({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: '' },
  })

  const onSubmit = async (values) => {
    try {
      const res = await forgot.mutateAsync({ email: values.email })
      setSentTo(values.email)
      if (res?.resetCode) setDevCode(res.resetCode)
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-col gap-6">
      <div className="flex flex-col items-center gap-2 text-center">
        <Logo size="lg" />
        <h1 className="text-2xl font-bold">Reset your {APP_NAME} password</h1>
        <p className="text-sm text-muted-foreground">
          Enter your account email and we&apos;ll send you a reset code.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Forgot password</CardTitle>
          <CardDescription>
            {sentTo
              ? 'Check your inbox for the reset code to continue.'
              : 'We use the email address on your account.'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {sentTo ? (
            <div className="space-y-4">
              <p className="flex items-start gap-2 rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                <KeyRound className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
                If an account exists for <strong>{sentTo}</strong>, a reset code has been sent. The
                code expires in 15 minutes.
              </p>

              {devCode ? (
                <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  Development build: your reset code is{' '}
                  <strong className="font-mono">{devCode}</strong>
                </p>
              ) : null}

              <Button asChild className="w-full">
                <Link to={`/reset-password?email=${encodeURIComponent(sentTo)}`}>
                  Enter reset code
                </Link>
              </Button>
            </div>
          ) : (
            <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
              {form.formState.errors.root ? (
                <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {form.formState.errors.root.message}
                </p>
              ) : null}

              <FormField id="email" label="Email address" required error={form.formState.errors.email?.message}>
                <Input
                  id="email"
                  type="email"
                  autoComplete="email"
                  placeholder="you@example.com"
                  aria-invalid={Boolean(form.formState.errors.email)}
                  {...form.register('email')}
                />
              </FormField>

              <Button type="submit" className="w-full" disabled={forgot.isPending}>
                {forgot.isPending ? 'Sending…' : 'Send reset code'}
              </Button>
            </form>
          )}

          <p className="mt-4 text-center text-sm text-muted-foreground">
            Remembered it?{' '}
            <Link to="/login" className="font-medium text-primary hover:underline">
              Back to sign in
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  )
}
