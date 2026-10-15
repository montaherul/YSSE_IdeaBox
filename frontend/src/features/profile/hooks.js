import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { profileApi } from './api'
import { toErrorMessage } from '@/lib/validation'
import { useAuthStore } from '@/store/auth-store'

function applyAvatarToAuthStore(imageUrl) {
  const { user, setUser } = useAuthStore.getState()
  if (user) setUser({ ...user, imageUrl: imageUrl ?? null })
}

export function useUploadAvatar() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (file) => profileApi.uploadAvatar(file),
    onSuccess: (imageUrl) => {
      applyAvatarToAuthStore(imageUrl)
      qc.invalidateQueries({ queryKey: ['profiles', 'member'] })
      qc.invalidateQueries({ queryKey: ['auth', 'me'] })
      toast.success('Profile picture updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useRemoveAvatar() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => profileApi.removeAvatar(),
    onSuccess: () => {
      applyAvatarToAuthStore(null)
      qc.invalidateQueries({ queryKey: ['profiles', 'member'] })
      qc.invalidateQueries({ queryKey: ['auth', 'me'] })
      toast.success('Profile picture removed.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useMemberProfile(enabled = true) {
  return useQuery({
    queryKey: ['profiles', 'member'],
    queryFn: () => profileApi.member(),
    enabled,
  })
}

export function useUpdateMemberProfile() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => profileApi.updateMember(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['profiles', 'member'] })
      qc.invalidateQueries({ queryKey: ['auth', 'me'] })
      toast.success('Profile saved.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useCreatorProfile(enabled = true) {
  return useQuery({
    queryKey: ['profiles', 'creator'],
    queryFn: () => profileApi.creator(),
    enabled,
  })
}

export function useCreatorProfileByUser(userId) {
  return useQuery({
    queryKey: ['profiles', 'creator', userId],
    queryFn: () => profileApi.creatorByUser(userId),
    enabled: Boolean(userId),
    retry: false,
  })
}

export function useUpdateCreatorProfile() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => profileApi.updateCreator(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['profiles', 'creator'] })
      qc.invalidateQueries({ queryKey: ['auth', 'me'] })
      toast.success('Creator profile saved.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteCreatorProfile() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => profileApi.deleteCreator(),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['profiles', 'creator'] })
      qc.invalidateQueries({ queryKey: ['auth', 'me'] })
      toast.success('Creator profile deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useEntrepreneurProfile(enabled = true) {
  return useQuery({
    queryKey: ['profiles', 'entrepreneur'],
    queryFn: () => profileApi.entrepreneur(),
    enabled,
  })
}

export function useUpdateEntrepreneurProfile() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => profileApi.updateEntrepreneur(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['profiles', 'entrepreneur'] })
      qc.invalidateQueries({ queryKey: ['auth', 'me'] })
      toast.success('Business profile saved.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useEntrepreneurPending() {
  return useQuery({
    queryKey: ['profiles', 'entrepreneur', 'pending'],
    queryFn: () => profileApi.entrepreneurPending(),
    staleTime: 60_000,
  })
}

export function useApproveEntrepreneur() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (userId) => profileApi.approveEntrepreneur(userId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['profiles', 'entrepreneur', 'pending'] })
      qc.invalidateQueries({ queryKey: ['admin', 'users'] })
      toast.success('Entrepreneur profile approved.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}