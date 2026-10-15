import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useSearchParams } from 'react-router-dom'
import { useResetPassword } from '@/features/auth/hooks'
import { resetPasswordSchema, toErrorMessage } from '@/lib/validation'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { FormField } from '@/components/ui/form-field'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { APP_NAME } from '@/config/constants'
import { Logo } from '@/components/shared/Logo'

export default function ResetPasswordPage() {
  useDocumentTitle('Set a new password')
  const [searchParams] = useSearchParams()
  const reset = useResetPassword()

  const form = useForm({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      email: searchParams.get('email') || '',
      code: '',
      password: '',
      confirmPassword: '',
    },
  })

  const onSubmit = async (values) => {
    try {
      await reset.mutateAsync({
        email: values.email,
        code: values.code,
        newPassword: values.password,
      })
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-col gap-6">
      <div className="flex flex-col items-center gap-2 text-center">
        <Logo size="lg" />
        <h1 className="text-2xl font-bold">Set a new password</h1>
        <p className="text-sm text-muted-foreground">
          Enter the reset code from your email and choose a new password for {APP_NAME}.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Reset password</CardTitle>
          <CardDescription>Codes expire in 15 minutes and can be used once.</CardDescription>
        </CardHeader>
        <CardContent>
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

            <FormField id="code" label="Reset code" required error={form.formState.errors.code?.message}>
              <Input
                id="code"
                inputMode="numeric"
                autoComplete="one-time-code"
                placeholder="123456"
                aria-invalid={Boolean(form.formState.errors.code)}
                {...form.register('code')}
              />
            </FormField>

            <FormField id="password" label="New password" required error={form.formState.errors.password?.message}>
              <Input
                id="password"
                type="password"
                autoComplete="new-password"
                aria-invalid={Boolean(form.formState.errors.password)}
                {...form.register('password')}
              />
            </FormField>

            <FormField
              id="confirm-password"
              label="Confirm new password"
              required
              error={form.formState.errors.confirmPassword?.message}
            >
              <Input
                id="confirm-password"
                type="password"
                autoComplete="new-password"
                aria-invalid={Boolean(form.formState.errors.confirmPassword)}
                {...form.register('confirmPassword')}
              />
            </FormField>

            <Button type="submit" className="w-full" disabled={reset.isPending}>
              {reset.isPending ? 'Resetting…' : 'Reset password'}
            </Button>
          </form>

          <p className="mt-4 text-center text-sm text-muted-foreground">
            Didn&apos;t get a code?{' '}
            <Link to="/forgot-password" className="font-medium text-primary hover:underline">
              Request a new one
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  )
}
