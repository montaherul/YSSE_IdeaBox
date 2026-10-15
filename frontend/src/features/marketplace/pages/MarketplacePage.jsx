import { Link } from 'react-router-dom'
import { Plus, Store, MapPin, Tag as TagIcon } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { marketplaceApi } from '@/features/marketplace/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { ResourceCardGrid } from '@/features/_shared'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { formatCurrency } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

export default function MarketplacePage() {
  useDocumentTitle('Marketplace')
  const { isAuthenticated } = useAuth()

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

      <ResourceCardGrid
        queryKey={['marketplace', 'list']}
        queryFn={(params) => marketplaceApi.list({ status: 'Available', ...params })}
        getKey={(listing) => listing.id}
        pageSize={DEFAULT_PAGE_SIZE}
        searchable
        searchLabel="Search marketplace"
        emptyIcon={Store}
        emptyTitle="No listings found"
        emptyDescription="Try a different search or create the first listing."
        renderCard={(listing) => (
          <Card className="flex h-full flex-col">
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
        )}
      />
    </div>
  )
}
