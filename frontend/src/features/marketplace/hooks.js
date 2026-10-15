import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { marketplaceApi } from './api'
import { toErrorMessage } from '@/lib/validation'

export function useListingList(params, enabled = true) {
  return useQuery({
    queryKey: ['marketplace', 'list', params],
    queryFn: () => marketplaceApi.list(params),
    enabled,
    placeholderData: (prev) => prev,
  })
}

export function useListing(id) {
  return useQuery({
    queryKey: ['marketplace', id],
    queryFn: () => marketplaceApi.byId(id),
    enabled: Boolean(id),
    retry: false,
  })
}

export function useMyListings(params) {
  return useQuery({
    queryKey: ['marketplace', 'mine', params],
    queryFn: () => marketplaceApi.mine(params),
    placeholderData: (prev) => prev,
  })
}

export function useCreateListing() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload) => marketplaceApi.create(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['marketplace'] })
      toast.success('Listing published.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useUpdateListing() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => marketplaceApi.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['marketplace'] })
      toast.success('Listing updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useDeleteListing() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => marketplaceApi.remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['marketplace'] })
      toast.success('Listing deleted.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useListingStatus() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => marketplaceApi.status(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['marketplace'] })
      toast.success('Listing status updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useCreateReservation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => marketplaceApi.createReservation(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['marketplace'] })
      toast.success('Reservation requested.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}

export function useListingReservations(id) {
  return useQuery({
    queryKey: ['marketplace', id, 'reservations'],
    queryFn: () => marketplaceApi.listingReservations(id),
    enabled: Boolean(id),
  })
}

export function useRespondReservation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }) => marketplaceApi.respondReservation(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['marketplace'] })
      toast.success('Reservation updated.')
    },
    onError: (e) => {
      if (!e?.status) toast.error(toErrorMessage(e))
    },
  })
}