import { Search, X } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'

/**
 * Accessible search field: always labelled, icon is decorative,
 * and an optional clear button appears when there is a value.
 */
export function SearchInput({
  id = 'search',
  label = 'Search',
  value,
  onChange,
  onClear,
  placeholder = 'Search…',
  className,
  ...props
}) {
  return (
    <div className={cn('relative w-full', className)}>
      <label htmlFor={id} className="sr-only">
        {label}
      </label>
      <Search
        className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
        aria-hidden="true"
      />
      <Input
        id={id}
        type="search"
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        className="pl-9 pr-9"
        autoComplete="off"
        {...props}
      />
      {value ? (
        <Button
          type="button"
          variant="ghost"
          size="icon"
          onClick={onClear}
          aria-label="Clear search"
          className="absolute right-0.5 top-1/2 h-7 w-7 -translate-y-1/2"
        >
          <X className="h-4 w-4" aria-hidden="true" />
        </Button>
      ) : null}
    </div>
  )
}

export default SearchInput
