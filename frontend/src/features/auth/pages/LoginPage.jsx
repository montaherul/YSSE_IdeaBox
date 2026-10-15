import { lazy, Suspense } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useLogin } from '@/features/auth/hooks'
import { loginSchema, toErrorMessage } from '@/lib/validation'
import { useAuth } from '@/hooks/useAuth'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { FormField } from '@/components/ui/form-field'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { APP_NAME } from '@/config/constants'
import { Logo } from '@/components/shared/Logo'

// Dev-only demo role picker; the dynamic import is dead-code-eliminated in production builds.
const DemoRoleLogins = import.meta.env.DEV
  ? lazy(() => import('@/features/auth/components/DemoRoleLogins'))
  : null

export default function LoginPage() {
  useDocumentTitle('Sign in')
  const navigate = useNavigate()
  const location = useLocation()
  const { isAuthenticated } = useAuth()
  const login = useLogin()
  const from = location.state?.from || '/dashboard'

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(loginSchema),
    defaultValues: { identifier: '', password: '' },
  })

  if (isAuthenticated) {
    return <Navigate to={from} replace />
  }

  const onSubmit = async (values) => {
    try {
      await login.mutateAsync({
        emailOrPhone: values.identifier,
        password: values.password,
      })
      navigate(from, { replace: true })
    } catch (err) {
      if (err?.status === 401 || err?.status === 400) {
        setError('root', { message: toErrorMessage(err) })
      }
    }
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-col gap-6">
      <div className="flex flex-col items-center gap-2 text-center">
        <Logo size="lg" />
        <h1 className="text-2xl font-bold">Welcome back to {APP_NAME}</h1>
        <p className="text-sm text-muted-foreground">
          Sign in to manage your ideas, reviews and conversations.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Sign in</CardTitle>
          <CardDescription>Use your email or phone number and password.</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
            {errors.root ? (
              <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {errors.root.message}
              </p>
            ) : null}

            <FormField id="identifier" label="Email or phone" required error={errors.identifier?.message}>
              <Input
                id="identifier"
                autoComplete="username"
                placeholder="you@example.com or +880…"
                aria-invalid={Boolean(errors.identifier)}
                {...register('identifier')}
              />
            </FormField>

            <FormField id="password" label="Password" required error={errors.password?.message}>
              <Input
                id="password"
                type="password"
                autoComplete="current-password"
                aria-invalid={Boolean(errors.password)}
                {...register('password')}
              />
            </FormField>

            <Button type="submit" className="w-full" disabled={login.isPending}>
              {login.isPending ? 'Signing in…' : 'Sign in'}
            </Button>
          </form>

          <p className="mt-4 text-center text-sm text-muted-foreground">
            <Link to="/forgot-password" className="font-medium text-primary hover:underline">
              Forgot your password?
            </Link>
          </p>

          <p className="mt-4 text-center text-sm text-muted-foreground">
            New to {APP_NAME}?{' '}
            <Link to="/register" className="font-medium text-primary hover:underline">
              Create a free account
            </Link>
          </p>
        </CardContent>
      </Card>

      {DemoRoleLogins ? (
        <Suspense fallback={null}>
          <DemoRoleLogins onSignIn={() => navigate(from, { replace: true })} />
        </Suspense>
      ) : null}

      <p className="text-center text-xs text-muted-foreground">
        Guests can browse and review one idea anonymously without an account.
      </p>
    </div>
  )
}