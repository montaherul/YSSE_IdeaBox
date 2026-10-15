import { DEFAULT_PAGE_SIZE } from '@/config/constants'

/**
 * Applies search, sort and pagination in-memory for small catalogs that the
 * backend returns as a full array (roles, permissions). Mirrors the
 * `{ items, total }` contract expected by ServerDataTable.
 *
 * For large datasets use the server-side listing endpoints instead.
 */
export function paginateCollection(items, params, { searchFields = [] } = {}) {
  const source = Array.isArray(items) ? items : []
  const term = (params?.search ?? '').trim().toLowerCase()

  let rows = term
    ? source.filter((row) =>
        searchFields.some((field) => String(row?.[field] ?? '').toLowerCase().includes(term)),
      )
    : source

  const field = params?.sort
  if (field) {
    const direction = params?.direction === 'asc' ? 1 : -1
    rows = [...rows].sort((a, b) => {
      const left = a?.[field]
      const right = b?.[field]
      if (left == null) return 1
      if (right == null) return -1
      if (typeof left === 'number' && typeof right === 'number') return (left - right) * direction
      return String(left).localeCompare(String(right)) * direction
    })
  }

  const page = params?.page ?? 1
  const pageSize = params?.pageSize ?? DEFAULT_PAGE_SIZE
  const start = (page - 1) * pageSize

  return { items: rows.slice(start, start + pageSize), total: rows.length }
}
