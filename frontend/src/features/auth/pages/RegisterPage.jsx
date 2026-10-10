import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from 'react-router-dom'
import { MailCheck, Lightbulb } from 'lucide-react'
import { useRegister, useVerifyAccount } from '../hooks'
import { registerSchema, verifyAccountSchema, toErrorMessage } from '@/lib/validation'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Checkbox } from '@/components/ui/checkbox'
import { FormField } from '@/components/ui/form-field'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { APP_NAME } from '@/config/constants'

export default function RegisterPage() {
  useDocumentTitle('Create an account')
  const navigate = useNavigate()
  const register = useRegister()
  const verify = useVerifyAccount()
  const [pendingEmail, setPendingEmail] = useState(null)
  const [devCode, setDevCode] = useState(null)

  const form = useForm({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      displayName: '',
      email: '',
      phoneNumber: '',
      password: '',
      confirmPassword: '',
      acceptTerms: false,
    },
  })

  const verifyForm = useForm({
    resolver: zodResolver(verifyAccountSchema),
    defaultValues: { email: '', code: '' },
  })

  const onSubmit = async (values) => {
    try {
      const res = await register.mutateAsync({
        displayName: values.displayName,
        email: values.email,
        phoneNumber: values.phoneNumber || undefined,
        password: values.password,
        acceptTerms: values.acceptTerms,
      })
      setPendingEmail(values.email)
      verifyForm.setValue('email', values.email)
      if (res?.verificationCode) {
        setDevCode(res.verificationCode)
        verifyForm.setValue('code', res.verificationCode)
      }
      form.reset()
    } catch (err) {
      if (err?.status) form.setError('root', { message: toErrorMessage(err) })
    }
  }

  const onVerify = async (values) => {
    try {
      await verify.mutateAsync({ email: values.email, code: values.code })
      navigate('/app', { replace: true })
    } catch (err) {
      if (err?.status) verifyForm.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-col gap-6">
      <div className="flex flex-col items-center gap-2 text-center">
        <span className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary text-primary-foreground">
          <Lightbulb className="h-6 w-6" aria-hidden="true" />
        </span>
        <h1 className="text-2xl font-bold">Join {APP_NAME}</h1>
        <p className="text-sm text-muted-foreground">
          Share ideas, gather feedback and connect with collaborators.
        </p>
      </div>

      {pendingEmail ? (
        <Card>
          <CardHeader>
            <CardTitle>Verify your email</CardTitle>
            <CardDescription>
              <MailCheck className="mr-1 inline h-4 w-4" aria-hidden="true" />
              We sent a 6-digit code to <strong>{pendingEmail}</strong>. Enter it below to activate
              your account.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={verifyForm.handleSubmit(onVerify)} noValidate className="space-y-4">
              {verifyForm.formState.errors.root ? (
                <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {verifyForm.formState.errors.root.message}
                </p>
              ) : null}

              {devCode ? (
                <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  Development build: your verification code is <strong className="font-mono">{devCode}</strong>
                </p>
              ) : null}

              <FormField id="verify-code" label="Verification code" required error={verifyForm.formState.errors.code?.message}>
                <Input
                  id="verify-code"
                  inputMode="numeric"
                  autoComplete="one-time-code"
                  placeholder="123456"
                  aria-invalid={Boolean(verifyForm.formState.errors.code)}
                  {...verifyForm.register('code')}
                />
              </FormField>

              <Button type="submit" className="w-full" disabled={verify.isPending}>
                {verify.isPending ? 'Verifying…' : 'Verify my account'}
              </Button>
            </form>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>Create your account</CardTitle>
            <CardDescription>
              Registration is open to staff, students and the public community.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
              {form.formState.errors.root ? (
                <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {form.formState.errors.root.message}
                </p>
              ) : null}

              <FormField id="display-name" label="Display name" required error={form.formState.errors.displayName?.message}>
                <Input
                  id="display-name"
                  autoComplete="name"
                  placeholder="How you want to appear publicly"
                  aria-invalid={Boolean(form.formState.errors.displayName)}
                  {...form.register('displayName')}
                />
              </FormField>

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

              <FormField id="phone" label="Phone number (optional)" error={form.formState.errors.phoneNumber?.message}>
                <Input
                  id="phone"
                  type="tel"
                  autoComplete="tel"
                  placeholder="+8801XXXXXXXXX"
                  aria-invalid={Boolean(form.formState.errors.phoneNumber)}
                  {...form.register('phoneNumber')}
                />
              </FormField>

              <FormField id="password" label="Password" required error={form.formState.errors.password?.message}>
                <Input
                  id="password"
                  type="password"
                  autoComplete="new-password"
                  aria-invalid={Boolean(form.formState.errors.password)}
                  {...form.register('password')}
                />
              </FormField>

              <FormField id="confirm-password" label="Confirm password" required error={form.formState.errors.confirmPassword?.message}>
                <Input
                  id="confirm-password"
                  type="password"
                  autoComplete="new-password"
                  aria-invalid={Boolean(form.formState.errors.confirmPassword)}
                  {...form.register('confirmPassword')}
                />
              </FormField>

              <div className="flex items-start gap-3">
                <Controller
                  control={form.control}
                  name="acceptTerms"
                  render={({ field }) => (
                    <Checkbox
                      id="accept-terms"
                      checked={Boolean(field.value)}
                      onCheckedChange={(checked) => field.onChange(checked === true)}
                      onBlur={field.onBlur}
                      aria-invalid={Boolean(form.formState.errors.acceptTerms)}
                    />
                  )}
                />
                <label htmlFor="accept-terms" className="text-sm text-muted-foreground">
                  I agree to the{' '}
                  <Link to="/terms" className="text-primary hover:underline">
                    Terms of Use
                  </Link>{' '}
                  and{' '}
                  <Link to="/privacy" className="text-primary hover:underline">
                    Privacy Policy
                  </Link>
                  {form.formState.errors.acceptTerms ? (
                    <span className="mt-1 block text-sm text-destructive" role="alert">
                      {form.formState.errors.acceptTerms.message}
                    </span>
                  ) : null}
                </label>
              </div>

              <Button type="submit" className="w-full" disabled={register.isPending}>
                {register.isPending ? 'Creating account…' : 'Create account'}
              </Button>
            </form>

            <p className="mt-4 text-center text-sm text-muted-foreground">
              Already have an account?{' '}
              <Link to="/login" className="font-medium text-primary hover:underline">
                Sign in
              </Link>
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  )
}