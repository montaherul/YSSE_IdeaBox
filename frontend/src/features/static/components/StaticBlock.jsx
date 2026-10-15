import { useStaticContent } from '../hooks'
import { RichText } from '@/components/ui/rich-text-editor'
import { sanitizeHtml } from '@/lib/sanitize'
import { cn } from '@/lib/utils'

/** Renders CMS-managed content with a fallback to children (hardcoded). */
export function StaticBlock({ key, children, className, titleClassName, bodyClassName }) {
  const { data, isLoading } = useStaticContent(key)

  if (isLoading) {
    return <div className={className}><p className="text-sm text-muted-foreground">Loading…</p></div>
  }

  if (data) {
    return (
      <div className={cn(className)}>
        <RichText html={data.content} className={bodyClassName} />
      </div>
    )
  }

  return children ? (
    <div className={className}>
      {typeof children === 'function' ? children() : children}
    </div>
  ) : null
}

export function StaticPage({ key, title, description, fallbackSections }) {
  const { data, isLoading } = useStaticContent(key)

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="h-6 bg-muted animate-pulse rounded w-1/4" />
        <div className="h-4 bg-muted animate-pulse rounded w-1/2" />
        <div className="grid gap-4">
          <div className="h-32 bg-muted animate-pulse rounded" />
          <div className="h-32 bg-muted animate-pulse rounded" />
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">{data?.title ?? title}</h1>
        <p className="text-muted-foreground">{data?.title ? data.title : description}</p>
      </div>
      {data ? (
        <RichText html={data.content} />
      ) : (
        fallbackSections
      )}
    </div>
  )
}