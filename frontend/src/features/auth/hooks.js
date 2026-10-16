import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { authApi } from './api'
import { useAuthStore } from '@/store/auth-store'
import { toErrorMessage } from '@/lib/validation'

export function useLogin() {
  const setAuth = useAuthStore((s) => s.setAuth)
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: (payload) => authApi.login(payload),
    onSuccess: (data) => {
      const tokens = data?.accessToken || data?.data ? data : null
      const { accessToken, refreshToken, user } = tokens ?? {}
      if (accessToken && refreshToken) {
        // Drop any cached data from a previous account before establishing the new session.
        queryClient.clear()
        setAuth(accessToken, refreshToken, user)
        toast.success('Welcome back!')
      } else {
        toast.success('Signed in successfully.')
      }
    },
    onError: (error) => {
      if (!error?.status) {
        toast.error(toErrorMessage(error))
      }
    },
  })
}

export function useRegister() {
  const setAuth = useAuthStore((s) => s.setAuth)

  return useMutation({
    mutationFn: (payload) => authApi.register(payload),
    onSuccess: (data) => {
      const tokens = data?.accessToken || data?.refreshToken ? data : data?.data
      if (tokens?.accessToken && tokens?.refreshToken) {
        setAuth(tokens.accessToken, tokens.refreshToken, tokens.user)
      }
      toast.success('Account created. Please verify your email to continue.')
    },
    onError: (error) => {
      if (!error?.status) toast.error(toErrorMessage(error))
    },
  })
}

export function useAuthMe({ enabled = true } = {}) {
  const setUser = useAuthStore((s) => s.setUser)
  return useQuery({
    queryKey: ['auth', 'me'],
    queryFn: async () => {
      const data = await authApi.me()
      if (data?.user) setUser(data.user)
      else if (data?.id || data?.roles) setUser(data)
      return data
    },
    enabled,
    retry: false,
    staleTime: 60_000,
  })
}

export function useLogout() {
  const logout = useAuthStore((s) => s.logout)
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: () => authApi.logout().catch(() => null),
    onSettled: () => {
      logout()
      // Clear cached server state so a different account never sees stale private data.
      queryClient.clear()
      toast.info('Signed out.')
      navigate('/')
    },
  })
}

export function useRequestVerification() {
  return useMutation({
    mutationFn: (payload) => authApi.requestVerify(payload),
    onSuccess: () => toast.success('Verification code sent.'),
    onError: (error) => {
      if (!error?.status) toast.error(toErrorMessage(error))
    },
  })
}

export function useVerifyAccount() {
  return useMutation({
    mutationFn: (payload) => authApi.verify(payload),
    onSuccess: () => toast.success('Account verified.'),
    onError: (error) => {
      if (!error?.status) toast.error(toErrorMessage(error))
    },
  })
}

export function useForgotPassword() {
  return useMutation({
    mutationFn: (payload) => authApi.forgotPassword(payload),
    onSuccess: () => toast.success('If that account exists, a reset code has been sent.'),
  })
}

export function useResetPassword() {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: (payload) => authApi.resetPassword(payload),
    onSuccess: () => {
      toast.success('Password reset. Sign in with your new password.')
      navigate('/login', { replace: true })
    },
  })
}