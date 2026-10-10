import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus, Store } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useMyListings, useListingStatus, useListingReservations, useRespondReservation } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { CardSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { formatCurrency, formatDate } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

const NEXT_STATUSES = ['Available', 'Reserved', 'PendingReview', 'Expired', 'Removed']

function Reservations({ listingId }) {
  const { data, isLoading } = useListingReservations(listingId)
  const respond = useRespondReservation()
  const items = data?.items ?? data ?? []
  if (isLoading) return <p className="text-sm text-muted-foreground">Loading reservations…</p>
  if (!items.length) return <p className="text-sm text-muted-foreground">No reservations yet.</p>
  return (
    <ul className="space-y-2">
      {items.map((r) => (
        <li key={r.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg border p-3 text-sm">
          <div className="min-w-0">
            <p className="font-medium">{r.requester?.displayName ?? 'Member'}</p>
            {r.message ? <p className="truncate text-muted-foreground">{r.message}</p> : null}
          </div>
          <div className="flex items-center gap-2">
            <StatusBadge status={r.status} />
            {r.status === 'Requested' ? (
              <>
                <Button size="sm" onClick={() => respond.mutate({ id: r.id, payload: { accept: true } })} disabled={respond.isPending}>
                  Accept
                </Button>
                <Button size="sm" variant="outline" onClick={() => respond.mutate({ id: r.id, payload: { accept: false } })} disabled={respond.isPending}>
                  Decline
                </Button>
              </>
            ) : null}
          </div>
        </li>
      ))}
    </ul>
  )
}

export default function MyListingsPage() {
  useDocumentTitle('My listings')
  const [page, setPage] = useState(1)
  const [pending, setPending] = useState(null)
  const { data, isLoading, isError, refetch } = useMyListings({ page, pageSize: DEFAULT_PAGE_SIZE })
  const changeStatus = useListingStatus()

  const items = data?.items ?? []
  const total = data?.total ?? 0

  return (
    <div className="space-y-6">
      <PageHeader
        title="My listings"
        description="Manage the resources you have shared on the marketplace."
        actions={
          <Button asChild>
            <Link to="/marketplace/new">
              <Plus className="h-4 w-4" aria-hidden="true" />
              New listing
            </Link>
          </Button>
        }
      />

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2">
          <CardSkeleton />
          <CardSkeleton />
        </div>
      ) : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState icon={Store} title="You have no listings" description="Share a resource to get started." action={<Button asChild><Link to="/marketplace/new">Create listing</Link></Button>} />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="space-y-4">
            {items.map((listing) => (
              <Card key={listing.id}>
                <CardHeader className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                  <div>
                    <CardTitle className="text-base">
                      <Link to={`/marketplace/${listing.id}`} className="hover:text-primary">
                        {listing.title}
                      </Link>
                    </CardTitle>
                    <p className="mt-1 text-xs text-muted-foreground">
                      {listing.price != null ? formatCurrency(listing.price) : 'Contact for details'} · {formatDate(listing.createdAt, { short: true })}
                    </p>
                  </div>
                  <StatusBadge status={listing.status} />
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="flex flex-wrap gap-2">
                    {NEXT_STATUSES.filter((s) => s !== listing.status).map((status) => (
                      <Button key={status} size="sm" variant="outline" onClick={() => setPending({ id: listing.id, status })}>
                        Mark {status}
                      </Button>
                    ))}
                  </div>
                  <div>
                    <h3 className="mb-2 text-sm font-semibold">Reservations</h3>
                    <Reservations listingId={listing.id} />
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={total} onChange={setPage} />
        </>
      ) : null}

      <ConfirmDialog
        open={Boolean(pending)}
        onOpenChange={(open) => !open && setPending(null)}
        title="Change listing status?"
        description={pending ? `This will mark the listing as ${pending.status}.` : ''}
        confirmLabel="Update status"
        variant="default"
        onConfirm={() => {
          if (pending) changeStatus.mutate({ id: pending.id, payload: { status: pending.status } })
        }}
      />
    </div>
  )
}
