import { useState } from 'react'
import { Link, NavLink, Outlet } from 'react-router-dom'
import { ChevronDown, Menu, Moon, Send, Sun, X } from 'lucide-react'
import { cn, slugify } from '@/lib/utils'
import { useAuth } from '@/hooks/useAuth'
import { useCategories } from '@/features/categories/hooks'
import { useUiStore } from '@/store/ui-store'
import { APP_NAME, FALLBACK_CATEGORY_NAMES } from '@/config/constants'
import { Button } from '@/components/ui/button'
import { Logo } from '@/components/shared/Logo'

const NAV_LINK_CLASS =
  'rounded-md px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring'

/** "Discover Ideas" nav item with a hover/focus dropdown of idea categories (desktop). */
function DiscoverMenu() {
  const [open, setOpen] = useState(false)
  const { data } = useCategories()
  const categories = data?.items ?? data ?? []

  const items =
    categories.length > 0
      ? categories.map((category) => ({
          name: category.name ?? category.title ?? 'Category',
          to: `/categories/${category.slug ?? slugify(category.name)}`,
        }))
      : FALLBACK_CATEGORY_NAMES.map((name) => ({
          name,
          to: `/categories/${slugify(name)}`,
        }))

  return (
    <div
      className="relative"
      onMouseEnter={() => setOpen(true)}
      onMouseLeave={() => setOpen(false)}
      onFocus={() => setOpen(true)}
      onBlur={(event) => {
        if (!event.currentTarget.contains(event.relatedTarget)) setOpen(false)
      }}
      onKeyDown={(event) => {
        if (event.key === 'Escape') setOpen(false)
      }}
    >
      <Link
        to="/ideas"
        aria-haspopup="menu"
        aria-expanded={open}
        className={cn(NAV_LINK_CLASS, 'flex items-center gap-1', open && 'text-foreground')}
      >
        Discover Ideas
        <ChevronDown
          className={cn('h-4 w-4 transition-transform', open && 'rotate-180')}
          aria-hidden="true"
        />
      </Link>

      {open ? (
        <div className="absolute left-0 top-full z-40 pt-2">
          <div
            role="menu"
            aria-label="Idea categories"
            className="min-w-[220px] rounded-lg border bg-popover p-1.5 shadow-lg"
          >
            {items.map((item) => (
              <Link
                key={item.to}
                to={item.to}
                role="menuitem"
                onClick={() => setOpen(false)}
                className="block rounded-md px-3 py-2 text-sm text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              >
                {item.name}
              </Link>
            ))}
            <div className="my-1 h-px bg-muted" role="none" />
            <Link
              to="/ideas"
              role="menuitem"
              onClick={() => setOpen(false)}
              className="block rounded-md px-3 py-2 text-sm font-medium text-primary transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            >
              View all ideas
            </Link>
          </div>
        </div>
      ) : null}
    </div>
  )
}

