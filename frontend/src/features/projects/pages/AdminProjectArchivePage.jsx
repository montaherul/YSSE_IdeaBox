import { FolderKanban, Plus } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { usePermission } from '@/hooks/usePermission'
import { projectsApi } from '@/features/projects/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { Button } from '@/components/ui/button'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'
import { formatDate } from '@/lib/format'

/** Admin archive of successfully completed projects ("success stories"). */
export default function AdminProjectArchivePage() {
  useDocumentTitle('Project archive')
  const { canPermission } = usePermission()
  const canCreate = canPermission('projects.create')

  return (
    <div className="space-y-6">
      <PageHeader
        title="Project archive"
        description="Successfully completed projects archived as success stories. Visible to administrators only."
        actions={
          canCreate ? (
            <Button asChild size="sm">
              <Link to="/projects/new">
                <Plus className="h-4 w-4" aria-hidden="true" />
                New project
              </Link>
            </Button>
          ) : null
        }
      />

      <ServerDataTable
        queryKey={['admin', 'project-archive']}
        queryFn={(params) => projectsApi.archivedProjects(params)}
        pageSize={DEFAULT_PAGE_SIZE}
        searchLabel="Search archived projects"
        emptyIcon={FolderKanban}
        emptyTitle="No archived projects"
        emptyDescription="Completed projects archived as successes appear here."
        columns={[
          {
            field: 'name',
            title: 'Project',
            headerSort: false,
            formatter: (cell) => {
              const row = cell.getData()
              return (
                <a href={`/projects/${row.id}`} className="font-medium hover:text-primary">
                  {row.name ?? ''}
                </a>
              )
            },
          },
          {
            field: 'ideaTitle',
            title: 'Idea',
            headerSort: false,
            formatter: (cell) => {
              const row = cell.getData()
              return (
                <a href={`/ideas/${row.ideaSlug}`} className="hover:text-primary">
                  {row.ideaTitle ?? ''}
                </a>
              )
            },
          },
          { field: 'leadDisplayName', title: 'Lead', minWidth: 160, headerSort: false },
          {
            field: 'completedAt',
            title: 'Completed',
            width: 150,
            headerSort: false,
            formatter: (cell) => formatDate(cell.getValue(), { short: true }),
          },
          {
            field: 'archivedAt',
            title: 'Archived',
            width: 150,
            headerSort: false,
            formatter: (cell) => formatDate(cell.getValue(), { short: true }),
          },
        ]}
      />
    </div>
  )
}
