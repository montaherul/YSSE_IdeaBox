import { Link } from 'react-router-dom'
import { FolderKanban } from 'lucide-react'
import { useMyProjects } from '@/features/projects/hooks'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { EmptyState } from '@/components/shared/EmptyState'
import { ListSkeleton } from '@/components/shared/LoadingSkeleton'

/**
 * Projects assigned to the signed-in user by staff (as the selected lead, a project
 * member, or the creator of the originating idea) — shown on both the member and
 * entrepreneur dashboards.
 */
export function MyProjectsPanel() {
  const { data, isLoading, isError } = useMyProjects()
  const items = data?.items ?? data ?? []

  if (isLoading) return <ListSkeleton count={3} />
  if (isError) return null
  if (items.length === 0) {
    return (
      <EmptyState
        compact
        icon={FolderKanban}
        title="No projects yet"
        description="Projects the YSSE team assigns to you appear here."
      />
    )
  }

  return (
    <ul aria-label="My projects" className="space-y-2">
      {items.map((project) => (
        <li key={project.id}>
          <Link
            to={`/projects/${project.id}`}
            className="flex items-center justify-between gap-2 rounded-lg border p-3 text-sm hover:bg-accent"
          >
            <span className="font-medium">{project.name}</span>
            <StatusBadge status={project.status} />
          </Link>
        </li>
      ))}
    </ul>
  )
}
