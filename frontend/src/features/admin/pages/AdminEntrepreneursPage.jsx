import { useState } from 'react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useEntrepreneurPending, useApproveEntrepreneur } from '@/features/profile/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { EmptyState } from '@/components/shared/EmptyState'
import { Card, CardContent, CardHeader } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { UserAvatar } from '@/components/shared/UserAvatar'
import { CheckCircle } from 'lucide-react'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'
import { Pagination } from '@/components/shared/Pagination'

export default function AdminEntrepreneursPage() {
  useDocumentTitle('Entrepreneur approval')
  const [page, setPage] = useState(1)
  const { data, isLoading, isError } = useEntrepreneurPending({ page, pageSize: DEFAULT_PAGE_SIZE })
  const approve = useApproveEntrepreneur()
  const items = data?.items ?? []
  const total = data?.total ?? 0

  if (isLoading) return <PageSkeleton lines={4} />
  if (isError) return <p className="text-destructive">Failed to load pending entrepreneurs.</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Entrepreneur approval queue"
        description="Review and approve pending business profiles. Approved profiles can access entrepreneur features."
      />

      {items.length === 0 ? (
        <EmptyState
          icon={CheckCircle}
          title="No pending profiles"
          description="All entrepreneur profiles have been reviewed."
        />
      ) : (
        <>
          <div className="space-y-3">
            {items.map((profile) => (
              <Card key={profile.id}>
                <CardHeader className="pb-2">
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <div className="flex items-center gap-3">
                      <UserAvatar name={profile.organization} className="h-10 w-10" />
                      <div>
                        <p className="font-medium">{profile.organization}</p>
                        <p className="text-sm text-muted-foreground">{profile.industry}</p>
                      </div>
                    </div>
                    <Badge variant={profile.isVerified ? 'default' : 'secondary'}>
                      {profile.isVerified ? 'Verified' : 'Unverified'}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent className="space-y-3 pt-0">
                  <div className="flex justify-end gap-2 pt-2 border-t">
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => approve.mutate(profile.userId)}
                      disabled={approve.isPending}
                    >
                      {approve.isPending ? 'Approving...' : 'Approve'}
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
          <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={total} onChange={setPage} />
        </>
      )}
    </div>
  )
}