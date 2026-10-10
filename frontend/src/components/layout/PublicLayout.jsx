import { Link, NavLink, Outlet } from 'react-router-dom'
import { Lightbulb, Moon, Sun } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/useAuth'
import { useUiStore } from '@/store/ui-store'
import { APP_NAME } from '@/config/constants'
import { Button } from '@/components/ui/button'

const PUBLIC_LINKS = [
  { to: '/ideas', label: 'Ideas' },
  { to: '/problems', label: 'Problems' },
  { to: '/leaderboard', label: 'Leaderboard' },
  { to: '/marketplace', label: 'Marketplace' },
]

/** Public (signed-out friendly) layout: landing navbar + footer around the outlet. */
export function PublicLayout() {
  const { isAuthenticated } = useAuth()
  const theme = useUiStore((s) => s.theme)
  const toggleTheme = useUiStore((s) => s.toggleTheme)

  return (
    <div className="flex min-h-screen flex-col bg-background">
      <header className="sticky top-0 z-30 border-b bg-background/95 backdrop-blur">
        <div className="mx-auto flex h-14 max-w-6xl items-center justify-between gap-3 px-4">
          <Link to="/" className="flex items-center gap-2">
            <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
              <Lightbulb className="h-4 w-4" aria-hidden="true" />
            </span>
            <span className="text-base font-bold tracking-tight">{APP_NAME}</span>
          </Link>

          <nav aria-label="Main" className="hidden items-center gap-1 md:flex">
            {PUBLIC_LINKS.map((link) => (
              <NavLink
                key={link.to}
                to={link.to}
                className={({ isActive }) =>
                  cn(
                    'rounded-md px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                    isActive && 'text-foreground',
                  )
                }
              >
                {link.label}
              </NavLink>
            ))}
          </nav>

          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="icon"
              onClick={toggleTheme}
              aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} theme`}
            >
              {theme === 'dark' ? <Sun className="h-4 w-4" aria-hidden="true" /> : <Moon className="h-4 w-4" aria-hidden="true" />}
            </Button>

            {isAuthenticated ? (
              <Button asChild size="sm">
                <Link to="/app">Dashboard</Link>
              </Button>
            ) : (
              <>
                <Button variant="ghost" size="sm" asChild>
                  <Link to="/login">Sign in</Link>
                </Button>
                <Button size="sm" asChild>
                  <Link to="/register">Join free</Link>
                </Button>
              </>
            )}
          </div>
        </div>
        {/* Mobile nav */}
        <nav aria-label="Main (mobile)" className="flex overflow-x-auto border-t px-2 py-1 md:hidden">
          {PUBLIC_LINKS.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) =>
                cn(
                  'whitespace-nowrap rounded-md px-3 py-1.5 text-sm font-medium text-muted-foreground',
                  isActive && 'bg-accent text-accent-foreground',
                )
              }
            >
              {link.label}
            </NavLink>
          ))}
        </nav>
      </header>

      <main id="main-content" className="mx-auto w-full max-w-6xl flex-1 px-4 py-8">
        <Outlet />
      </main>

      <footer className="border-t bg-muted/30">
        <div className="mx-auto flex max-w-6xl flex-col gap-4 px-4 py-8 sm:flex-row sm:items-start sm:justify-between">
          <div className="max-w-sm">
            <p className="flex items-center gap-2 text-sm font-semibold">
              <Lightbulb className="h-4 w-4" aria-hidden="true" />
              {APP_NAME}
            </p>
            <p className="mt-2 text-sm text-muted-foreground">
              Think boldly, build together. YSSE facilitates connections; it does not manage
              funding.
            </p>
          </div>
          <nav aria-label="Footer" className="grid grid-cols-2 gap-x-8 gap-y-2 text-sm sm:grid-cols-3">
            <Link to="/about" className="text-muted-foreground hover:text-foreground">
              About
            </Link>
            <Link to="/guidelines" className="text-muted-foreground hover:text-foreground">
              Guidelines
            </Link>
            <Link to="/contact" className="text-muted-foreground hover:text-foreground">
              Contact
            </Link>
            <Link to="/marketplace" className="text-muted-foreground hover:text-foreground">
              Marketplace
            </Link>
            <Link to="/leaderboard" className="text-muted-foreground hover:text-foreground">
              Leaderboard
            </Link>
            <Link to="/problems" className="text-muted-foreground hover:text-foreground">
              Problems
            </Link>
          </nav>
        </div>
        <div className="border-t">
          <div className="mx-auto flex max-w-6xl flex-col gap-2 px-4 py-4 text-xs text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
            <p>© {new Date().getFullYear()} YSSE Idea Box. Community-driven idea discovery.</p>
            <div className="flex gap-4">
              <Link to="/privacy" className="hover:text-foreground">
                Privacy policy
              </Link>
              <Link to="/terms" className="hover:text-foreground">
                Terms of use
              </Link>
            </div>
          </div>
        </div>
      </footer>
    </div>
  )
}