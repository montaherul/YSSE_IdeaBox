import { Outlet } from 'react-router-dom'
import { Sidebar } from './Sidebar'
import { Topbar } from './Topbar'
import { useUiStore } from '@/store/ui-store'
import { useMediaQuery } from '@/hooks/useMediaQuery'

/** Shared authenticated workspace: topbar + collapsible sidebar + content outlet. */
export function WorkspaceLayout() {
  const sidebarOpen = useUiStore((s) => s.sidebarOpen)
  const setSidebarOpen = useUiStore((s) => s.setSidebarOpen)
  const isDesktop = useMediaQuery('(min-width: 1024px)')

  return (
    <div className="min-h-screen bg-background">
      <Topbar onMenuClick={() => setSidebarOpen(!sidebarOpen)} />

      <div className="mx-auto flex w-full max-w-[1440px]">
        {isDesktop ? (
          <aside className="sticky top-14 hidden max-h-[calc(100vh-3.5rem)] w-72 shrink-0 overflow-y-auto border-r bg-card lg:block">
            <Sidebar />
          </aside>
        ) : null}

        <main id="main-content" className="min-w-0 flex-1 px-3 py-6 sm:px-6 lg:px-8">
          <Outlet />
        </main>
      </div>

      {/* Mobile sidebar drawer */}
      {!isDesktop && sidebarOpen ? (
        <div className="fixed inset-0 z-40 lg:hidden">
          <button
            type="button"
            className="absolute inset-0 bg-black/50"
            onClick={() => setSidebarOpen(false)}
            aria-label="Close navigation menu"
            tabIndex={-1}
          />
          <aside className="absolute inset-y-0 left-0 w-72 overflow-y-auto border-r bg-card shadow-xl">
            <Sidebar onNavigate={() => setSidebarOpen(false)} />
          </aside>
        </div>
      ) : null}
    </div>
  )
}