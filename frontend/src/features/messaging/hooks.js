import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { messagingApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export function useConversations(params) {
  return useQuery({
    queryKey: ['conversations', 'list', params],
    queryFn: () => messagingApi.conversations(params),
    placeholderData: (prev) => prev,
    refetchInterval: 30_000,
  })
}

export function useConversation(id) {
  return useQuery({
    queryKey: ['conversations', id],
    queryFn: () => messagingApi.conversation(id),
    enabled: Boolean(id),
    retry: false,
    refetchInterval: 20_000,
  })
}

export function useSendMessage(conversationId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => messagingApi.sendMessage(conversationId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['conversations', conversationId] })
      qc.invalidateQueries({ queryKey: ['conversations', 'list'] })
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateMessage(conversationId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ messageId, payload }) => messagingApi.updateMessage(conversationId, messageId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['conversations', conversationId] })
      qc.invalidateQueries({ queryKey: ['conversations', 'list'] })
      toast.success('Message updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteMessage(conversationId) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (messageId) => messagingApi.deleteMessage(conversationId, messageId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['conversations', conversationId] })
      qc.invalidateQueries({ queryKey: ['conversations', 'list'] })
      toast.success('Message deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useStartConversation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => messagingApi.startConversation(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['conversations'] })
      toast.success('Conversation started.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useMarkConversationRead() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => messagingApi.markRead(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['conversations'] })
    },
    retry: false,
  })
}

export function useConversationStatus() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => messagingApi.conversationStatus(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['conversations'] })
      toast.success('Conversation updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useBlockUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ userId, payload }) => messagingApi.blockUser(userId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['conversations'] })
      toast.success('User blocked.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useCreateReport() {
  return useMutation({
    mutationFn: (payload) => messagingApi.createReport(payload),
    onSuccess: () => toast.success('Report submitted. Our team will review it.'),
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}