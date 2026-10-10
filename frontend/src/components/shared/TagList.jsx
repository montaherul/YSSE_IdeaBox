import { Tag } from './Tag'
import { cn } from '@/lib/utils'

/** Renders a list of tag chips, optionally clickable and removable. */
export function TagList({ tags = [], onTagClick, onRemove, max, className }) {
  const list = Array.isArray(tags) ? tags : []
  const visible = max ? list.slice(0, max) : list
  if (visible.length === 0) return null

  return (
    <ul className={cn('flex flex-wrap gap-1.5', className)} aria-label="Tags">
      {visible.map((tag) => (
        <li key={tag}>
          <Tag
            onRemove={onRemove ? () => onRemove(tag) : undefined}
            className="p-0"
          >
            {onTagClick ? (
              <button
                type="button"
                onClick={() => onTagClick(tag)}
                className="rounded-full px-2.5 py-0.5 text-xs font-medium hover:underline"
              >
                #{tag}
              </button>
            ) : (
              <span className="px-2.5 py-0.5 text-xs">#{tag}</span>
            )}
          </Tag>
        </li>
      ))}
    </ul>
  )
}

export default TagList
