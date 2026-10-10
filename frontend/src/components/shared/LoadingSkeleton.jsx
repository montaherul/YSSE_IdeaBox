import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'

/** Card skeleton used while ideas/projects/… load. */
export function CardSkeleton({ className }) {
  return (
    <div className={cn('rounded-xl border bg-card p-5 shadow-sm', className)}>
      <div className="flex items-center justify-between">
        <Skeleton className="h-5 w-24" />
        <Skeleton className="h-5 w-16 rounded-full" />
      </div>
      <Skeleton className="mt-3 h-4 w-3/4" />
      <Skeleton className="mt-2 h-4 w-full" />
      <Skeleton className="mt-2 h-4 w-1/2" />
      <div className="mt-4 flex items-center gap-4">
        <Skeleton className="h-4 w-16" />
        <Skeleton className="h-4 w-16" />
        <Skeleton className="h-4 w-20" />
      </div>
    </div>
  )
}

export function ListSkeleton({ count = 6, className }) {
  return (
    <div className={cn('grid gap-4 sm:grid-cols-2 lg:grid-cols-3', className)} aria-hidden="true">
      {Array.from({ length: count }).map((_, i) => (
        <CardSkeleton key={i} />
      ))}
    </div>
  )
}

export function PageSkeleton({ lines = 3 }) {
  return (
    <div className="space-y-4" aria-hidden="true">
      <Skeleton className="h-8 w-1/3" />
      <Skeleton className="h-4 w-2/3" />
      {Array.from({ length: lines }).map((_, i) => (
        <Skeleton key={i} className="h-4 w-full" />
      ))}
    </div>
  )
}