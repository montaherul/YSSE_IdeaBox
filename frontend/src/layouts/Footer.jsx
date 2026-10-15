import { Link } from 'react-router-dom'
import { APP_NAME } from '@/config/constants'
import { Logo } from '@/components/shared/Logo'

/** Shared site footer used across public and auth layouts. */
export function Footer() {
  return (
    <footer className="border-t bg-muted/30">
      <div className="mx-auto flex max-w-6xl flex-col gap-4 px-4 py-8 sm:flex-row sm:items-start sm:justify-between">
        <div className="max-w-sm">
          <p className="flex items-center gap-2 text-sm font-semibold">
            <Logo size="sm" />
            {APP_NAME}
          </p>
          <p className="mt-2 text-sm text-muted-foreground">
            From ideas to entrepreneurship. YSSE facilitates connections; it does not manage
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
            Leaderboards
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
  )
}

export default Footer
