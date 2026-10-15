import api from '@/services/apiClient'

/**
 * Build a thin CRUD client for a REST resource from a base path.
 *
 * Assumed backend shape (override any method via `overrides`):
 *   GET    {base}          -> paged list  ({ items, page, pageSize, total, hasNextPage })
 *   GET    {base}/{id}     -> single item
 *   POST   {base}          -> create
 *   PUT    {base}/{id}     -> update
 *   PATCH  {base}/{id}     -> partial update
 *   DELETE {base}/{id}     -> delete
 *
 * Set `paged: false` for list endpoints that return a plain array
 * (e.g. GET /categories). Extra resource-specific endpoints can be added
 * with `overrides` without duplicating the common CRUD methods.
 */
export function createResourceApi(basePath, { paged = true, overrides = {} } = {}) {
  const base = String(basePath).replace(/\/$/, '')

  return {
    basePath: base,
    list: (params) => (paged ? api.getPaged(base, { params }) : api.get(base, { params })),
    get: (id) => api.get(`${base}/${id}`),
    create: (payload) => api.post(base, payload),
    update: (id, payload) => api.put(`${base}/${id}`, payload),
    patch: (id, payload) => api.patch(`${base}/${id}`, payload),
    remove: (id) => api.delete(`${base}/${id}`),
    ...overrides,
  }
}

export default createResourceApi
