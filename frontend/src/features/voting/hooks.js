import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { votingApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export function useIdeaVotes(ideaId) {
  return useQuery({
    queryKey: ['votes', ideaId],
    queryFn: () => votingApi.votes(ideaId),
    enabled: Boolean(ideaId),
  })
}

export function useCastVote(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => votingApi.castVote(ideaId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['votes', ideaId] })
      qc.invalidateQueries({ queryKey: ['ideas', 'id', ideaId] })
      qc.invalidateQueries({ queryKey: ['ideas', 'list'] })
      qc.invalidateQueries({ queryKey: ['leaderboards'] })
      toast.success('Vote cast.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useRemoveVote(ideaId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => votingApi.removeVote(ideaId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['votes', ideaId] })
      qc.invalidateQueries({ queryKey: ['ideas'] })
      qc.invalidateQueries({ queryKey: ['leaderboards'] })
      toast.success('Vote removed.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useVotingPeriods(enabled = true) {
  return useQuery({
    queryKey: ['voting-periods'],
    queryFn: () => votingApi.periods(),
    enabled,
    staleTime: 60_000,
  })
}

export function useCreateVotingPeriod() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => votingApi.createPeriod(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['voting-periods'] })
      toast.success('Voting period created.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateVotingPeriod() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => votingApi.updatePeriod(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['voting-periods'] })
      toast.success('Voting period updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteVotingPeriod() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => votingApi.deletePeriod(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['voting-periods'] })
      toast.success('Voting period deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useActivateVotingPeriod() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => votingApi.activatePeriod(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['voting-periods'] })
      toast.success('Voting period activated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useCloseVotingPeriod() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => votingApi.closePeriod(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['voting-periods'] })
      qc.invalidateQueries({ queryKey: ['leaderboards'] })
      toast.success('Voting period closed.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useLeaderboardIdeas(params) {
  return useQuery({
    queryKey: ['leaderboards', 'ideas', params],
    queryFn: () => votingApi.leaderboardIdeas(params),
    staleTime: 5 * 60_000,
  })
}

export function useLeaderboardIdeaMakers(params) {
  return useQuery({
    queryKey: ['leaderboards', 'makers', params],
    queryFn: () => votingApi.leaderboardIdeaMakers(params),
    staleTime: 5 * 60_000,
  })
}

export function useMonthlyTopIdeas(take = 3) {
  return useQuery({
    queryKey: ['leaderboards', 'monthly-top', take],
    queryFn: () => votingApi.monthlyTopIdeas({ take }),
    staleTime: 5 * 60_000,
  })
}

export function useAnnounceMonthlyTop() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => votingApi.announceMonthlyTop(payload),
    onSuccess: (data) => {
      qc.invalidateQueries({ queryKey: ['leaderboards'] })
      const count = data?.notifiedCount ?? 0
      toast.success(
        count > 0
          ? `Appreciation sent to ${count} contributor${count === 1 ? '' : 's'}.`
          : 'No contributors to notify.',
      )
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}