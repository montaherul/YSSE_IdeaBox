import { useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { Plus, Store, MapPin, Tag as TagIcon } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { useListingList } from '../hooks'
import { useDebounce } from '@/hooks/useDebounce'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { SearchInput } from '@/components/shared/SearchInput'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { formatCurrency } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

export default function MarketplacePage() {
  useDocumentTitle('Marketplace')
  const { isAuthenticated } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const [search, setSearch] = useState(searchParams.get('search') ?? '')
  const debounced = useDebounce(search, 300)
  const page = Number(searchParams.get('page')) || 1

  const { data, isLoading, isError, refetch } = useListingList({
    search: debounced || undefined,
    status: 'Available',
    page,
    pageSize: DEFAULT_PAGE_SIZE,
  })

  const items = data?.items ?? []
  const total = data?.total ?? 0

  const setPage = (next) => {
    const params = new URLSearchParams(searchParams)
    if (next <= 1) params.delete('page')
    else params.set('page', String(next))
    setSearchParams(params)
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Marketplace"
        description="Resources, equipment, skills and opportunities shared by the community."
        actions={
          isAuthenticated ? (
            <Button asChild>
              <Link to="/marketplace/new">
                <Plus className="h-4 w-4" aria-hidden="true" />
                Create listing
              </Link>
            </Button>
          ) : null
        }
      />

      <SearchInput
        id="marketplace-search"
        label="Search marketplace"
        value={search}
        onChange={(e) => {
          setSearch(e.target.value)
          setPage(1)
        }}
        onClear={() => setSearch('')}
        placeholder="Search listings…"
      />

      {isLoading ? <ListSkeleton count={6} /> : null}
      {isError ? <ErrorState onRetry={refetch} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState icon={Store} title="No listings found" description="Try a different search or create the first listing." />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((listing) => (
              <Card key={listing.id} className="flex flex-col">
                <CardContent className="flex flex-1 flex-col gap-3 p-5">
                  <div className="flex items-center justify-between gap-2">
                    <span className="rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary">
                      {listing.listingType ?? listing.category ?? 'Listing'}
                    </span>
                    <StatusBadge status={listing.status} />
                  </div>
                  <h2 className="text-base font-semibold leading-snug">
                    <Link to={`/marketplace/${listing.id}`} className="hover:text-primary">
                      {listing.title}
                    </Link>
                  </h2>
                  <p className="line-clamp-3 text-sm text-muted-foreground">{listing.description}</p>
                  <div className="mt-auto flex flex-wrap items-center gap-3 pt-2 text-xs text-muted-foreground">
                    {listing.price != null ? (
                      <span className="font-medium text-foreground">{formatCurrency(listing.price)}</span>
                    ) : (
                      <span className="inline-flex items-center gap-1">
                        <TagIcon className="h-3.5 w-3.5" aria-hidden="true" />
                        Contact for details
                      </span>
                    )}
                    {listing.coarseLocation ? (
                      <span className="inline-flex items-center gap-1">
                        <MapPin className="h-3.5 w-3.5" aria-hidden="true" />
                        {listing.coarseLocation}
                      </span>
                    ) : null}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={total} onChange={setPage} />
        </>
      ) : null}
    </div>
  )
}
