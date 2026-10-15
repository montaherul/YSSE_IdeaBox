import { useState } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import {
  Bell,
  Globe,
  LogOut,
  Menu,
  Moon,
  Search,
  Settings,
  ShieldCheck,
  Sun,
  User,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { mediaUrl } from '@/lib/media'
import { useAuth } from '@/hooks/useAuth'
import { usePermission } from '@/hooks/usePermission'
import { useUiStore } from '@/store/ui-store'
import { useLocaleStore, SUPPORTED_LOCALES } from '@/store/locale-store'
import { UserAvatar } from '@/components/shared/Avatar'
import { Logo } from '@/components/shared/Logo'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Separator } from '@/components/ui/separator'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'

export function Topbar({ onMenuClick }) {
  const { user, isAuthenticated, logout } = useAuth()
  const { canAccessAdmin } = usePermission()
  const theme = useUiStore((s) => s.theme)
  const toggleTheme = useUiStore((s) => s.toggleTheme)
  const navigate = useNavigate()
  const [search, setSearch] = useState('')

  const submitSearch = (e) => {
    e.preventDefault()
    if (search.trim()) {
      navigate(`/ideas?q=${encodeURIComponent(search.trim())}`)
      setSearch('')
    }
  }

  return (
    <header className="sticky top-0 z-30 flex h-14 items-center gap-2 border-b bg-background/95 px-3 backdrop-blur sm:px-5">
      <Button
        variant="ghost"
        size="icon"
        className="lg:hidden"
        onClick={onMenuClick}
        aria-label="Toggle navigation menu"
      >
        <Menu className="h-5 w-5" aria-hidden="true" />
      </Button>

      <Link to="/" className="flex items-center gap-2 lg:hidden">
        <Logo size="sm" />
      </Link>

      <form onSubmit={submitSearch} className="relative ml-1 hidden flex-1 max-w-md sm:block" role="search">
        <label htmlFor="topbar-search" className="sr-only">
          Search ideas
        </label>
        <Search className="pointer-events-none absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" aria-hidden="true" />
        <Input
          id="topbar-search"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search ideas…"
          className="pl-8"
          autoComplete="off"
        />
      </form>

      <nav aria-label="Quick links" className="ml-auto flex items-center gap-1">
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" aria-label="Change language">
              <Globe className="h-4 w-4" aria-hidden="true" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-36">
            {SUPPORTED_LOCALES.map((locale) => (
              <DropdownMenuItem
                key={locale.code}
                onClick={() => useLocaleStore.getState().setLocale(locale.code)}
                className={useLocaleStore.getState().locale === locale.code ? 'bg-primary/10' : ''}
              >
                <span className="flex items-center gap-2">
                  <span>{locale.nativeName}</span>
                  {useLocaleStore.getState().locale === locale.code && (
                    <span className="ml-auto text-primary">✓</span>
                  )}
                </span>
              </DropdownMenuItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenu>

        <Button variant="ghost" size="icon" onClick={toggleTheme} aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} theme`}>
          {theme === 'dark' ? <Sun className="h-4 w-4" aria-hidden="true" /> : <Moon className="h-4 w-4" aria-hidden="true" />}
        </Button>

        {isAuthenticated ? (
          <>
            <Button variant="ghost" size="icon" asChild aria-label="Notifications">
              <Link to="/app/notifications">
                <Bell className="h-4 w-4" aria-hidden="true" />
              </Link>
            </Button>

            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="gap-2 px-1.5" aria-label="Account menu">
                  <UserAvatar name={user?.displayName} src={mediaUrl(user?.imageUrl)} className="h-8 w-8" />
                  <span className="hidden max-w-[120px] truncate text-sm font-medium md:inline">
                    {user?.displayName}
                  </span>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel>
                  {user?.displayName}
                  <span className="block text-xs font-normal text-muted-foreground">{user?.email}</span>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild>
                  <Link to="/app">
                    <User className="h-4 w-4" aria-hidden="true" />
                    Dashboard
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link to="/app/profile">
                    <Settings className="h-4 w-4" aria-hidden="true" />
                    Settings
                  </Link>
                </DropdownMenuItem>
                {canAccessAdmin() ? (
                  <DropdownMenuItem asChild>
                    <Link to="/admin">
                      <ShieldCheck className="h-4 w-4" aria-hidden="true" />
                      Administration
                    </Link>
                  </DropdownMenuItem>
                ) : null}
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  onClick={() => {
                    logout()
                    navigate('/')
                  }}
                >
                  <LogOut className="h-4 w-4" aria-hidden="true" />
                  Sign out
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </>
        ) : (
          <div className="flex items-center gap-2">
            <Button variant="ghost" size="sm" asChild>
              <NavLink to="/login">Sign in</NavLink>
            </Button>
            <Button size="sm" asChild>
              <NavLink to="/register">Join free</NavLink>
            </Button>
          </div>
        )}
      </nav>
      <Separator className="mx-1" />
    </header>
  )
}