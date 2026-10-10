import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, renderHook, waitFor, act } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter, useLocation } from 'react-router-dom'

vi.mock('@/features/auth/api', () => ({
  authApi: {
    login: vi.fn(),
    forgotPassword: vi.fn(),
    resetPassword: vi.fn(),
    logout: vi.fn(),
  },
}))

vi.mock('sonner', () => ({
  toast: { success: vi.fn(), error: vi.fn(), info: vi.fn() },
}))

import { authApi } from '@/features/auth/api'
import { toast } from 'sonner'
import { useForgotPassword, useLogin, useResetPassword } from '@/features/auth/hooks'
import { useAuthStore } from '@/store/auth-store'

function createWrapper(initialEntries = ['/']) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
  return function Wrapper({ children }) {
    return (
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={initialEntries}>{children}</MemoryRouter>
      </QueryClientProvider>
    )
  }
}

beforeEach(() => {
  vi.clearAllMocks()
  useAuthStore.setState({ accessToken: null, refreshToken: null, user: null })
})

describe('useForgotPassword', () => {
  it('submits the email and confirms without revealing account existence', async () => {
    authApi.forgotPassword.mockResolvedValue({})
    const { result } = renderHook(() => useForgotPassword(), { wrapper: createWrapper() })

    await act(async () => {
      await result.current.mutateAsync({ email: 'user@example.com' })
    })

    expect(authApi.forgotPassword).toHaveBeenCalledWith({ email: 'user@example.com' })
    expect(toast.success).toHaveBeenCalledWith(
      'If that account exists, a reset code has been sent.',
    )
  })
})

describe('useResetPassword', () => {
  function Harness() {
    const mutation = useResetPassword()
    const location = useLocation()
    return (
      <button onClick={() => mutation.mutate({ email: 'u@e.com', code: '1', password: 'p' })}>
        reset:{location.pathname}
      </button>
    )
  }

  it('navigates to the login screen after a successful reset', async () => {
    authApi.resetPassword.mockResolvedValue({})
    const { getByText } = render(<Harness />, { wrapper: createWrapper(['/reset-password']) })

    await act(async () => {
      getByText(/^reset:/).click()
    })

    await waitFor(() => expect(getByText(/\/login$/)).toBeTruthy())
    expect(toast.success).toHaveBeenCalledWith('Password reset. Sign in with your new password.')
  })
})

describe('useLogin', () => {
  it('stores the session when tokens are returned', async () => {
    authApi.login.mockResolvedValue({
      accessToken: 'access-1',
      refreshToken: 'refresh-1',
      user: { id: 7, displayName: 'Ada' },
    })

    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() })

    await act(async () => {
      await result.current.mutateAsync({ identifier: 'ada', password: 'secret' })
    })

    const state = useAuthStore.getState()
    expect(state.accessToken).toBe('access-1')
    expect(state.refreshToken).toBe('refresh-1')
    expect(state.user).toMatchObject({ id: 7 })
    expect(toast.success).toHaveBeenCalledWith('Welcome back!')
  })

  it('does not double-report server errors as toasts', async () => {
    const serverError = Object.assign(new Error('Invalid credentials'), { status: 401 })
    authApi.login.mockRejectedValue(serverError)

    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() })

    await act(async () => {
      await result.current.mutateAsync({ identifier: 'ada', password: 'wrong' }).catch(() => {})
    })

    expect(toast.error).not.toHaveBeenCalled()
  })
})