/** Public (signed-out friendly) layout: landing navbar + footer around the outlet. */
export function PublicLayout() {
  const { isAuthenticated } = useAuth()
  const theme = useUiStore((s) => s.theme)
  const toggleTheme = useUiStore((s) => s.toggleTheme)
  const [menuOpen, setMenuOpen] = useState(false)

  const submitHref = isAuthenticated ? '/app/submissions/new' : '/register'

  return (
    <div className="flex min-h-screen flex-col bg-background">
      <header className="sticky top-0 z-30 border-b bg-background/95 backdrop-blur">
        <div className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-3 px-4">
          <Link to="/" className="flex items-center" aria-label={APP_NAME}>
            <Logo size="md" />
          </Link>

          <nav aria-label="Main" className="hidden items-center gap-1 md:flex">
            <DiscoverMenu />
            <Link to="/#how-it-works" className={NAV_LINK_CLASS}>
              How It Works
            </Link>
            <NavLink
              to="/about"
              className={({ isActive }) => cn(NAV_LINK_CLASS, isActive && 'text-foreground')}
            >
              About YSSE
            </NavLink>
          </nav>

          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="icon"
              onClick={toggleTheme}
              aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} theme`}
            >
              {theme === 'dark' ? (
                <Sun className="h-4 w-4" aria-hidden="true" />
              ) : (
                <Moon className="h-4 w-4" aria-hidden="true" />
              )}
            </Button>

            {isAuthenticated ? (
              <Button variant="ghost" size="sm" asChild className="hidden sm:inline-flex">
                <Link to="/app">Dashboard</Link>
              </Button>
            ) : (
              <Button variant="ghost" size="sm" asChild className="hidden sm:inline-flex">
                <Link to="/login">Log in</Link>
              </Button>
            )}

            <Button size="sm" asChild className="hidden sm:inline-flex">
              <Link to={submitHref}>
                <Send aria-hidden="true" />
                Submit an Idea
              </Link>
            </Button>

            <Button
              variant="ghost"
              size="icon"
              className="md:hidden"
              onClick={() => setMenuOpen((value) => !value)}
              aria-expanded={menuOpen}
              aria-controls="mobile-nav"
              aria-label={menuOpen ? 'Close menu' : 'Open menu'}
            >
              {menuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </Button>
          </div>
        </div>

        {/* Mobile nav */}
        {menuOpen ? (
          <nav
            id="mobile-nav"
            aria-label="Main (mobile)"
            className="border-t bg-background px-4 py-3 md:hidden"
          >
            <ul className="flex flex-col gap-1">
              <li>
                <Link
                  to="/ideas"
                  onClick={() => setMenuOpen(false)}
                  className="block rounded-md px-3 py-2 text-sm font-medium text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                >
                  Discover Ideas
                </Link>
              </li>
              <li>
                <Link
                  to="/#how-it-works"
                  onClick={() => setMenuOpen(false)}
                  className="block rounded-md px-3 py-2 text-sm font-medium text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                >
                  How It Works
                </Link>
              </li>
              <li>
                <Link
                  to="/about"
                  onClick={() => setMenuOpen(false)}
                  className="block rounded-md px-3 py-2 text-sm font-medium text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                >
                  About YSSE
                </Link>
              </li>
              <li className="pt-2">
                <Button size="sm" asChild className="w-full">
                  <Link to={submitHref} onClick={() => setMenuOpen(false)}>
                    <Send aria-hidden="true" />
                    Submit an Idea
                  </Link>
                </Button>
              </li>
              <li>
                <Button variant="ghost" size="sm" asChild className="w-full">
                  <Link to={isAuthenticated ? '/app' : '/login'} onClick={() => setMenuOpen(false)}>
                    {isAuthenticated ? 'Dashboard' : 'Log in'}
                  </Link>
                </Button>
              </li>
            </ul>
          </nav>
        ) : null}
      </header>

      <main id="main-content" className="mx-auto w-full max-w-6xl flex-1 px-4 py-8">
        <Outlet />
      </main>

      <footer className="border-t bg-muted/30">
        <div className="mx-auto flex max-w-6xl flex-col gap-8 px-4 py-10 sm:flex-row sm:justify-between">
          <div className="max-w-sm">
            <p className="flex items-center gap-2 text-sm font-semibold">
              <Logo size="sm" />
              {APP_NAME}
            </p>
            <p className="mt-2 text-sm text-muted-foreground">
              Discover bold ideas, share your perspective, and help turn promising solutions into
              real-world impact. YSSE facilitates connections; it does not manage funding.
            </p>
          </div>
          <nav aria-label="Footer" className="grid grid-cols-2 gap-x-10 gap-y-2 text-sm">
            <Link to="/ideas" className="text-muted-foreground hover:text-foreground">
              Discover Ideas
            </Link>
            <Link to="/#how-it-works" className="text-muted-foreground hover:text-foreground">
              How It Works
            </Link>
            <Link to="/about" className="text-muted-foreground hover:text-foreground">
              About YSSE
            </Link>
            <Link to={submitHref} className="text-muted-foreground hover:text-foreground">
              Submit an Idea
            </Link>
            <Link to="/guidelines" className="text-muted-foreground hover:text-foreground">
              Guidelines
            </Link>
            <Link to="/contact" className="text-muted-foreground hover:text-foreground">
              Contact
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
