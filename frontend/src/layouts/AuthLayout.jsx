import { Link, Outlet } from 'react-router-dom'
import { Lightbulb, Moon, Sun } from 'lucide-react'
import { useUiStore } from '@/store/ui-store'
import { APP_NAME } from '@/config/constants'
import { Button } from '@/components/ui/button'
import { Footer } from './Footer'

/** Minimal centred layout for authentication screens (login/register/verify). */
export function AuthLayout() {
  const theme = useUiStore((s) => s.theme)
  const toggleTheme = useUiStore((s) => s.toggleTheme)

  return (
    <div className="flex min-h-screen flex-col bg-background">
      <header className="border-b">
        <div className="mx-auto flex h-14 max-w-6xl items-center justify-between px-4">
          <Link to="/" className="flex items-center gap-2">
            <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
              <Lightbulb className="h-4 w-4" aria-hidden="true" />
            </span>
            <span className="text-base font-bold tracking-tight">{APP_NAME}</span>
          </Link>
          <Button
            variant="ghost"
            size="icon"
            onClick={toggleTheme}
            aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} theme`}
          >
            {theme === 'dark' ? <Sun className="h-4 w-4" aria-hidden="true" /> : <Moon className="h-4 w-4" aria-hidden="true" />}
          </Button>
        </div>
      </header>

      <main id="main-content" className="mx-auto flex w-full max-w-6xl flex-1 items-center justify-center px-4 py-10">
        <Outlet />
      </main>

      <Footer />
    </div>
  )
}

export default AuthLayout
