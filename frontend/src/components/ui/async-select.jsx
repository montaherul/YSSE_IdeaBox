import * as React from 'react'
import * as PopoverPrimitive from '@radix-ui/react-popover'
import { Check, ChevronDown, Loader2, Search, X } from 'lucide-react'
import { cn } from '@/lib/utils'

/**
 * Generic searchable select ("Select2"-style) for React.
 *
 * Two data modes:
 *  - Local:  pass `options` — the list is filtered in the browser. Use for small,
 *            static collections.
 *  - Remote: pass `loadOptions({ search, page, pageSize })` — options are fetched
 *            from the server with debounced search and "load more" pagination. Use
 *            for large collections that must not be loaded into the browser.
 *
 * Supports single and multiple selection. The value is the caller's own option
 * object(s) (e.g. `[{ id, code }]`), so already-selected items keep their label
 * even when they are not part of the current search results.
 *
 * Accessibility: trigger is a `combobox`; the option list is a `listbox`
 * (`aria-multiselectable` when multiple). Arrow keys move the active option and
 * Enter toggles it; Escape closes. This only drives selection — the API remains
 * the authoritative authorization boundary.
 */
function useDebouncedValue(value, delay) {
  const [debounced, setDebounced] = React.useState(value)
  React.useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay)
    return () => clearTimeout(timer)
  }, [value, delay])
  return debounced
}

const defaultGetValue = (option) =>
  option && typeof option === 'object' ? option.value : option

const defaultGetLabel = (option) => {
  if (option && typeof option === 'object') {
    return option.label ?? option.name ?? option.code ?? String(option.value ?? '')
  }
  return String(option ?? '')
}

