import { Link, useParams, useSearchParams } from 'react-router-dom'
import { Sparkles } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useCategories } from '@/features/categories/hooks'
import { useIdeaList } from '@/features/ideas/hooks'
import { IdeaCard } from '@/features/ideas/components/IdeaCard'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { Button } from '@/components/ui/button'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

/** Public category landing: every idea in one category (spec route /categories/:slug). */
export default function CategoryDetailPage() {
  const { slug } = useParams()
  useDocumentTitle('Category ideas')
  const [searchParams, setSearchParams] = useSearchParams()
  const page = Number(searchParams.get('page')) || 1

  const { data: categories, isLoading: categoriesLoading } = useCategories()
  const list = categories?.items ?? categories ?? []
  const category = list.find(
    (c) => c.slug?.toLowerCase() === slug?.toLowerCase(),
  )

  const { data, isLoading, isError, error, refetch } = useIdeaList(
    {
      categoryId: category?.id,
      status: 'Published',
      sort: 'newest',
      page,
      pageSize: DEFAULT_PAGE_SIZE,
    },
    Boolean(category),
  )

  const items = data?.items ?? []
  const total = data?.total ?? 0

  if (categoriesLoading) return <ListSkeleton count={4} />

  if (!category) {
    return (
      <EmptyState
        icon={Sparkles}
        title="Category not found"
        description="The category you are looking for does not exist."
        action={
          <Button asChild variant="outline">
            <Link to="/ideas">Explore ideas</Link>
          </Button>
        }
      />
    )
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={category.name}
        description={category.description ?? `Browse published ideas in ${category.name}.`}
      />

      {isLoading ? <ListSkeleton count={6} /> : null}

      {isError ? <ErrorState onRetry={refetch} message={error?.message} /> : null}

      {!isLoading && !isError && items.length === 0 ? (
        <EmptyState
          icon={Sparkles}
          title="No ideas yet"
          description={`No published ideas in ${category.name} yet. Be the first to contribute.`}
          action={
            <Button asChild variant="outline">
              <Link to="/app/submissions/new">Submit an idea</Link>
            </Button>
          }
        />
      ) : null}

      {!isLoading && !isError && items.length > 0 ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((idea) => (
              <IdeaCard key={idea.id ?? idea.slug} idea={idea} />
            ))}
          </div>
          <Pagination
            page={page}
            pageSize={DEFAULT_PAGE_SIZE}
            total={total}
            onChange={(p) => setSearchParams({ page: String(p) })}
          />
        </>
      ) : null}
    </div>
  )
}