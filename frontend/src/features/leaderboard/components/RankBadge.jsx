import { Trophy, Medal } from 'lucide-react'
import { cn } from '@/lib/utils'

/** Rank indicator for leaderboard rows (medal icons + text, not colour alone). */
export function RankBadge({ rank, className }) {
  const Icon = rank === 1 ? Trophy : rank <= 3 ? Medal : null
  return (
    <span
      className={cn(
        'flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted text-xs font-semibold',
        rank === 1 && 'text-warning',
        rank > 1 && rank <= 3 && 'text-info',
        className,
      )}
      aria-label={`Rank ${rank}`}
    >
      {Icon ? <Icon className="h-4 w-4" aria-hidden="true" /> : rank}
    </span>
  )
}

export default RankBadge