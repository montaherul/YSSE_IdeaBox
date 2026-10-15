import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useDebounce } from '@/hooks/useDebounce'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

/**
 * Generic server-side list hook: page + debounced search + sort + filters.
 *
 * `queryFn(params)` must return `{ items, total, page?, pageSize?, hasNextPage? }`
 * (use `api.getPaged` or `createResourceApi` on the backend paged endpoints).
 */
export function useResourceList({
  queryKey,
  queryFn,
  pageSize = DEFAULT_PAGE_SIZE,
  enabled = true,
  initialSearch = '',
  initialSort = { field: null, direction: null },
  initialFilters = {},
  keepPreviousData = true,
}) {
  const [page, setPage] = useState(1)
  const [search, setSearchState] = useState(initialSearch)
  const [sort, setSort] = useState(initialSort)
  const [filters, setFiltersState] = useState(initialFilters)
  const debouncedSearch = useDebounce(search, 300)

  const params = {
    page,
    pageSize,
    search: debouncedSearch.trim() || undefined,
    sort: sort.field ?? undefined,
    direction: sort.direction ?? undefined,
    ...filters,
  }

  const { data, isLoading, isFetching, isError, refetch } = useQuery({
    queryKey: [...queryKey, params],
    queryFn: () => queryFn(params),
    enabled,
    ...(keepPreviousData ? { placeholderData: (prev) => prev } : {}),
  })

  const items = Array.isArray(data?.items) ? data.items : []
  const total = data?.total ?? 0

  const setSearch = (value) => {
    setSearchState(value)
    setPage(1)
  }
  const setFilters = (next) => {
    setFiltersState((prev) => (typeof next === 'function' ? next(prev) : next))
    setPage(1)
  }
  const setSortState = (next) => {
    setSort((prev) => (typeof next === 'function' ? next(prev) : next))
    setPage(1)
  }

  return {
    items,
    total,
    page,
    pageSize,
    search,
    setSearch,
    sort,
    setSort: setSortState,
    filters,
    setFilters,
    setPage,
    isLoading,
    isFetching,
    isError,
    refetch,
    params,
  }
}

/** Generic single-resource hook. */
export function useResource(id, { queryKey, queryFn, enabled = true, retry = false } = {}) {
  return useQuery({
    queryKey: [...(queryKey ?? []), 'id', id],
    queryFn: () => queryFn(id),
    enabled: enabled && Boolean(id),
    retry,
  })
}

export default useResourceList
