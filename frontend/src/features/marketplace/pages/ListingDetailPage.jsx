import { Link, useNavigate, useParams } from 'react-router-dom'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { MapPin, Calendar, Send, Pencil, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { reservationSchema, toErrorMessage } from '@/lib/validation'
import { useListing, useCreateReservation, useDeleteListing } from '@/features/marketplace/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { formatCurrency, formatDate } from '@/lib/format'

export default function ListingDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { isAuthenticated, user } = useAuth()
  const { data: listing, isLoading, isError, error } = useListing(id)
  const reserve = useCreateReservation()
  const remove = useDeleteListing()
  const [confirmingDelete, setConfirmingDelete] = useState(false)
  useDocumentTitle(listing?.title ? `${listing.title} — Listing` : 'Listing')

  const form = useForm({
    resolver: zodResolver(reservationSchema),
    defaultValues: { message: '' },
  })

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState message={error?.message} />
  if (!listing) return <ErrorState title="Listing not found" message="This listing may have been removed." />

  const isOwner = isAuthenticated && user?.id === listing.ownerUserId

  const onSubmit = async (values) => {
    try {
      await reserve.mutateAsync({ id: listing.id, payload: values })
      form.reset()
      toast.success('Reservation requested.')
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={listing.title}
        description={listing.category ? `Category: ${listing.category}` : undefined}
        actions={
          <div className="flex flex-wrap items-center gap-2">
            {isOwner ? (
              <>
                <Button asChild variant="outline">
                  <Link to={`/marketplace/${listing.id}/edit`}>
                    <Pencil className="h-4 w-4" aria-hidden="true" />
                    Edit
                  </Link>
                </Button>
                <Button variant="outline" onClick={() => setConfirmingDelete(true)}>
                  <Trash2 className="h-4 w-4" aria-hidden="true" />
                  Delete
                </Button>
              </>
            ) : null}
            <StatusBadge status={listing.status} />
          </div>
        }
      />

      <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
        <Card>
          <CardContent className="space-y-4 p-6">
            <p className="whitespace-pre-line text-sm text-muted-foreground">{listing.description}</p>
            <dl className="grid gap-3 text-sm sm:grid-cols-2">
              <div className="flex items-center justify-between gap-2 rounded-lg border p-3">
                <dt className="text-muted-foreground">Type</dt>
                <dd className="font-medium">{listing.listingType ?? '—'}</dd>
              </div>
              <div className="flex items-center justify-between gap-2 rounded-lg border p-3">
                <dt className="text-muted-foreground">Condition</dt>
                <dd className="font-medium">{listing.condition ?? '—'}</dd>
              </div>
              <div className="flex items-center justify-between gap-2 rounded-lg border p-3">
                <dt className="text-muted-foreground">Price</dt>
                <dd className="font-medium">{listing.price != null ? formatCurrency(listing.price) : 'Contact'}</dd>
              </div>
              <div className="flex items-center justify-between gap-2 rounded-lg border p-3">
                <dt className="text-muted-foreground">Listed</dt>
                <dd className="font-medium">{formatDate(listing.createdAt, { short: true })}</dd>
              </div>
            </dl>
            <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
              {listing.coarseLocation ? (
                <span className="inline-flex items-center gap-1">
                  <MapPin className="h-4 w-4" aria-hidden="true" />
                  {listing.coarseLocation}
                </span>
              ) : null}
              {listing.owner?.displayName ? (
                <span className="inline-flex items-center gap-1">
                  <Calendar className="h-4 w-4" aria-hidden="true" />
                  Listed by {listing.owner.displayName}
                </span>
              ) : null}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Request this item</CardTitle>
          </CardHeader>
          <CardContent>
            {isAuthenticated ? (
              <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-3">
                {form.formState.errors.root ? (
                  <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                    {form.formState.errors.root.message}
                  </p>
                ) : null}
                <FormField
                  id="reservation-message"
                  label="Message to the owner (optional)"
                  error={form.formState.errors.message?.message}
                >
                  <Textarea id="reservation-message" rows={4} placeholder="Introduce yourself and what you need." {...form.register('message')} />
                </FormField>
                <Button type="submit" className="w-full" disabled={reserve.isPending}>
                  <Send className="h-4 w-4" aria-hidden="true" />
                  {reserve.isPending ? 'Sending…' : 'Request reservation'}
                </Button>
              </form>
            ) : (
              <p className="text-sm text-muted-foreground">
                <Link to="/login" className="text-primary hover:underline">
                  Sign in
                </Link>{' '}
                to request this item from the owner.
              </p>
            )}
          </CardContent>
        </Card>
      </div>

      <FacilitationNotice />

      <ConfirmDialog
        open={confirmingDelete}
        onOpenChange={setConfirmingDelete}
        title="Delete this listing?"
        description="This listing will be removed for everyone. This cannot be undone."
        confirmLabel="Delete listing"
        onConfirm={() => {
          remove.mutate(listing.id, {
            onSuccess: () => navigate('/marketplace/mine'),
          })
        }}
      />
    </div>
  )
}