function AsyncSelect({
  id,
  multiple = false,
  value,
  onChange,
  options,
  loadOptions,
  getOptionValue = defaultGetValue,
  getOptionLabel = defaultGetLabel,
  renderOption,
  placeholder = 'Select…',
  searchPlaceholder = 'Search…',
  emptyMessage = 'No options found.',
  loadingMessage = 'Loading…',
  clearable = true,
  disabled = false,
  pageSize = 20,
  debounceMs = 300,
  className,
  contentClassName,
  'aria-invalid': ariaInvalid,
  'aria-describedby': ariaDescribedBy,
}) {
  const isRemote = typeof loadOptions === 'function'

  const [open, setOpen] = React.useState(false)
  const [search, setSearch] = React.useState('')
  const debouncedSearch = useDebouncedValue(search, debounceMs)
  const [remoteOptions, setRemoteOptions] = React.useState([])
  const [page, setPage] = React.useState(1)
  const [hasMore, setHasMore] = React.useState(false)
  const [loading, setLoading] = React.useState(false)
  const [error, setError] = React.useState(null)
  const [activeIndex, setActiveIndex] = React.useState(0)

  const searchRef = React.useRef(null)
  const baseId = React.useId()
  const requestIdRef = React.useRef(0)
  const loadOptionsRef = React.useRef(loadOptions)

  React.useEffect(() => {
    loadOptionsRef.current = loadOptions
  })

  const selected = React.useMemo(() => {
    if (multiple) return Array.isArray(value) ? value.filter((item) => item != null) : []
    return value == null || value === '' ? [] : [value]
  }, [value, multiple])

  const selectedValues = React.useMemo(
    () => selected.map((option) => getOptionValue(option)),
    [selected, getOptionValue],
  )

  const loadRemote = React.useCallback(
    async (term, nextPage, append) => {
      const loader = loadOptionsRef.current
      if (!loader) return
      const requestId = ++requestIdRef.current
      setLoading(true)
      setError(null)
      try {
        const result = await loader({ search: term, page: nextPage, pageSize })
        if (requestId !== requestIdRef.current) return
        const items = result?.options ?? result?.items ?? []
        setRemoteOptions((prev) => (append ? [...prev, ...items] : items))
        setHasMore(Boolean(result?.hasNextPage ?? result?.hasNext ?? items.length >= pageSize))
        setPage(nextPage)
      } catch (err) {
        if (requestId !== requestIdRef.current) return
        setError(err?.message ?? 'Could not load options.')
        if (!append) setRemoteOptions([])
        setHasMore(false)
      } finally {
        if (requestId === requestIdRef.current) setLoading(false)
      }
    },
    [pageSize],
  )

  React.useEffect(() => {
    if (!open || !isRemote) return
    loadRemote(debouncedSearch, 1, false)
  }, [open, isRemote, debouncedSearch, loadRemote])

  React.useEffect(() => {
    setActiveIndex(0)
  }, [debouncedSearch, open])

  const visibleOptions = React.useMemo(() => {
    if (isRemote) return remoteOptions
    const list = Array.isArray(options) ? options : []
    const term = debouncedSearch.trim().toLowerCase()
    if (!term) return list
    return list.filter((option) => getOptionLabel(option).toLowerCase().includes(term))
  }, [isRemote, remoteOptions, options, debouncedSearch, getOptionLabel])

  const isSelected = (option) =>
    selectedValues.some((optionValue) => optionValue === getOptionValue(option))

  const toggleOption = (option) => {
    if (disabled) return
    const optionValue = getOptionValue(option)
    if (multiple) {
      const next = selectedValues.some((v) => v === optionValue)
        ? selected.filter((item) => getOptionValue(item) !== optionValue)
        : [...selected, option]
      onChange?.(next)
    } else {
      onChange?.(option)
      setOpen(false)
    }
  }

  const removeOption = (option) => (event) => {
    event?.stopPropagation()
    event?.preventDefault()
    const optionValue = getOptionValue(option)
    onChange?.(selected.filter((item) => getOptionValue(item) !== optionValue))
  }

  const clearAll = (event) => {
    event?.stopPropagation()
    event?.preventDefault()
    onChange?.(multiple ? [] : null)
  }

  const handleOpenChange = (next) => {
    setOpen(next)
    if (!next) {
      setSearch('')
      setActiveIndex(0)
    }
  }

  const handleSearchKeyDown = (event) => {
    if (event.key === 'ArrowDown') {
      event.preventDefault()
      setActiveIndex((index) => Math.min(index + 1, visibleOptions.length - 1))
    } else if (event.key === 'ArrowUp') {
      event.preventDefault()
      setActiveIndex((index) => Math.max(index - 1, 0))
    } else if (event.key === 'Enter') {
      event.preventDefault()
      const option = visibleOptions[activeIndex]
      if (option) toggleOption(option)
    }
  }

  const hasSelection = selected.length > 0
  const listboxId = `${baseId}-listbox`
  const activeOptionId =
    visibleOptions.length > 0 && visibleOptions[activeIndex]
      ? `${baseId}-opt-${activeIndex}`
      : undefined

  return (
    <PopoverPrimitive.Root open={open} onOpenChange={handleOpenChange}>
      <PopoverPrimitive.Trigger
        id={id}
        type="button"
        role="combobox"
        aria-expanded={open}
        aria-haspopup="listbox"
        aria-controls={listboxId}
        aria-invalid={ariaInvalid || undefined}
        aria-describedby={ariaDescribedBy}
        disabled={disabled}
        className={cn(
          'flex min-h-9 w-full items-center justify-between gap-1 rounded-md border border-input bg-transparent px-3 py-1.5 text-sm shadow-sm focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-1 disabled:cursor-not-allowed disabled:opacity-50',
          className,
        )}
      >
        <span className="flex flex-1 flex-wrap items-center gap-1 overflow-hidden text-left">
          {hasSelection ? (
            multiple ? (
              selected.map((option) => (
                <span
                  key={getOptionValue(option)}
                  className="inline-flex max-w-full items-center gap-1 rounded bg-secondary px-1.5 py-0.5 text-xs"
                >
                  <span className="truncate">{getOptionLabel(option)}</span>
                  <span
                    role="button"
                    tabIndex={-1}
                    aria-label={`Remove ${getOptionLabel(option)}`}
                    className="cursor-pointer text-muted-foreground hover:text-foreground"
                    onClick={removeOption(option)}
                  >
                    <X className="h-3 w-3" />
                  </span>
                </span>
              ))
            ) : (
              <span className="truncate">{getOptionLabel(selected[0])}</span>
            )
          ) : (
            <span className="truncate text-muted-foreground">{placeholder}</span>
          )}
        </span>
        <span className="flex shrink-0 items-center gap-1">
          {clearable && hasSelection ? (
            <span
              role="button"
              tabIndex={-1}
              aria-label="Clear selection"
              className="cursor-pointer text-muted-foreground hover:text-foreground"
              onClick={clearAll}
            >
              <X className="h-3.5 w-3.5" />
            </span>
          ) : null}
          <ChevronDown className="h-4 w-4 opacity-50" aria-hidden="true" />
        </span>
      </PopoverPrimitive.Trigger>

      <PopoverPrimitive.Portal>
        <PopoverPrimitive.Content
          align="start"
          sideOffset={4}
          className={cn(
            'z-50 w-[var(--radix-popover-trigger-width)] min-w-[12rem] overflow-hidden rounded-md border bg-popover text-popover-foreground shadow-md',
            contentClassName,
          )}
          onOpenAutoFocus={(event) => {
            event.preventDefault()
            searchRef.current?.focus()
          }}
        >
          <div className="flex items-center gap-2 border-b px-3 py-2">
            <Search className="h-4 w-4 shrink-0 opacity-50" aria-hidden="true" />
            <input
              ref={searchRef}
              type="text"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              onKeyDown={handleSearchKeyDown}
              placeholder={searchPlaceholder}
              aria-label={searchPlaceholder}
              aria-controls={listboxId}
              aria-autocomplete="list"
              autoComplete="off"
              className="flex-1 bg-transparent text-sm outline-none placeholder:text-muted-foreground"
            />
            {loading ? (
              <Loader2 className="h-4 w-4 shrink-0 animate-spin opacity-60" aria-hidden="true" />
            ) : null}
          </div>

          <div
            id={listboxId}
            role="listbox"
            aria-multiselectable={multiple || undefined}
            className="max-h-60 overflow-y-auto p-1"
          >
            {visibleOptions.length === 0 ? (
              <p className="px-3 py-6 text-center text-sm text-muted-foreground">
                {loading ? loadingMessage : error || emptyMessage}
              </p>
            ) : (
              visibleOptions.map((option, index) => {
                const optionValue = getOptionValue(option)
                const selectedOption = isSelected(option)
                return (
                  <div
                    key={optionValue}
                    role="option"
                    aria-selected={selectedOption}
                    className={cn(
                      'flex cursor-pointer items-center gap-2 rounded-sm px-2 py-1.5 text-sm',
                      index === activeIndex && 'bg-accent text-accent-foreground',
                    )}
                    onMouseEnter={() => setActiveIndex(index)}
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => toggleOption(option)}
                  >
                    <span
                      className={cn(
                        'flex h-4 w-4 shrink-0 items-center justify-center rounded border',
                        selectedOption
                          ? 'border-primary bg-primary text-primary-foreground'
                          : 'border-input',
                      )}
                    >
                      {selectedOption ? <Check className="h-3 w-3" aria-hidden="true" /> : null}
                    </span>
                    <span className="flex-1 truncate">
                      {renderOption ? renderOption(option) : getOptionLabel(option)}
                    </span>
                  </div>
                )
              })
            )}
          </div>

          {isRemote && hasMore ? (
            <div className="border-t p-1">
              <button
                type="button"
                className="w-full rounded-sm px-2 py-1.5 text-center text-sm text-muted-foreground hover:bg-accent hover:text-accent-foreground disabled:opacity-50"
                onClick={() => loadRemote(debouncedSearch, page + 1, true)}
                disabled={loading}
              >
                {loading ? loadingMessage : 'Load more'}
              </button>
            </div>
          ) : null}
        </PopoverPrimitive.Content>
      </PopoverPrimitive.Portal>
    </PopoverPrimitive.Root>
  )
}

export { AsyncSelect }
